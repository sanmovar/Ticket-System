using System.ComponentModel.DataAnnotations;

namespace Ticket_System.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-Mail ist erforderlich")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Passwort ist erforderlich")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
