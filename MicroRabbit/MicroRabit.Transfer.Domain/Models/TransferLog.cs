using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbit.Transfer.Domain.Models
{
    public class TransferLog
    {
        public int Id { get; set; }
        public string FromAccount {get; set;}
        public int ToAccount { get; set; }
        public decimal TransferAmount { get; set; }
    }
}
