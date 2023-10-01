using System;
using System.Collections.Generic;

namespace TaskManagement.Models;

public partial class Section
{
    public int Id { get; set; }

    public int WorkSpaceId { get; set; }

    public string Title { get; set; } = null!;

    public string? Describe { get; set; }

    public DateTime CreatedTime { get; set; } = DateTime.Now;

    public bool Status { get; set; }

    public virtual ICollection<Task> Tasks { get; } = new List<Task>();

    public virtual ICollection<UserSectionRole> UserSectionRoles { get; } = new List<UserSectionRole>();

    public virtual WorkSpace WorkSpace { get; set; } = null!;
}
