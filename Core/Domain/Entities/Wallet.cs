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
        public BankType BankType { get; private set; }
        public decimal Ballance { get; private set; }
        public IEnumerable<Transaction> Transactions { get; private set; }
        public DateTime LastTransactionDateTime { get; private set; }
        public decimal UsedDepositThisMonth { get; private set; }
        public decimal UsedWithdrawThisMonth { get; private set; }
        private string _password;
        public Wallet()
        {
            Transactions = new List<Transaction>();
        }

        public Wallet(string firstName, string lastName, string jmbg, BankType bankType, string pin, string bankAccount, string password)
        {
            FirstName = firstName;
            LastName = lastName;
            Jmbg = jmbg;
            BankType = bankType;
            PIN = pin;
            BankAccount = bankAccount;
            _password = password;
            Ballance = 0M;
            Transactions = new List<Transaction>();
        }
    }
}
