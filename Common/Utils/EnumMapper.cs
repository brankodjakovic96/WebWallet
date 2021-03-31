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

    }
}
