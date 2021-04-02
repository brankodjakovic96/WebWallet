using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Applications.WebApp.Models
{
    public class ChangePasswordVM
    {
        [Required]
        [StringLength(13, ErrorMessage = "The Jmbg value must be 13 characters long. ", MinimumLength = 13)]
        public string Jmbg { get; set; }
        [Required]
        public string OldPassword { get; set; }
        [Required]
        [StringLength(6, ErrorMessage = "The NewPassword value must be 6 characters long. ", MinimumLength = 6)]
        public string NewPassword { get; set; }
        [Required]
        [StringLength(6, ErrorMessage = "The NewPasswordConfirmation value must be 6 characters long. ", MinimumLength = 6)]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords don't match.")]
        public string NewPasswordConfirmation { get; set; }
    }
}
