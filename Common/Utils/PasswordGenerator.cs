using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Common.Utils
{
    public static class PasswordGenerator
    {
        public static string Generate(int length)
        {
            if (length < 1 || length > 128)
            {
                throw new ArgumentException(nameof(length));
            }

            using (var rng = RandomNumberGenerator.Create())
            {
                var byteBuffer = new byte[length];

                rng.GetBytes(byteBuffer);

                var count = 0;
                var characterBuffer = new char[length];

                for (var iter = 0; iter < length; iter++)
                {
                    var i = byteBuffer[iter] % 10;

                    characterBuffer[iter] = (char)('0' + i);
                }          

                return new string(characterBuffer);
            }
        }
    }
}
