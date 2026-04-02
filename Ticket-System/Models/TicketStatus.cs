namespace Ticket_System.Models
{
    public static class TicketStatus
    {
        public const string Offen = "Offen";
        public const string InBearbeitung = "In Bearbeitung";
        public const string Geloest = "Gelöst";
        public const string Abgebrochen = "Abgebrochen";

        public static readonly string[] Alle =
        {
            Offen,
            InBearbeitung,
            Geloest,
            Abgebrochen
        };
    }
}