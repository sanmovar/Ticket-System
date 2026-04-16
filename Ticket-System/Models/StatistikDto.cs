namespace Ticket_System.Models
{
    public class StatistikDto
    {
        // Ticket-Statistiken
        public int TicketsGesamt { get; set; }
        public int TicketsOffen { get; set; }
        public int TicketsGeschlossen { get; set; }

        // Projekt-Statistiken
        public int ProjekteGesamt { get; set; }
        public int ProjekteAktiv { get; set; }
        public int ProjekteBeendet { get; set; }

        // Benutzer-Statistiken
        public int BenutzerGesamt { get; set; }
        public int AnzahlAdmins { get; set; }
        public int AnzahlDeveloper { get; set; }
        public int AnzahlTester { get; set; }
    }
}
