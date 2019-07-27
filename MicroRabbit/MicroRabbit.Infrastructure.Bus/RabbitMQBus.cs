using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MicroRabbit.Infrastructure.Bus
{
    public sealed class RabbitMQBus : IEventBus
    {
        //Create our mediator
        private readonly IMediator _mediator;
        //Hold our handlers for all events in dictionary
        private readonly Dictionary<string, List<Type>> _handlers;
        //Create a list of event types
        //We are using generics so we can have all type of events
        private readonly List<Type> _eventTypes;

        //Set up our constructor so we can use dependency injection
        public RabbitMQBus(IMediator mediator)
        {
            _mediator = mediator;
            _handlers = new Dictionary<string, List<Type>>();
            _eventTypes = new List<Type>();//handles all types of event handlers
        }

        public Task SendCommand<T>(T command) where T : Command
        {
            //Use MediatR to send the command
            return _mediator.Send(command);
        }

        public void Publish<T>(T @event) where T : Event
        {
            //ConnectionFactory from RabbitMQ 
            //Establish our connection
            var factory = new ConnectionFactory()
            {
                //Service name
                HostName = "localhost"
            };

            //Create connection
            using (var connection = factory.CreateConnection())
            //Create a model
            using (var channel = connection.CreateModel())
            {
                //Grab the type and name of event
                var eventName = @event.GetType().Name;
                //Queue event
                channel.QueueDeclare(eventName, false, false, false, null);

                //JsonConvert from Newtonsoft.Json
                var message = JsonConvert.SerializeObject(@event);
                var body = Encoding.UTF8.GetBytes(message);//encode the message
                //Publish the message using our channel
                channel.BasicPublish("", eventName, null, body);
            }
        }
        //Takes in an event and event handler
        public void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>
        {
            //Generic types of names and handlers
            var eventName = typeof(T).Name;
            var handlerType = typeof(TH);

            //If we have a new type of event, add it to the list of events
            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }
            //Check dictionary keys for existing event names
            //And add to the list of types
            if (!_handlers.ContainsKey(eventName))
            {
                _handlers.Add(eventName, new List<Type>());
            }
            //Validate the events so that we dont duplicate events
            if(_handlers[eventName].Any(s => s.GetType() == handlerType))
            {
                throw new ArgumentException(
                    $"Handler Type {handlerType.Name} already is registered for '{eventName}'", nameof(handlerType));             
            }
            //Add event to the list
            _handlers[eventName].Add(handlerType);
            //Start the consumer handler to consume the messages
            StartBasicConsume<T>();
        }
        //Consumer method
        private void StartBasicConsume<T>() where T : Event
        {
            //Establish connection
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                DispatchConsumersAsync = true//Async consumer
            };

            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            var eventName = typeof(T).Name;

            channel.QueueDeclare(eventName, false, false, false, null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            //Use delegate to point to event
            consumer.Received += Consumer_Received;
            //Start consumption
            channel.BasicConsume(eventName, true, consumer);
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {   
            //Grab the message from the queue
            var eventName = e.RoutingKey;
            //And convert it
            var message = Encoding.UTF8.GetString(e.Body);

            try
            {
                await ProcessEvent(eventName, message).ConfigureAwait(false);
            }
            catch(Exception exc)
            {

            }
        }
        //
        private async Task ProcessEvent(string eventName, string message)
        {
            if (_handlers.ContainsKey(eventName))
            {
                var subscriptions = _handlers[eventName];
                foreach(var subscription in subscriptions)
                {
                    var handler = Activator.CreateInstance(subscription);
                    if (handler == null) continue;
                    var eventType = _eventTypes.SingleOrDefault(t => t.Name == eventName);
                    var @event = JsonConvert.DeserializeObject(message, eventType);
                    var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                    await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { @event });
                }
            }
        }
    }
}
