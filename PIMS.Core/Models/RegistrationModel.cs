using System;
using System.ComponentModel.DataAnnotations;


namespace PIMS.Core.Models
{
    [Serializable]
    public class RegistrationModel
    {
        //  Data annotation attributes will be used for validating the registration payload request. 
        [Required]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Non-matching passwords found.")]
        public string ConfirmPassword { get; set; }
    }
}
