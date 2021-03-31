using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Applications.WebApp.Models.WalletInfo
{
    public class TransactionResposneVM
    {
        public decimal Inflow { get; set; }
        public decimal Outflow { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public decimal WalletBalance { get; set; }
        public DateTime TransactionDateTime { get; set; }
    }
}
