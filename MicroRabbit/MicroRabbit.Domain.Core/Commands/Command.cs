using System;
using System.Collections.Generic;
using System.Text;
using MicroRabbit.Domain.Core.Events;

namespace MicroRabbit.Domain.Core.Commands
{
    //Command is of type message
    //we are gonna have messages sent across the bus
    public abstract class Command : Message
    {
        //Set our timestamp because for each command
        //we should know the exast time
        //Protect set because only ihnerited classes 
        //should be able to set the timestamp
        public DateTime TimeStamp { get; protected set; }

        protected Command()
        {
            //Time stamp is set to the time of execution
            TimeStamp = DateTime.Now;
        }
    }
}
