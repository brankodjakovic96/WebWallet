using System.ComponentModel.DataAnnotations;

namespace Applications.WebApp.Models
{
    public class WalletDepositVM
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
