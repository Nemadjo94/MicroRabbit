using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbit.Domain.Core.Events
{
    //IRequest is used from MediatR
    public abstract class Message : IRequest<bool>
    {
        //We can add more msg types like senders name etc...
        public string MessageType { get; protected set; }

        protected Message()
        {
            MessageType = GetType().Name;
        }
    }
}
