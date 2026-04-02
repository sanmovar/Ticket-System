namespace Ticket_System.Models
{
    public class TicketFilterDto
    {
        public int? ProjektId { get; set; }
        public string? ZugewiesenerBenutzerId { get; set; }
        public string? ErstellerId { get; set; }
        public string? Status { get; set; } 
        public int PageSize { get; set; } = 10;
        public int Page { get; set; } = 1;
        public int TotalCount { get; set; }
        public IEnumerable<Ticket> Tickets { get; set; } = new List<Ticket>();

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPrev => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
}