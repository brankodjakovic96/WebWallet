using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Entities
{
    public enum TransactionType : byte
    {
        Undefined = 0,
        Withdraw = 1,
        Deposit = 2,
        TransferPayIn = 3,
        TransferPayOut = 4,
        FeePayOut = 5
    }
}
