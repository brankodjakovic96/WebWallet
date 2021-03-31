using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Applications.WebApp.Models
{
    public class WalletInfoResponseVM
    {
        public string Jmbg { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BankAccount { get; set; }
        public string BankType { get; set; }
        public decimal Balance { get; set; }
        public decimal UsedDepositThisMonth { get; set; }
        public decimal MaximalDeposit { get; set; }
        public decimal UsedWithdrawThisMonth { get; set; }
        public decimal MaximalWithdraw { get; set; }
    }
}
