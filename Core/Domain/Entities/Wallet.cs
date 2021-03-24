using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Entities
{
    public class Wallet
    {
        public string Jmbg { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string PIN { get; private set; }
        public string BankAccount { get; private set; }
        public decimal Ballance { get; set; }
        public IEnumerable<Transaction> Transactions { get; private set; }
        public DateTime LastTransactionDateTime { get; set; }
        public decimal UsedDepositThisMonth { get; set; }
        public decimal UsedWithdrawThisMonth { get; set; }
        public Wallet()
        {
            Transactions = new List<Transaction>();
        }

        public Wallet(string firstName, string lastName, string jmbg, string pin, string bankAccount)
        {
            FirstName = firstName;
            Jmbg = jmbg;
            PIN = pin;
            BankAccount = bankAccount;
            Ballance = 0M;
            Transactions = new List<Transaction>();
        }
    }
}
