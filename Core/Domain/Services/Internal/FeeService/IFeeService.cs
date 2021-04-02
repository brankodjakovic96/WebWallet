using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Services.Internal.FeeService
{
    public interface IFeeService
    {
        decimal CalculateFee(int numberOfDaysAfterCreationWithNoFee, bool firstTransactionFreeEachMonth, decimal fixedFeeLimit, decimal fixedFee, decimal percentageFee, Wallet wallet, decimal amount);
    }
}
