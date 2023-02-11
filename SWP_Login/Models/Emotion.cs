using System;
using System.Collections.Generic;

namespace SWP_Login.Models
{
    public partial class Emotion
    {
        public string Id { get; set; } = null!;
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public bool Like { get; set; }

        public virtual Comment Comment { get; set; } = null!;
    }
}
