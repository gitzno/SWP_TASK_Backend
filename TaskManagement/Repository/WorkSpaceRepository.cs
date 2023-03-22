﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskManagement.Dto;
using TaskManagement.Interface;
using TaskManagement.Models;
using TaskManagement.Service;
using TaskManagement.Utils;

namespace TaskManagement.Repository
{
    public class WorkSpaceRepository : IWorkSpaceRepository
    {
        private readonly TaskManagementContext _context;
        private readonly IMapper _mapper;

        public WorkSpaceRepository(TaskManagementContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        public ResponseObject AddMemberIntoWorkspace(int workSpaceID, string nameUser, int roleID, int adminID)
        {

            var user = _context.Users.Where(u => u.UserName == nameUser).FirstOrDefault(); // null
            if (user == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + "user",
                };
            }
            var role = _context.Roles.Where(r => r.Id == roleID).FirstOrDefault();
            var workSpace = _context.WorkSpaces.Where(w => w.Id == workSpaceID).FirstOrDefault();
            var _user = _context.UserWorkSpaceRoles
                .Where(o => o.UserId == user.Id && o.WorkSpaceId == workSpace.Id).FirstOrDefault(); // tim xem da trong ws chua
            var admin = _context.UserWorkSpaceRoles.Where(o => o.UserId == adminID && o.WorkSpaceId == workSpaceID && o.RoleId == 1).FirstOrDefault(); // tim th admin
            if (admin == null)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest + " not ADMIN"
                };
            }
            
            if (role == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + "role",
                };
            }
            if (workSpace == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + "workSpace",
                };
            }

            if (_user != null)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest +"user already in workspace",
                };
            }
            var userWorkSpace = new UserWorkSpaceRole
            {
                UserId = user.Id,
                RoleId = roleID,
                WorkSpaceId = workSpaceID,
            };
            _context.UserWorkSpaceRoles.Add(userWorkSpace);
            var userMap = _mapper.Map<UserDto>(user);
            if (Save())
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success +"add member into workSpace",
                    Data = userMap
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest +"fail add member into workSpace",
                };
            }
        }

        public ResponseObject CreateWorkSpace(int userID, int roleID, WorkSpace workSpaceCreate)
        {
            var user = _context.Users.Where(u => u.Id == userID).FirstOrDefault();
            var role = _context.Roles.Where(r => r.Id == roleID).FirstOrDefault();

            if (user == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound +"user",
                };
            }
            if (role == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + "role",
                };
            }

            var userWorkSpace = new UserWorkSpaceRole
            {
                UserId = userID,
                RoleId = roleID,
                WorkSpaceId = workSpaceCreate.Id,
                Role = role,
                User = user,
                WorkSpace = workSpaceCreate,
            };
            _context.Add(userWorkSpace);
            _context.WorkSpaces.Add(workSpaceCreate);

            var wsMap = _mapper.Map<WorkSpaceDto>(workSpaceCreate);

            if (Save())
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success+"Created workSpace",
                    Data = wsMap
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest + "fail created workSpace",
                };
            }
        } 

        public ResponseObject DeleteWorkSpace(WorkSpace workSpace, int userID)
        {
            if (workSpace == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " workSpace"
                };
            }
            var _user = _context.UserWorkSpaceRoles.SingleOrDefault(o => o.UserId == userID && o.RoleId == 1 && o.WorkSpaceId == workSpace.Id);
            if (_user == null )
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest + "you are not admin",
                };
            }
            if (GetWorkSpaceByID(workSpace.Id) == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound +"workSpace"
                };
            }
            _context.Remove(workSpace);
            if (Save())
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success +"deleted"
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest +"deleted"
                };
            }
        }

        public ResponseObject GetWorkSpaceByCreateTime(int? month, int year)
        {
            var ws = _context.WorkSpaces
            .Where(w => (w.CreatedTime.Month == month || !month.HasValue)
            && w.CreatedTime.Year == year).ToList();
            var wsMap = _mapper.Map<List<WorkSpaceDto>>(ws);

            if (ws != null)
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = wsMap
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
        public WorkSpace GetWorkSpaceByID(int workSpaceID)
        => _context.WorkSpaces.Where(o => o.Id == workSpaceID).FirstOrDefault();

        public ResponseObject GetWorkSpacesByUser(int userID)
        {
            var _user = _context.Users.Where(o => o.Id == userID).FirstOrDefault();
            if (_user == null)
            {
                return new ResponseObject { Status = Status.NotFound, Message = Message.NotFound+" user" };
            }

            var ws = _context.UserWorkSpaceRoles.Where(o => o.UserId == userID).Select(o => o.WorkSpace).ToList();
            var wsMap = _mapper.Map<List<WorkSpaceDto>>(ws);
            return new ResponseObject
            {
                Status = Status.Success,
                Message = Message.Success,
                Data = wsMap
            };
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public ResponseObject UpdateWorkSpace(WorkSpace workSpace, int userID)
        {
            var user = _context.UserWorkSpaceRoles.Where(o => o.UserId == userID && o.WorkSpaceId == workSpace.Id && o.RoleId == 1).FirstOrDefault();
            if (user == null)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest + " you can not update"
                };
            }
            _context.WorkSpaces.Update(workSpace);
            var wsMap = _mapper.Map<WorkSpaceDto>(workSpace);
            if (GetWorkSpaceByID(workSpace.Id) == null)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest +" workSpace"
                };
            }

           
            if (Save())
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = wsMap
                };
            else
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest +"not update workspace",
                    Data = null
                };
        }

        public ResponseObject WorkSpaces()
        {
            var ws = _context.WorkSpaces.ToList();
            var wsMap = _mapper.Map<List<WorkSpaceDto>>(ws);

            if (ws != null)
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = wsMap
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
    }
}
