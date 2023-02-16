using TaskManagement.Interface;
using TaskManagement.Models;

namespace TaskManagement.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly TaskManagementContext _context;

        public TaskRepository(TaskManagementContext context)
        {
            _context = context;
        }
        public bool CreateTask(int userId, int roleId, Models.Task task)
        {
            var user = _context.Users.SingleOrDefault(o => o.Id == userId); 
            var role = _context.Roles.SingleOrDefault(o => o.Id== roleId);

            var userTaskRole = new UserTaskRole
            {
                RoleId = roleId,
                UserId = userId,
                TaskId = task.Id,
                User = user,
                Role = role,
                Task= task
            };
            _context.UserTaskRoles.Add(userTaskRole);
            _context.Tasks.Add(task);   
            return Save();
        }

        public bool DeleteTask(Models.Task task)
        {
            _context.Tasks.Remove(task);
            return Save();
        }

        public Models.Task GetTaskByID(int taskId)
        => _context.Tasks.SingleOrDefault(o => o.Id == taskId);

        public ICollection<Models.Task> GetTasks()
        => _context.Tasks.ToList();

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0? true: false;
        }

        public bool TaskExists(int taskId)
        => _context.Tasks.Any(o => o.Id == taskId);

        public bool UpdateTask(Models.Task task)
        {
            _context.Tasks.Update(task);
            return Save();
        }
    }
}
