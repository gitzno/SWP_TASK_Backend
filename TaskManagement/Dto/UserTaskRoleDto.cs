namespace TaskManagement.Dto
{
    public class UserTaskRoleDto
    {
        public int UserId { get; set; }

        public int TaskId { get; set; }

        public int RoleId { get; set; }

        public bool? Status { get; set; }

        public bool? PinTask { get; set; }
    }
}
