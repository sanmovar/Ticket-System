using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Ticket_System.Models
{
    public class Comment
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "Der Inhalt des Kommentars ist erforderlich.")]
        [StringLength(1000, ErrorMessage = "Der Inhalt des Kommentars darf nicht länger als 1000 Zeichen sein.")]
        public string Inhalt { get; set; } = string.Empty;

        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        public string ErstellerId { get; set; } = string.Empty;
        public IdentityUser? Ersteller { get; set; }

        public DateTime Erstellzeitpunkt { get; set; }

    }
}
