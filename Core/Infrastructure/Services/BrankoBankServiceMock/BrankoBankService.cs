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
                { "0605996781028:1234", 100000M }
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
    }
}
