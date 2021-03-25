using Core.Domain.Services.Internal.BankRoutingService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Services.External.BankService
{
    public interface IBankService
    {
        Task<BankResponse> CheckStatus(string jmbg, string pin);
    }
}
