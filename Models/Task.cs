namespace TaskManagerAPI.Models
{
    public class Task
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public required string Status { get; set; }
        public int UserId { get; set; }
    }
}
