namespace TaskManagement.Dto
{
    public class TaskDto
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string Title { get; set; } = null!;
        public string? Describe { get; set; }
        public string? Image { get; set; }
        public bool Status { get; set; }
        public DateTime? TaskTo { get; set; }
        public DateTime? TaskFrom { get; set; }
        public bool PinTask { get; set; }
        public string? Tag { get; set; }
        public string? Attachment { get; set; }
    }
}
