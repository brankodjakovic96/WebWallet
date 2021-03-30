using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Applications.WebApp.Models
{
    public class WalletTransferVM
    {
        [Required]
        [StringLength(13, ErrorMessage = "The Jmbg value must be 13 characters long. ", MinimumLength = 13)]
        public string JmbgFrom { get; set; }
        [Required]
        public string PasswordFrom { get; set; }
        [Required]
        [StringLength(13, ErrorMessage = "The Jmbg value must be 13 characters long. ", MinimumLength = 13)]
        public string JmbgTo { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public decimal Fee { get; set; }
    }
}
