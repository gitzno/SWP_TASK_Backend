using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using System.Collections.ObjectModel;
using TaskManagement.Dto;
using TaskManagement.Interface;
using TaskManagement.Models;
using TaskManagement.Service;
using TaskManagement.Utils;
using static System.Collections.Specialized.BitVector32;

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
            var taskMap = _mapper.Map<List<TaskGetObject>>(tasks);
            foreach (var item in taskMap)
            {
                item.Info = GetInforTask(item.Id);
            }
            if (tasks == null)
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest,
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
            var _user = _context.Users.SingleOrDefault(o => o.Id == userID);
            if (_user == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " user",
                };
            }

            ICollection<Models.Task> tasks = new Collection<Models.Task>();

            if (workspaceID != 0)
            {
                tasks = _context.UserTaskRoles.Where(utr => utr.UserId == userID && utr.RoleId == 2)
                   .Select(t => t.Task).Where(o => o.Section.WorkSpace.Id == workspaceID).ToList();
            }
            else
            {
                tasks = _context.UserTaskRoles.Where(utr => utr.UserId == userID && (utr.RoleId == 2 || utr.RoleId == 1))
                   .Select(t => t.Task).Distinct().ToList();
            }

            var tasksMap = _mapper.Map<List<TaskGetObject>>(tasks);
            foreach (var item in tasksMap)
            {
                item.Info = GetInforTask(item.Id);
            }
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
                    Message = Message.NotFound + " task",
                };
            }
        }

        public ResponseObject GetUnassignedTasks(int workspaceID)
        {
            var ws = _context.WorkSpaces.SingleOrDefault(o => o.Id == workspaceID);
            if (ws == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " workSpace",
                };
            }
            var tasksAll = _context.Tasks.Where(t => t.Section.WorkSpaceId == workspaceID).ToList();
            var tasksAssign = _context.UserTaskRoles.Where(utr => utr.RoleId == 2)
            .Select(t => t.Task).Where(t => t.Section.WorkSpaceId == workspaceID).ToList();

            var tasks = tasksAll.Except(tasksAssign);
            var taskMap = _mapper.Map<List<TaskGetObject>>(tasks);
            foreach (var item in taskMap)
            {
                item.Info = GetInforTask(item.Id);
            }
            if (tasks != null)
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
                    Status = Status.NotFound,
                    Message = Message.NotFound + " task"
                };
            }
        }

        public ResponseObject GetTaskCountOfUser(int workspaceID, int userId)
        {
            var ws = _context.WorkSpaces.SingleOrDefault(o => o.Id == workspaceID);
            if (ws == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " workSpace",
                };
            }
            var _user = _context.Users.SingleOrDefault(o => o.Id == userId);
            if (_user == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " user",
                };
            }

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
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound,
                };
            }
        }

        public ResponseObject GetTaskCountUserCompleted(int workspaceID, int userId, bool status)
        {
            var ws = _context.WorkSpaces.SingleOrDefault(o => o.Id == workspaceID);
            if (ws == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " workSpace",
                };
            }
            var _user = _context.Users.SingleOrDefault(o => o.Id == userId);
            if (_user == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " user",
                };
            }
            var numberOfTaskDone = _context.UserTaskRoles.Where(utr => utr.UserId == userId && utr.RoleId == 2)
            .Select(t => t.Task).Where(t => t.Section.WorkSpaceId == workspaceID && t.Status == status).ToList();
            var taskMap = _mapper.Map<List<TaskGetObject>>(numberOfTaskDone);
            foreach (var item in taskMap)
            {
                item.Info = GetInforTask(item.Id);
            }
            if (numberOfTaskDone == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound,
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = taskMap,
                };
            }
        }


        public ResponseObject CreateTask(int? sectionID, int userId, int roleId, Models.Task task)
        {
            var user = _context.Users.SingleOrDefault(o => o.Id == userId);
            var role = _context.Roles.SingleOrDefault(o => o.Id == roleId);
            var section = _context.Sections.SingleOrDefault(o => o.Id == sectionID);
            if (user == null)
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " user"
                };
            if (role == null)
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " role"
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
            string noti = user.UserName + " đã thêm " + task.Title;
            if (section != null)
                noti += " vào danh sách " + section.Title;

            var notifi = new Notification
            {
                TaskId = task.Id,
                UserActiveId = userId,
                UserPassiveId = null,
                Describe = noti,
                Task = task,
            };
            _context.Add(notifi);
            _context.Add(userTaskRole);
            _context.Tasks.Add(task);


            var taskMap = _mapper.Map<TaskDto>(task);
            
                
            if (Save())
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success + "created task",
                    Data = taskMap
                };
            else
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest + " created task"
                };
            }

        }


        public ResponseObject UpdateTask(Models.Task task, int userID)
        {
            List<Notification> noti = new List<Notification>();
            var _user = _context.Users.SingleOrDefault(o => o.Id == userID);

            var section = _context.Sections.Where(o => o.Id == task.SectionId).FirstOrDefault();
            int wsID = 0;
            if (section != null)
            {
                var ws = _context.WorkSpaces.Where(o => o.Id == section.WorkSpaceId).FirstOrDefault();
                wsID = ws.Id;
            
            }
            // tim ra th tao ws 
            var _userWorkSpace = _context.UserWorkSpaceRoles.Where(o => o.UserId == userID && o.RoleId == 1 && o.WorkSpaceId == wsID).FirstOrDefault();
            // tim ra th tao task 
            var _userCreateTask = _context.UserTaskRoles.Where(o => o.UserId == userID && o.RoleId == 1 && o.TaskId == task.Id).FirstOrDefault();

            if (_userWorkSpace != null || _userCreateTask != null)
            {

                var _task = _context.Tasks.AsNoTracking().SingleOrDefault(o => o.Id == task.Id); // task old 
                if (_task == null)
                {
                    return new ResponseObject
                    {
                        Status = Status.NotFound,
                        Message = Message.NotFound + " task",
                    };
                }
                else
                {
                    if (!_task.Title.Equals(task.Title))
                    {
                        noti.Add(new Notification
                        {
                            TaskId = task.Id,
                            UserActiveId = userID,
                            Describe = _user.UserName + " đã thay đổi tiêu đề thành " + task.Title.ToUpper(),
                        });
                    }
                    if (!_task.Describe.Equals(task.Describe))
                    {
                        noti.Add(new Notification
                        {
                            TaskId = task.Id,
                            UserActiveId = userID,
                            Describe = _user.UserName + " đã thay đổi mô tả thành \"" + task.Describe + "\"",
                        });
                    }
                    if (_task.Status != task.Status)
                    {
                        string msgStatus = task.Status ? "đã hoàn thành" : "chưa hoàn thành";

                        noti.Add(new Notification
                        {
                            TaskId = task.Id,
                            UserActiveId = userID,
                            Describe = _user.UserName + " đã đánh dấu " + task.Title.ToUpper() + " là: " + msgStatus,
                        });
                    }
                    if (_task.TaskFrom != task.TaskFrom)
                    {
                        if (_task.TaskFrom == null)
                        {
                            noti.Add(new Notification
                            {
                                TaskId = task.Id,
                                UserActiveId = userID,
                                Describe = _user.UserName + " đã thêm ngày bắt đầu là " + task.TaskFrom,
                            });
                        }
                        else
                        {
                            noti.Add(new Notification
                            {
                                TaskId = task.Id,
                                UserActiveId = userID,
                                Describe = _user.UserName + " đã sửa ngày bắt đầu thành " + task.TaskFrom,
                            });
                        }
                    }
                    if (_task.TaskTo != task.TaskTo)
                    {
                        if (_task.TaskTo == null)
                        {
                            noti.Add(new Notification
                            {
                                TaskId = task.Id,
                                UserActiveId = userID,
                                Describe = _user.UserName + " đã thêm ngày hết hạn là " + task.TaskTo,
                            });
                        }
                        else
                        {
                            noti.Add(new Notification
                            {
                                TaskId = task.Id,
                                UserActiveId = userID,
                                Describe = _user.UserName + " đã sửa ngày hết hạn thành " + task.TaskTo,
                            });
                        }
                    }

                }
                _context.Notifications.AddRange(noti.ToArray());
                _context.Tasks.Update(task);
                var taskMap = _mapper.Map<TaskDto>(task);
                if (Save())
                {
                    return new ResponseObject
                    {
                        Status = Status.Success,
                        Message = Message.Success + "updated task",
                        Data = taskMap
                    };
                }
                else
                {
                    return new ResponseObject
                    {
                        Status = Status.BadRequest,
                        Message = Message.BadRequest + "updated task",
                    };
                }
            }

            return new ResponseObject
            {
                Status = Status.BadRequest,
                Message = Message.BadRequest + " you can not update"
            };

        }
        public ResponseObject DeleteTask(Models.Task task, int userID)
        {
            if (task == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " task"
                };
            }
            var secID = task.SectionId;
            var wsID = _context.Sections.Where(o => o.Id == secID).Select(o => o.WorkSpaceId).SingleOrDefault();

            var _userAdmin = _context.UserWorkSpaceRoles.SingleOrDefault(o => o.UserId == userID && o.RoleId == 1 && o.WorkSpaceId == wsID);
            var _userCreateTask = _context.UserTaskRoles.SingleOrDefault(o => o.UserId == userID && o.RoleId == 1 && o.TaskId == task.Id);

            if (_userAdmin != null || _userCreateTask != null)
            {

                _context.Remove(task);

                if (Save())
                {
                    return new ResponseObject
                    {
                        Status = Status.Success,
                        Message = Message.Success + "delete task"

                    };
                }
                else
                {
                    return new ResponseObject
                    {
                        Status = Status.BadRequest,
                        Message = Message.BadRequest + "delete task"
                    };
                }
            }
            return new ResponseObject { Status = Status.BadRequest, Message = Message.BadRequest + " can not delete" };
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

        public ResponseObject AddMemberIntoTask(int taskID, int userID, int roleID, int adminID)
        {
            var task = _context.Tasks.Where(o => o.Id == taskID).FirstOrDefault();
            var secID = task.SectionId;
            var wsID = _context.Sections.Where(o => o.Id == secID).Select(o => o.WorkSpaceId).SingleOrDefault();
            var user = _context.Users.Where(o => o.Id == userID).FirstOrDefault(); // la thang duoc them vao
            var role = _context.Roles.Where(o => o.Id == roleID).FirstOrDefault();
            var _user = _context.UserTaskRoles.Where(o => o.UserId == userID && o.TaskId == taskID).FirstOrDefault(); // xem da o trong task chua
            var _userWS = _context.UserWorkSpaceRoles.Where(o => o.UserId == userID && o.WorkSpaceId == wsID).FirstOrDefault();  // xem da trong ws k

            var admin = _context.UserWorkSpaceRoles.Where(o => o.UserId == adminID && o.WorkSpaceId == wsID && o.RoleId == 1).FirstOrDefault();
            var adminInfo = _context.Users.SingleOrDefault(o => o.Id == adminID);
            if (_userWS == null)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest + "user not into workSpace",
                };
            }
            if (_user != null)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest + "user already into task",
                };
            }
            if (user == null || task == null || role == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + "user or task or role ",
                };
            }
            if (admin == null)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest + " not ADMIN"
                };
            }
            var addUserTask = new UserTaskRole
            {
                TaskId = taskID,
                RoleId = roleID,
                UserId = userID,
            };
            var noti = new Notification
            {
                TaskId= taskID,
                UserActiveId= adminID,
                UserPassiveId= userID,
                Describe = adminInfo.UserName.ToUpper() +" đã giao "+ task.Title+ " cho "+user.UserName.ToUpper(),
            };
            _context.Notifications.Add(noti);
            _context.UserTaskRoles.Add(addUserTask);
            var userMap = _mapper.Map<UserDto>(user);
            if (Save())
            {

                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success + "add member task",
                    Data = userMap
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest + "add member task",
                };
            }
        }

        public ResponseObject GetTasksInSection(int sectionId)
        {
            var section = _context.Sections.SingleOrDefault(o => o.Id == sectionId);
            if (section == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " section",
                };
            }

            var task = _context.Tasks.Where(o => o.SectionId == sectionId).ToList();
            var taskMap = _mapper.Map<List<TaskGetObject>>(task);
            foreach (var item in taskMap)
            {
                item.Info = GetInforTask(item.Id);
            }
            if (task == null)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest,
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
                    };
                }
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + "task",
                };
            }
        }

        public ResponseObject GetTasksRangeByTime(int userID, DateTime timeFrom, DateTime timeTo)
        {
            var user = _context.Users.SingleOrDefault(o => o.Id == userID);
            if (user == null)
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " user"
                };

            var task = _context.UserTaskRoles.Where(o => o.UserId == userID)
                .Select(o => o.Task).Where(o => o.TaskTo >= timeFrom && o.TaskFrom <= timeTo).ToList();

            var taskMap = _mapper.Map<List<TaskGetObject>>(task);
            foreach (var item in taskMap)
            {
                item.Info = GetInforTask(item.Id);
            }

            if (task != null)
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
                    Status = Status.NotFound,
                    Message = Message.NotFound,
                };
            }
        }

        public Object GetInforTask(int taskID)
        {
            var task = _context.Tasks.SingleOrDefault(o => o.Id == taskID);
            if (task == null)
            {
                return null;
                //return new ResponseObject
                //{
                //    Status = Status.NotFound,
                //    Message = Message.NotFound + "task",
                //};
            }
            var section = _context.Sections.Where(o => o.Id == task.SectionId).SingleOrDefault();
            if (section == null)
            {
                return null;
            }
            var workSpace = _context.WorkSpaces.Where(o => o.Id == section.WorkSpaceId).SingleOrDefault();
            var userCreateTask = _context.UserTaskRoles.Where(o => o.TaskId == taskID && o.RoleId == 1).Select(o => o.User).SingleOrDefault();
            return new { section = section.Title, workSpace = workSpace.Name, user = userCreateTask.UserName };

            //return new ResponseObject
            //{
            //    Status = Status.Success,
            //    Message = Message.Success,
            //    Data = new { section = section.Title, workSpace = workSpace.Name, user = userCreateTask.UserName },
            //};
        }

        public ResponseObject UpdateStatusTask(int taskID, int userID, bool status)
        {
            var task = _context.UserTaskRoles.Where(o => o.UserId == userID && o.TaskId == taskID && o.RoleId != 1).SingleOrDefault();

            if (task == null)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest
                };
            }

            task.Status = status;
            _context.Update(task);
            if (Save())
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = task
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest,
                };
            }

        }

        public ResponseObject UpdatePinTask(int taskID, int userID, bool status)
        {
            var task = _context.UserTaskRoles.Where(o => o.UserId == userID && o.TaskId == taskID && o.RoleId != 1).SingleOrDefault();

            if (task == null)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest
                };
            }

            task.PinTask = status;
            _context.Update(task);
            if (Save())
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = task
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest,
                };
            }
        }

        public ResponseObject GetTaskInWorkSpace(int workSpaceID, int userID)
        {
            var userWorkSpace = _context.UserWorkSpaceRoles.Where(o => o.UserId == userID && o.WorkSpaceId == workSpaceID && o.RoleId == 1).SingleOrDefault();
            if (userWorkSpace == null)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest,
                };
            }
            var task = _context.UserTaskRoles.Where(o => o.RoleId == 1&& o.UserId == userID).Select(o => o.Task).Where(o => o.Section.WorkSpaceId == workSpaceID).ToList();
            var taskMap = _mapper.Map<List<TaskGetObject>>(task);
            foreach (var item in taskMap)
            {
                item.Info = GetInforTask(item.Id);
            }
            return new ResponseObject
            {
                Status = Status.Success,
                Message = Message.Success,
                Data = taskMap
            };
        }

        public ResponseObject GetUserTaskRoleByUserID(int userId, int taskID)
        {
            var userTask = _context.UserTaskRoles.Where(o => o.UserId == userId && o.TaskId == taskID && o.RoleId != 1).SingleOrDefault();
            if (userTask != null)
            {
                var userTaskMap = _mapper.Map<UserTaskRoleDto>(userTask);
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = userTaskMap
                };
            }

            return new ResponseObject
            {
                Status = Status.BadRequest,
                Message = Message.BadRequest,
            };
        }
        public ResponseObject GetUserTaskRoleByUserID(int userId)
        {
            var userTask = _context.UserTaskRoles.Join(_context.Tasks, userRole => userRole.TaskId, task => task.Id, 
                (userRole, task) => new
                {
                    UserId = userRole.UserId,
                    RoleId = userRole.RoleId,
                    Status = userRole.Status,
                    PinTask = userRole.PinTask,
                    TaskId = task.Id,
                    SectionId= task.SectionId,
                    Title = task.Title,
                    Describe = task.Describe,
                    Image = task.Image,
                    TaskFrom = task.TaskFrom,
                    TaskTo = task.TaskTo,
                    Attachment = task.Attachment,
                } 
               ).Where(o => o.UserId == userId && o.RoleId != 1).ToList();
            if (userTask != null)
            {

                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = userTask
                };
            }

            return new ResponseObject
            {
                Status = Status.BadRequest,
                Message = Message.BadRequest,
            };
        }

        public ResponseObject UpdateSectionTask(int sectionNewID, int taskID, int userID)
        {
            var sectionNew = _context.Sections.SingleOrDefault(o => o.Id == sectionNewID);
            if (sectionNew == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " section",
                };
            }
            var task = _context.Tasks.Where(o => o.Id == taskID).FirstOrDefault();
            var sec = _context.Sections.SingleOrDefault(o => o.Id == task.SectionId);
            var wsID = _context.Sections.Where(o => o.Id == sec.WorkSpaceId).Select(o => o.WorkSpaceId).SingleOrDefault();

            var admin = _context.UserWorkSpaceRoles.Where(o => o.WorkSpaceId == wsID && o.UserId == userID && o.RoleId == 1).FirstOrDefault();
            var userCreateTask = _context.UserTaskRoles.Where(o => o.TaskId == taskID && o.UserId == userID && o.RoleId == 1).FirstOrDefault();
            var user = _context.Users.SingleOrDefault(o => o.Id == userID);
            if (admin != null || userCreateTask != null)
            {
                task.SectionId = sectionNewID;
                _context.Tasks.Update(task);
                var noti = new Notification
                {
                    TaskId = taskID,
                    UserActiveId = userID,
                    Describe = $"{user.UserName} đã chuyển {task.Title.ToUpper()} từ {sec.Title.ToUpper()} section sang {sectionNew.Title.ToUpper()} section",
                };
                _context.Notifications.Add(noti);
                var taskMap = _mapper.Map<TaskGetObject>(task);
                taskMap.Info = GetInforTask(taskID);
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
                    };
                }
            }
            return new ResponseObject
            {
                Status = Status.BadRequest,
                Message = Message.BadRequest,
            };
        }
    }
}
