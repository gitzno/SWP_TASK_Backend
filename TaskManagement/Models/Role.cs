using System;
using System.Collections.Generic;

namespace TaskManagement.Models;

public partial class Role
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Describe { get; set; } = null!;

    public virtual ICollection<UserSectionRole> UserSectionRoles { get; } = new List<UserSectionRole>();

    public virtual ICollection<UserTaskRole> UserTaskRoles { get; } = new List<UserTaskRole>();

    public virtual ICollection<UserWorkSpaceRole> UserWorkSpaceRoles { get; } = new List<UserWorkSpaceRole>();
}
