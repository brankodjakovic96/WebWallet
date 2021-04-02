using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Services.Internal.FeeService
{
    public class FeeService : IFeeService
    {
        public decimal CalculateFee(int numberOfDaysAfterCreationWithNoFee, bool firstTransactionFreeEachMonth, decimal fixedFeeLimit, decimal fixedFee, decimal percentageFee, Wallet wallet, decimal amount)
        {
            if (wallet.WalletCreatedDateTime.Date.AddDays(numberOfDaysAfterCreationWithNoFee) > DateTime.Now.Date)
            {
                return 0;
            }

            if (firstTransactionFreeEachMonth && (wallet.LastTransferDateTime.Month != DateTime.Now.Month || wallet.LastTransferDateTime.Year != DateTime.Now.Year))
            {
                return 0;
            }

            if (amount < fixedFeeLimit)
            {
                return fixedFee;
            }
            else
            {
                return (amount * percentageFee) / 100M;
            }

        }
    }
}
