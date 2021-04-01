using System;
using System.Collections.Generic;
using System.Linq;
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
        public decimal Balance { get; private set; }
        public ICollection<Transaction> Transactions { get; private set; }
        public DateTime WalletCreatedDateTime { get; private set; }
        public DateTime LastTransactionDateTime { get; private set; }
        public DateTime LastTransferDateTime { get; private set; }
        public decimal UsedDepositThisMonth { get; private set; }
        public decimal UsedWithdrawThisMonth { get; private set; }
        public bool IsBlocked { get; private set; }
        public byte[] RowVersion { get; protected set; }

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
            Balance = 0M;
            Transactions = new List<Transaction>();
            WalletCreatedDateTime = DateTime.Now;
        }

        public bool CheckPassword(string password)
        {
            return _password == password;
        }

        public void PayIn(decimal amount, TransactionType type, string source, decimal maximalDeposit)
        {
            if (UsedDepositThisMonth + amount > maximalDeposit)
            {
                throw new InvalidOperationException($"Transaction would exceed wallet ({Jmbg}) monthly deposit limit ({maximalDeposit} RSD).");
            }

            Balance += amount;

            if (LastTransactionDateTime.Month != DateTime.Now.Month || LastTransactionDateTime.Year != DateTime.Now.Year)
            {
                UsedDepositThisMonth = 0M;
                UsedWithdrawThisMonth = 0M;
            }

            UsedDepositThisMonth += amount;

            var transaction = new Transaction(this, amount, type, source, Jmbg, Balance);

            Transactions.Add(transaction);

            LastTransactionDateTime = DateTime.Now;
        }

        public void PayOut(decimal amount, TransactionType type, string destination, decimal maximalWithdraw)
        {
            if (UsedWithdrawThisMonth + amount > maximalWithdraw)
            {
                throw new InvalidOperationException($"Transaction would exceed wallet ({Jmbg}) monthly withdraw limit ({maximalWithdraw} RSD).");
            }
            if (Balance - amount < 0)
            {
                throw new InvalidOperationException($"Transaction would put wallet ({Jmbg}) Balance under 0.00 RSD).");
            }
            Balance -= amount;

            if (LastTransactionDateTime.Month != DateTime.Now.Month || LastTransactionDateTime.Year != DateTime.Now.Year)
            {
                UsedDepositThisMonth = 0M;
                UsedWithdrawThisMonth = 0M;
            }

            UsedWithdrawThisMonth += amount;


            var transaction = new Transaction(this, amount, type, Jmbg, destination, Balance);

            Transactions.Add(transaction);

            LastTransactionDateTime = DateTime.Now;
            if (type == TransactionType.TransferPayOut)
            {
                LastTransferDateTime = DateTime.Now;
            }
        }

        public void Block()
        {
            if (IsBlocked) {
                throw new InvalidOperationException($"Wallet {Jmbg} is already blocked");
            }
            IsBlocked = true;
        }

        public void Unblock()
        {
            if (!IsBlocked)
            {
                throw new InvalidOperationException($"Wallet {Jmbg} is not blocked");
            }
            IsBlocked = false;
        }


        public void ChangePassword(string oldPassword, string newPassword, string newPasswordConfirmation)
        {
            if (oldPassword != _password)
            {
                throw new ArgumentException("Old password doesn't match.");
            }
            if (newPassword == oldPassword)
            {
                throw new ArgumentException("New password and old password must be different.");
            }
            if (newPassword != newPasswordConfirmation)
            {
                throw new ArgumentException("New password and password confirmation do not match.");
            }
            if (newPassword.Length != 6)
            {
                throw new ArgumentException("New password must be 6 digits long.");
            }
            if (newPassword.Any(c => !char.IsDigit(c)))
            {
                throw new ArgumentException("New password can only contain digits.");
            }

            _password = newPassword;
        }

        public void CheckAndUpdateUsedDepositWithdraw()
        {
            if (LastTransactionDateTime.Month != DateTime.Now.Month || LastTransactionDateTime.Year != DateTime.Now.Year)
            {
                UsedDepositThisMonth = 0M;
                UsedWithdrawThisMonth = 0M;
            }
        }
    }
}
