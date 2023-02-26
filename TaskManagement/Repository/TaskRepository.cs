using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using TaskManagement.Dto;
using TaskManagement.Interface;
using TaskManagement.Models;
using TaskManagement.Utils;
using static System.Collections.Specialized.BitVector32;

namespace TaskManagement.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly TaskManagementContext _context;
        private readonly IMapper _mapper;


        public static class Status
        {
            public const string Success = "200";
            public const string NotFound = "404";
            public const string BadRequest = "400";
            public const string Created = "201";
            public const string NoContent = "204";
        }

        public static class Message
        {
            public const string Success = "Request processed successfully";
            public const string NotFound = "Not found";
            public const string BadRequest = "Bad request";
            public const string Created = "Resource created successfully";
            public const string NoContent = "No Content";
        }
        public TaskRepository(TaskManagementContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        public Models.Task GetTaskByID(int taskId)
        => _context.Tasks.SingleOrDefault(o => o.Id == taskId);

        public ResponseObject GetTasks()
        {
            var tasks = _context.Tasks.ToList();
            var taskMap = _mapper.Map<List<TaskDto>>(tasks);

            if (tasks == null)
                return new ResponseObject
                {
                    Status = "400",
                    Message = "Bad request",
                    Data = null,
                };
            else
                return new ResponseObject
                {
                    Status = "200",
                    Message = "Successfully",
                    Data = taskMap,
                };
        }

        public ResponseObject GetAssignedTasksForUser(int workspaceID, int userID)
        {
            var tasks = _context.UserTaskRoles.Where(utr => utr.UserId == userID && utr.RoleId == 2)
            .Select(t => t.Task).Where(t => t.Section.WorkSpaceId == workspaceID).ToList();
            var tasksMap = _mapper.Map<List<TaskDto>>(tasks);
            if (tasks != null)
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = tasksMap
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound,
                    Data = null
                };
            }
        }

        public ResponseObject GetUnassignedTasks(int workspaceID)
        {
            var tasksAll = _context.Tasks.Where(t => t.Section.WorkSpaceId == workspaceID).ToList();
            var tasksAssign = _context.UserTaskRoles.Where(utr => utr.RoleId == 2)
            .Select(t => t.Task).Where(t => t.Section.WorkSpaceId == workspaceID).ToList();

            var tasks = tasksAll.Except(tasksAssign);
            var tasksMap = _mapper.Map<List<TaskDto>>(tasks);
            if (tasks != null)
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = tasksMap
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound,
                    Data = null
                };
            }
        }

        public ResponseObject GetTaskCountOfUser(int workspaceID, int userId)
        {
            var tasksCount = _context.UserTaskRoles.Where(utr => utr.UserId == userId && utr.RoleId == 2)
            .Select(t => t.Task).Where(t => t.Section.WorkSpaceId == workspaceID).Count();

            if (tasksCount != 0)
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = tasksCount,
                };
            } else
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound,
                    Data = null
                };
            }
        }

        public ResponseObject GetUserCompletedTaskCount(int workspaceID, int userId, bool status)
        {
            var numberOfTaskDone = _context.UserTaskRoles.Where(utr => utr.UserId == userId && utr.RoleId == 2)
            .Select(t => t.Task).Where(t => t.Section.WorkSpaceId == workspaceID && t.Status == status).ToList();
            
            if (numberOfTaskDone == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = "No task done",
                    Data = null,
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = numberOfTaskDone,
                };
            }
        }


        public ResponseObject CreateTask(int? sectionID, int userId, int roleId, Models.Task task)
        {
            var user = _context.Users.SingleOrDefault(o => o.Id == userId);
            var role = _context.Roles.SingleOrDefault(o => o.Id == roleId);
            if (user == null)
                return new ResponseObject
                {
                    Status = "404",
                    Message = "Not found user",
                    Data = null
                };
            if (role == null)
                return new ResponseObject
                {
                    Status = "404",
                    Message = "Not found role",
                    Data = null
                };


            var userTaskRole = new UserTaskRole
            {
                RoleId = roleId,
                UserId = userId,
                TaskId = task.Id,
                User = user,
                Role = role,
                Task = task
            };
            _context.Add(userTaskRole);
            _context.Tasks.Add(task);
            var taskMap = _mapper.Map<TaskDto>(task);
            if (Save())
                return new ResponseObject
                {
                    Status = "200",
                    Message = "Task created",
                    Data = taskMap
                };
            else
            {
                return new ResponseObject
                {
                    Status = "400",
                    Message = "Bad request"
                };
            }

        }


        public ResponseObject UpdateTask(Models.Task task)
        {

            _context.Tasks.Update(task);
            var taskMap = _mapper.Map<TaskDto>(task);

            if (Save())
            {
                return new ResponseObject
                {
                    Status = "200",
                    Message = "successfully",
                    Data = taskMap
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = "400",
                    Message = "Bad request",
                    Data = null
                };
            }

        }
        public ResponseObject DeleteTask(Models.Task task)
        {

            _context.Remove(task);
            if (Save())
            {
                return new ResponseObject
                {
                    Status = "204",
                    Message = "Successfully",
                    Data = null

                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = "400",
                    Message = "Bad request"
                };
            }
        }



        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }



        public ICollection<Models.Task> TasksFilter(int workSpaceID, int? day, bool? done, int? numMenber)
        {

            var tasks = _context.Tasks.Where(t => t.Section.WorkSpaceId == workSpaceID)
                .ToList();


            if (day.HasValue)
            {
                tasks = tasks.Where(t => t.TaskTo != null && CalculateDay(t.TaskTo) > 0 && CalculateDay(t.TaskTo) <= day.Value).ToList();
            }
            if (done.HasValue)
            {
                tasks = tasks.Where(t => t.Status == done.Value).ToList();
            }
            if (numMenber.HasValue)
            {
                tasks = tasks.Where(t => CountMemberInTask(t.Id) < numMenber.Value).ToList();
            }

            return tasks;
        }

        private int CalculateDay(DateTime? dateTime)
        {
            if (dateTime.HasValue)
            {
                TimeSpan timeSince = (TimeSpan)(dateTime - DateTime.Now);
                return timeSince.Days;
            }
            return 0;
        }

        private int CountMemberInTask(int taskID)
        {
            return _context.UserTaskRoles.Where(t => t.TaskId == taskID && t.RoleId != 1).Count();
        }


    }
}
