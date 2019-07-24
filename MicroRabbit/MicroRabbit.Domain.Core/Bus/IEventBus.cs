using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;

namespace MicroRabbit.Domain.Core.Bus
{

    public interface IEventBus
    {
        //Send our commands through the bus
        //Restricted to type of Command
        Task SendCommand<T>(T command) where T : Command;

        //Punlish any type of event
        //Restricted to type of Event
        void Publish<T>(T @event) where T : Event;

        //We have services that are publishing events
        //But we also want services to be able to 
        //Subscribe to events

        //Allow services to subscribe
        //We have event and event handler
        //Restricted to type of Event and IEventHandler
        void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>;

    }
}
