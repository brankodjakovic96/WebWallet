using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Applications.WebWallet.Models
{
    public class WalletVM
    {
        [Required]
        [StringLength(13, ErrorMessage = "The Jmbg value must be 13 characters long. ", MinimumLength = 13)]
        public string Jmbg { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [StringLength(4, ErrorMessage = "The PIN value must be 4 characters long. ", MinimumLength = 4)]
        public string PIN { get; set; }
        [Required]
        [StringLength(18, ErrorMessage = "The BankAccount value must not be longer than 18 characters. ")]
        public string BankAccount { get; set; }
        [Required]
        public short BankType { get; set; }
        public string Password { get; set; }
    }
}
