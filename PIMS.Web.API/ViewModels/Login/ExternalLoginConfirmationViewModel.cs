using System.ComponentModel.DataAnnotations;

namespace PIMS.Web.Api.ViewModels.Login
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }






}