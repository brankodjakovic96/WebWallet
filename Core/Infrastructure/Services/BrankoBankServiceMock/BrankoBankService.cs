using Core.Domain.Services.External.BankService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Infrastructure.Services.BrankoBankServiceMock
{
    public class BrankoBankService : IBrankoBankService
    {
        private readonly Dictionary<string, decimal> Accounts;

        public BrankoBankService()
        {
            Accounts = new Dictionary<string, decimal>()
            {
                { "0605996781029:1234", 1000000M },
                { "0605996781028:1234", 100000M },
                { "0605996781027:1234", 2000000M },

            };
        }

        public async Task<BankResponse> CheckStatus(string jmbg, string pin)
        {
            var status = Accounts.ContainsKey($"{jmbg}:{pin}");
            var response = new BankResponse()
            {
                Status = status,
                ErrorCodes = status ? "" : "Bank account not found for given jmbg and pin!"
            };
            return response;
        }

        public async Task<BankResponse> Deposit(string jmbg, string pin, decimal amount)
        {
            var status = Accounts.ContainsKey($"{jmbg}:{pin}");
            if(status)
            {
                Accounts[$"{jmbg}:{pin}"] += amount;
            }
            var response = new BankResponse()
            {
                Status = status,
                ErrorCodes = status ? "" : "Bank account not found for given jmbg and pin!"
            };
            return response;
        }

        public async Task<BankResponse> Withdraw(string jmbg, string pin, decimal amount)
        {
            decimal balance;
            var status = Accounts.TryGetValue($"{jmbg}:{pin}", out balance);
            string error = "";
            if (!status)
            {
                error = "Bank account not found for given jmbg and pin!";
            }
            else if (balance < amount)
            {
                status = false;
                error = "Bank account doesn't have enoguh funds!";
            }
            else
            {
                Accounts[$"{jmbg}:{pin}"] -= amount;
            }

            var response = new BankResponse()
            {
                Status = status,
                ErrorCodes = error
            };

            return response;
        }
    }
}
