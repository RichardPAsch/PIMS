using System.ComponentModel.DataAnnotations;


namespace PIMS.Core.Models.ViewModels
{
    public class ImportFileVm
    {
        [Required]
        public string ImportFilePath { get; set; }

        [Required]
        public bool IsRevenueData { get; set; }

    }
}
