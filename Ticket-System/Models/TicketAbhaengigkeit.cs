namespace Ticket_System.Models
{
    public class TicketAbhaengigkeit
    {
        public int BlockiertesTicketId { get; set; }
        public Ticket BlockiertesTicket { get; set; } = null!;

        public int BlockierendesTicketId { get; set; }
        public Ticket BlockierendesTicket { get; set; } = null!;

        public enum TicketStatus
        {
            Offen,
            InBearbeitung,
            Gelöst,
            Abgebrochen
        }
    }
}
