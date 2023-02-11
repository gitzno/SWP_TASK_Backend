﻿using System;
using System.Collections.Generic;

namespace SWP_Login.Models
{
    public partial class Comment
    {
        public Comment()
        {
            Emotions = new HashSet<Emotion>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public int TaskId { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedTime { get; set; }

        public virtual Task Task { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Emotion> Emotions { get; set; }
    }
}
