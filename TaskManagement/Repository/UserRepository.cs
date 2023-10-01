using AutoMapper;
using System.Text.RegularExpressions;
using System.Text;
using System;
using TaskManagement.Dto;
using TaskManagement.Interface;
using TaskManagement.Models;
using TaskManagement.Utils;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using static System.Collections.Specialized.BitVector32;
using SWP_Login.Utils;
using TaskManagement.Service;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TaskManagement.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly TaskManagementContext _context;
        private readonly IMapper _mapper;
        private readonly Cloudinary cloudinary;

        public UserRepository(TaskManagementContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;
            Account account = new Account(
              configuration["Cloudinary:CloudName"],
              configuration["Cloudinary:ApiKey"],
              configuration["Cloudinary:ApiSecret"]
            );
            cloudinary = new Cloudinary(account);
        }



        public User GetUserByID(int userId)
        {
            return _context.Users.Where(u => u.Id == userId).FirstOrDefault();
        }

        public ResponseObject GetUsers()
        {
            var users = _context.Users.ToList();
            var usersMap = _mapper.Map<List<UserDto>>(users);
            if (users != null)
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = usersMap
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound
                };
            }

        }

        public ResponseObject GetUserByUserName(string userName)
        {
            var users = _context.Users.Where(u => u.UserName.Contains(userName)).ToList();
            var usersMap = _mapper.Map<List<UserDto>>(users);
            if (users != null)
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = usersMap
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound
                };
            }
        }

        public ResponseObject GetUsersJoinWorkSpace(int workSpaceID)
        {
            var users = _context.UserWorkSpaceRoles.Where(o => o.WorkSpaceId == workSpaceID).Select(o => o.User).Distinct().ToList();

            var usersMap = _mapper.Map<List<UserDto>>(users);
            if (users != null)
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = usersMap
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " user"
                };
            }
        }

        public ResponseObject GetUsersJoinSection(int sectionID)
        {
            var section = _context.Sections.SingleOrDefault(o => o.Id == sectionID);
            if (section == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " section"
                };
            }
            var users = _context.UserSectionRoles.Where(o => o.SectionId == sectionID).Select(o => o.User).Distinct().ToList();
            var usersMap = _mapper.Map<List<UserDto>>(users);
            if (users != null)
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = usersMap
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " user",
                };
            }
        }



        public ResponseObject GetUsersJoinTask(int taskID)
        {
            var task = _context.Tasks.SingleOrDefault(o => o.Id == taskID);
            if (task == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " task"
                };
            }
            var users = _context.UserTaskRoles.Where(o => o.TaskId == taskID && o.RoleId == 2).Select(o => o.User).Distinct().ToList();
            var usersMap = _mapper.Map<List<UserDto>>(users);
            if (users != null)  
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = usersMap
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " user"
                };
            }
        }



        public ResponseObject CreateUser(User user)
        {
            var _user = _context.Users.Any(o => o.UserName == user.UserName);
            if (_user)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest + "user name already exists",
                };
            }


            // hash password using md5 algorithm
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(user.Password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                string hashedPassword = BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 16);
                user.Password = hashedPassword;
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            return new ResponseObject
            {
                Status = Status.Created,
                Message = Message.Created,
                Data = null
            };
        }
        public ResponseObject UpdateUser(User user, int userId)
        {
            if (user.Id != userId)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest,
                };
            }
            var _user = _context.Users.AsNoTracking().SingleOrDefault(o => o.Id== userId);

            user.Password = _user.Password;
            _context.Update(user);
            var userMap = _mapper.Map<UserDto>(user);
            if (Save())
            {

                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success + "update",
                    Data = userMap
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest + " updated"
                };
            }
        }



        public ResponseObject DeleteUser(User user)
        {
            _context.Remove(user);
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
                    Message = Message.BadRequest + " delete",
                };
            }
        }
        public bool UserExists(int userId)
        {
            return _context.Users.Any(u => u.Id == userId);
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
        public bool UserNameExists(string userName)
        => _context.Users.Any(u => u.UserName == userName);

        public ResponseObject Login(User user)
        {

            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(user.Password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                string hashedPassword = BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 16);
                user.Password = hashedPassword;
            }
            var accDB = _context.Users
                    .Where(u => u.UserName == user.UserName && u.Password == user.Password)
                    .FirstOrDefault();

            if (accDB != null)
            {
                return new ResponseObject
                {
                    Status = "500",
                    Message = "Login successfully",
                    Data =
                    new
                    {
                        username = user.UserName,
                        usernameID= accDB.Id,
                        token = GenerateToken.GenerateMyToken(user)
                    }
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = "400",
                    Message = "Login failed",
                };
            }
        }

        public async Task<UploadResult> UploadAsync(IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    PublicId = Guid.NewGuid().ToString()
                };

                var result = await cloudinary.UploadAsync(uploadParams);

                return result;
            }
        }

        public ResponseObject UpdateImage(int userID, string file)
        {
            var user = _context.Users.Where(o => o.Id == userID).FirstOrDefault();
            if (user != null)
            {
                user.Image = file;
                _context.Update(user);
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
                        Message = Message.BadRequest + " update image"
                    };
                }
            }
            else
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " user"
                };
            }
        }

        public ResponseObject GetUsersUnJoinTask(int taskID)
        {

            var task = _context.Tasks.Where(o => o.Id == taskID).FirstOrDefault();
            if (task == null)
            {
                return new ResponseObject { Status = Status.NotFound, Message = Message.NotFound + " task" };
            }
            var section = _context.Sections.Where(o => o.Id == task.SectionId).FirstOrDefault();
            if (section == null)
            {
                return new ResponseObject { Status = Status.NotFound, Message = Message.NotFound + " section" };
            }
            var ws = _context.WorkSpaces.Where(o => o.Id == section.WorkSpaceId).FirstOrDefault();
            if (ws == null)
            {
                return new ResponseObject { Status = Status.NotFound, Message = Message.NotFound + " workSpace" };
            }

            var userWS = _context.UserWorkSpaceRoles.Where(o => o.WorkSpaceId == ws.Id).Select(o => o.User).Distinct().ToList();
            var userTask = _context.UserTaskRoles.Where(o => o.TaskId == taskID && o.RoleId == 2).Select(o => o.User).Distinct().ToList();

            var users = userWS.Except(userTask);
            var userMap = _mapper.Map<List<UserDto>>(users);
            if (users != null)
            {
                return new ResponseObject { Status = Status.Success, Message = Message.Success, Data = userMap };
            }
            else
            {
                return new ResponseObject { Status = Status.NotFound, Message = Message.NotFound + " user" };
            }
        }

        public ResponseObject DeleteUserWorkSpace(int workSpaceID, int userIdDeleted, int userAdminId)
        {
            // khong the tu xoa chinh minh
            if (userAdminId == userIdDeleted)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest + " cannot deleted yourself",
                };
            }
            // tim ra nguoi bi xoa trong workSpace
            var userDeleteWorkSpace = _context.UserWorkSpaceRoles
                .SingleOrDefault(o => o.UserId == userIdDeleted && o.WorkSpaceId == workSpaceID && o.RoleId != 1);
            if (userDeleteWorkSpace == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " user in workSpace",
                };
            }
            var userDeleteTask = _context.UserTaskRoles
                .Where(o => o.Task.Section.WorkSpaceId == workSpaceID && o.UserId == userIdDeleted).ToList();
            if (userDeleteTask != null)
            {
                _context.UserTaskRoles.RemoveRange(userDeleteTask);
            }


            // tim ra xem co phai la nguoi tao khong
            var userAdmin = _context.UserWorkSpaceRoles
                .SingleOrDefault(o => o.UserId == userAdminId && o.WorkSpaceId == workSpaceID && o.RoleId == 1);
            if (userAdmin == null)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest + " not admin",
                };
            }


            _context.UserWorkSpaceRoles.Remove(userDeleteWorkSpace); // xoa trong ws
            if (Save())
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
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
    }
}
