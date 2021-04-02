using Core.Domain.Entities;
using Core.Domain.Services.External.BankService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Services.Internal.BankRoutingService
{
    public interface IBankRoutingService
    {
        Task<BankResponse> CheckStatus(string jmbg, string pin, BankType bankType);
        Task<BankResponse> Withdraw(string jmbg, string pin, decimal amount, BankType bankType);
        Task<BankResponse> Deposit(string jmbg, string pin, decimal amount, BankType bankType);

    }
}
