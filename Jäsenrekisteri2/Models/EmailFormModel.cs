using System.ComponentModel.DataAnnotations;

namespace MVCEmail.Models
{
    public class EmailFormModel
    {
        [Required]
        public string Message { get; set; }
    }
}