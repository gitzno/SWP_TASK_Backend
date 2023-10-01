using System;
using System.Collections.Generic;

namespace TaskManagement.Models;

public partial class WorkSpace
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Describe { get; set; } = null!;

    public DateTime CreatedTime { get; set; } = DateTime.Now;

    public virtual ICollection<Section> Sections { get; } = new List<Section>();

    public virtual ICollection<UserWorkSpaceRole> UserWorkSpaceRoles { get; } = new List<UserWorkSpaceRole>();
}
