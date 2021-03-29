using Core.Domain.Entities;
using Core.Domain.Services.External.BankService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Services.Internal.BankRoutingService
{
    public class BankRoutingService : IBankRoutingService
    {
        private IBrankoBankService BrankoBankService;
        public BankRoutingService(IBrankoBankService brankoBankService)
        {
            BrankoBankService = brankoBankService;
        }
        public async Task<BankResponse> CheckStatus(string jmbg, string pin, BankType bankType)
        {
            switch (bankType)
            {
                case BankType.BrankoBank:
                    var response = await BrankoBankService.CheckStatus(jmbg, pin);
                    return response;
                default:
                    throw new ArgumentException("Bank type not supported!");
            }
        }

        public async Task<BankResponse> Withdraw(string jmbg, string pin, decimal amount, BankType bankType)
        {
            switch (bankType)
            {
                case BankType.BrankoBank:
                    var response = await BrankoBankService.Withdraw(jmbg, pin, amount);
                    return response;
                default:
                    throw new ArgumentException("Bank type not supported!");
            }
        }

        public async Task<BankResponse> Deposit(string jmbg, string pin, decimal amount, BankType bankType)
        {
            switch (bankType)
            {
                case BankType.BrankoBank:
                    var response = await BrankoBankService.Deposit(jmbg, pin, amount);
                    return response;
                default:
                    throw new ArgumentException("Bank type not supported!");
            }
        }
    }
}
