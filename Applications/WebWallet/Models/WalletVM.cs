using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Applications.WebWallet.Models
{
    public class WalletVM
    {
        public string Jmbg { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PIN { get; set; }
        public string BankAccount { get; set; }
        public short BankType { get; set; }
    }
}
