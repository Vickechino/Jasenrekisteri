using System.ComponentModel.DataAnnotations;

namespace MVCEmail.Models
{
    public class EmailFormModel {
        public string Message { get; set; }
        public int VerificationCode { get; set; }
    }
}