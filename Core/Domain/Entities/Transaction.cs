﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; private set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; private set; }
        public string Source { get; private set; }
        public string Destination { get; private set; }
        public DateTime TransactionDateTime { get; private set; }
        public string WalletJmbg { get; set; }
        public Wallet Wallet { get; set; }
        public Transaction()
        {

        }

        public Transaction(Wallet wallet, decimal amount, TransactionType type, string source, string destination)
        {
            Wallet = wallet;
            Amount = amount;
            Type = type;
            Source = source;
            Destination = destination;
            TransactionDateTime = DateTime.Now;
        }

    }
}
