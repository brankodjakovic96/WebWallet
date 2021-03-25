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
                    return await BrankoBankService.CheckStatus(jmbg, pin);
                    break;
                default:
                    throw new ArgumentException("Bank type not supported!");
            }
        }
    }
}
