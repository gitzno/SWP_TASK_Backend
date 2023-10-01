using System;
using System.Collections.Generic;

namespace TaskManagement.Models;

public partial class User
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Name { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Work { get; set; }

    public string? Image { get; set; }

    public virtual ICollection<Comment> Comments { get; } = new List<Comment>();

    public virtual ICollection<UserSectionRole> UserSectionRoles { get; } = new List<UserSectionRole>();

    public virtual ICollection<UserTaskRole> UserTaskRoles { get; } = new List<UserTaskRole>();

    public virtual ICollection<UserWorkSpaceRole> UserWorkSpaceRoles { get; } = new List<UserWorkSpaceRole>();
}
