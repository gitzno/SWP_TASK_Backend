using System;
using System.Collections.Generic;

namespace SWP_Login.Models
{
    public partial class Task
    {
        public Task()
        {
            Comments = new HashSet<Comment>();
            UserTaskRoles = new HashSet<UserTaskRole>();
        }

        public int Id { get; set; }
        public int SectionId { get; set; }
        public int UserCreatedId { get; set; }
        public string Title { get; set; } = null!;
        public string? Describe { get; set; }
        public string? Image { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? TaskTo { get; set; }
        public DateTime? TaskFrom { get; set; }
        public bool PinTask { get; set; }
        public string? Tag { get; set; }
        public string? Attachment { get; set; }

        public virtual Section Section { get; set; } = null!;
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<UserTaskRole> UserTaskRoles { get; set; }
    }
}
