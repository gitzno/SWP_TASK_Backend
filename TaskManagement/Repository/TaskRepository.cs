using AutoMapper;
using AutoMapper.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using TaskManagement.Dto;
using TaskManagement.Interface;
using TaskManagement.Models;
using TaskManagement.Service;
using TaskManagement.Utils;


namespace TaskManagement.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private readonly TaskManagementContext _context;
        private readonly IMapper _mapper;


        
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
                    Status = Status.BadRequest,
                    Message = Message.BadRequest,
                    Data = null,
                };
            else
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
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

        public ResponseObject GetTaskCountUserCompleted(int workspaceID, int userId, bool status)
        {
            var numberOfTaskDone = _context.UserTaskRoles.Where(utr => utr.UserId == userId && utr.RoleId == 2)
            .Select(t => t.Task).Where(t => t.Section.WorkSpaceId == workspaceID && t.Status == status).ToList();
            
            if (numberOfTaskDone == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound,
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

        public ResponseObject GetOngoingTasksInDay(int userID, DateTime date)
        {
            var ondoingTasks = _context.UserTaskRoles.Where(utr => utr.UserId == userID).Select(o => o.Task).ToList();

            ondoingTasks = _context.Tasks.Where(t => t.TaskFrom >= date && t.TaskTo <= date) .ToList();


            return new ResponseObject
            {
                Status = Status.Success,
                Message = Message.Success,
                Data = ondoingTasks
            };
        }


        public ResponseObject CreateTask(int? sectionID, int userId, int roleId, Models.Task task)
        {
            var user = _context.Users.SingleOrDefault(o => o.Id == userId);
            var role = _context.Roles.SingleOrDefault(o => o.Id == roleId);


            if(task.TaskFrom >= task.TaskTo)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest +" TaskTo must be bigger than TaskFrom",
                    Data = null
                };
            }
            if (user == null|| role == null)
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound,
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
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = taskMap
                };
            else
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest
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
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = taskMap
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest,
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
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = null

                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest
                };
            }
        }



        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }



       

        public ResponseObject AddMemberIntoTask(int taskID, int userID, int roleID)
        {
            var task = _context.Tasks.Where(o => o.Id == taskID).FirstOrDefault();
            var user = _context.Users.Where(o => o.Id == userID).FirstOrDefault();
            var role = _context.Roles.Where(o => o.Id == roleID).FirstOrDefault();

            if (user == null || task == null || role == null)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest,
                    Data = null,
                };
            }

            var addUserTask = new UserTaskRole
            {
                TaskId = taskID,
                RoleId = roleID,
                UserId = userID,
            };
            _context.UserTaskRoles.Add(addUserTask);
            var userMap = _mapper.Map<UserDto>(user);
            if (Save())
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = userMap
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest,
                    Data = null
                };
            }
        }

        public ResponseObject GetTasksInSection(int sectionId)
        {
            var task = _context.Tasks.Where(o => o.SectionId == sectionId).ToList();
            var taskMap = _mapper.Map<List<TaskDto>>(task);
            if (task == null)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest,
                    Data = null
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = taskMap
                };
            }
        }

        public ResponseObject GetOngoingTasksInRangeTime(int userID,DateTime startDate, DateTime endDate)
        {
            var ondoingTasks = _context.UserTaskRoles.Where(utr => utr.UserId == userID).Select(o => o.Task).ToList();

             ondoingTasks = _context.Tasks.Where(t =>(t.TaskFrom == null || t.TaskFrom <= endDate) && 
                                                    (t.TaskTo == null || t.TaskTo >= startDate))    
                                           .ToList();

            

            return new ResponseObject
            {
                Status = Status.Success,
                Message = Message.Success,
                Data = ondoingTasks
            };
        }

        public ResponseObject UpdateImage(int taskID, string file)
        {
            var task = _context.Tasks.Where(o => o.Id == taskID).FirstOrDefault();
            if (task != null)
            {
                task.Image = file;
                _context.Update(task);
                var taskMap = _mapper.Map<TaskDto>(task);
                if (Save())
                {
                    return new ResponseObject
                    {
                        Status = Status.Success,
                        Message = Message.Success,
                        Data = taskMap
                    };
                }
                else
                {
                    return new ResponseObject
                    {
                        Status = Status.BadRequest,
                        Message = Message.BadRequest,
                        Data = null
                    };
                }
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

        public ResponseObject MoveTask(int taskID, int targetSectionID)
        {
            
            var taskToMove = _context.Tasks.FirstOrDefault(t => t.Id == taskID);

            if (taskToMove == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound,
                    Data = null
                };
            }

            // Tìm section cần di chuyển task tới
            var targetSection = _context.Sections.FirstOrDefault(s => s.Id == targetSectionID);

            if (targetSection == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound,
                    Data = null
                };
            }

            // Lấy ra danh sách task trong section hiện tại
            var currentSectionTasks = _context.Tasks.Where(t => t.SectionId == taskToMove.SectionId).ToList();

            // Kiểm tra nếu task có trong section hiện tại thì xóa nó
            if (currentSectionTasks.Contains(taskToMove))
            {
                currentSectionTasks.Remove(taskToMove);
            }

            // Thêm task vào section mới
            targetSection.Tasks.Add(taskToMove);
            taskToMove.SectionId = targetSectionID;

            if (Save())
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = null
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest,
                    Data = null
                };
            }
        }

        public ResponseObject TaskFilter(List<Models.Task> tasks, int? userId, DateTime? fromDate, DateTime? toDate, bool? isCompleted, bool? isDueTomorrow, bool? isDueNextWeek, bool? isDueNextMonth)
        {

            // Chỉ định cho thành viên nào
            if (userId != null)
            {
                tasks = tasks.Where(t => t.UserTaskRoles.Any(utr => utr.UserId == userId)).ToList();
            }

            // Các task đã được giao cho tôi
            if (userId != null && !isCompleted.HasValue)
            {
                tasks = tasks.Where(t => t.UserTaskRoles.Any(utr => utr.UserId == userId)).ToList();
            }

            // Không có ngày hết hạn
            if (!fromDate.HasValue && !toDate.HasValue && !isCompleted.HasValue && !isDueTomorrow.HasValue && !isDueNextWeek.HasValue && !isDueNextMonth.HasValue)
            {
                tasks = tasks.Where(t => !t.TaskTo.HasValue).ToList();
            }

            // Quá hạn
            if (isCompleted.HasValue && !isCompleted.Value && !isDueTomorrow.HasValue && !isDueNextWeek.HasValue && !isDueNextMonth.HasValue)
            {
                tasks = tasks.Where(t => t.TaskTo.HasValue && t.TaskTo.Value < DateTime.Now).ToList();
            }

            // Sẽ hết hạn vào ngày mai
            if (!isCompleted.HasValue && isDueTomorrow.HasValue && isDueTomorrow.Value)
            {
                tasks = tasks.Where(t => t.TaskTo.HasValue && t.TaskTo.Value.Date == DateTime.Today.AddDays(1)).ToList();
            }

            // Sẽ hết hạn vào tuần sau
            if (!isCompleted.HasValue && isDueNextWeek.HasValue && isDueNextWeek.Value)
            {
                tasks = tasks.Where(t => t.TaskTo.HasValue && t.TaskTo.Value.Date > DateTime.Today.AddDays(1) && t.TaskTo.Value.Date <= DateTime.Today.AddDays(7)).ToList();
            }

            // Sẽ hết hạn vào tháng sau
            if (!isCompleted.HasValue && isDueNextMonth.HasValue && isDueNextMonth.Value)
            {
                tasks = tasks.Where(t => t.TaskTo.HasValue && t.TaskTo.Value.Date > DateTime.Today.AddDays(7) && t.TaskTo.Value.Date <= DateTime.Today.AddMonths(1)).ToList();
            }

            // Đánh dấu là đã hoàn thành (status = true)
            if (isCompleted.HasValue && isCompleted.Value)
            {
                tasks = tasks.Where(t => t.Status).ToList();
            }

            // Không được đánh dấu là đã hoàn thành (status = false)
            if (isCompleted.HasValue && !isCompleted.Value)
            {
                tasks = tasks.Where(t => !t.Status).ToList();
            }

        }
    }
}
