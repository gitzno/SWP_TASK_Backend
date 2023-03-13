using System;
using System.Collections.Generic;

namespace TaskManagement.Models;

public partial class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Task> Tasks { get; } = new List<Task>();
}
