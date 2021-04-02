using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Domain.Services.External.BankService
{
    public class BankResponse
    {
        public bool Status { get; set; }
        public string ErrorCodes { get; set; }
    }
}
