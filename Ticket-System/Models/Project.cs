using System.ComponentModel.DataAnnotations;

namespace Ticket_System.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Titel ist erforderlich")]
        [StringLength(100)]
        public string? Titel { get; set; }

        [Required(ErrorMessage = "Beschreibung ist erforderlich")]
        public string? Beschreibung { get; set; }

        [Required(ErrorMessage = "Startdatum ist erforderlich")]
        [DataType(DataType.Date)]
        public DateTime Startdatum { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Enddatum { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}