using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Applications.WebApp.Models
{
    public class WalletAdministrationVM
    {
        [Required]
        [StringLength(13, ErrorMessage = "The Jmbg value must be 13 characters long. ", MinimumLength = 13)]
        public string Jmbg{ get; set; }
        [Required]
        public string AdminPassword { get; set; }
        [Required]
        public string Action { get; set; }
    }
}
