using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utils
{
    public static class EnumMapper
    {
        public static string MapBankType(short bankType)
        {
            string result = bankType switch
            {
                0 => "Undefined",
                1 => "BrankoBank",             
                _ => throw new ArgumentException("Bank type not defined.")
            };

            return result;
        }
        public static string MapTransactionType(short transactionType)
        {
            string result = transactionType switch
            {
                0 => "Undefined",
                1 => "Withdraw",
                2 => "Deposit",
                3 => "Transfer inflow",
                4 => "Transfer outflow",
                5 => "Fee",
                _ => throw new ArgumentException("Transaction type not defined.")
            };

            return result;
        }

        public static string MapTransactionTypeFlow(short transactionType)
        {
            string result = transactionType switch
            {
                0 => "Undefined",
                1 => "Outflow",
                2 => "Inflow",
                3 => "Inflow",
                4 => "Outflow",
                5 => "Outflow",
                _ => throw new ArgumentException("Transaction type not defined.")
            };

            return result;
        }
    }
}
