using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Applications.WebApp.Models
{
    public class CalculateFeeVM
    {
        [Required]
        [StringLength(13, ErrorMessage = "The Jmbg value must be 13 characters long. ", MinimumLength = 13)]
        public string Jmbg { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public decimal Amount { get; set; }
    }
}
