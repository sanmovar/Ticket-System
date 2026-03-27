using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ticket_System.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Der Titel ist erforderlich.")]
        [StringLength(200, ErrorMessage = "Der Titel darf maximal 200 Zeichen lang sein.")]
        public string? Titel { get; set; }

        [StringLength(1000, ErrorMessage = "Die Beschreibung darf maximal 1000 Zeichen lang sein.")]
        [Required(ErrorMessage = "Die Beschreibung ist erforderlich.")]
        public string? Beschreibung { get; set; }

        [Required(ErrorMessage = "Projekt ist erforderlich.")]
        public int ProjektId { get; set; }

        [ForeignKey("ProjektId")]
        public Project? Projekt { get; set; }

        [Required]
        public string ErstellerId { get; set; } = string.Empty;

        [ForeignKey("ErstellerId")]
        public IdentityUser? Ersteller { get; set; }

        public DateTime ErstelltAm { get; set; } = DateTime.Now;

        public string? ZugewiesenerBenutzerId { get; set; }

        [ForeignKey("ZugewiesenerBenutzerId")]
        public IdentityUser? ZugewiesenerBenutzer { get; set; }

        public DateTime? ZugewiesenAm { get; set; }

        public string? GeschlossenVonId { get; set; }

        [ForeignKey("GeschlossenVonId")]
        public IdentityUser? GeschlossenVon { get; set; }

        public DateTime? GeschlossenAm { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();


    }
}
