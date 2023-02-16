using TaskManagement.Models;
using Task = TaskManagement.Models.Task;

namespace TaskManagement.Interface
{
    public interface ITaskRepository
    {   
       
        ICollection<Task> GetTasks();
        Task GetTaskByID(int taskId);
        bool TaskExists(int taskId);
        bool CreateTask(int userId,int roleId,Task task);
        bool UpdateTask(Task task);
        bool DeleteTask(Task task);
        bool Save();
    }
}
