using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ApplicationServices.DTOs
{
    public class TransactionDTO
    {
        public decimal Amount { get; set; }
        public short Type { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public decimal WalletBalance { get; set;  }
        public DateTime TransactionDateTime { get; set; }
    }
}
