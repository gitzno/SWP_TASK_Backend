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

namespace TaskManagement.Repository
{
    public class UserRepository : IUserRepository
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
        public UserRepository(TaskManagementContext context ,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

       

        public User GetUserByID(int userId)
        {
            return _context.Users.Where(u => u.Id == userId).FirstOrDefault();
        }

        public ResponseObject GetUsers()
        {
          var users =  _context.Users.ToList();
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
                    Message = Message.NotFound,
                    Data = null
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
                    Message = Message.NotFound,
                    Data = null
                };
            }
        }

        public ResponseObject GetWorkSpacesByUser(int userID)
        {
            var workspaces = _context.UserWorkSpaceRoles.Where(u => u.UserId == userID).Select(s => s.WorkSpace).ToList();
            var workspacesMap = _mapper.Map<List<WorkSpaceDto>>(workspaces);
            if (workspaces != null)
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = workspacesMap
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

        public ResponseObject GetUsersJoinSection(int sectionID)
        {
            var users = from u in _context.Users
                        join usr in _context.UserSectionRoles on u.Id equals usr.UserId
                        where usr.SectionId == sectionID
                        select u;
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
                    Message = Message.NotFound,
                    Data = null
                };
            }
        }



        public ResponseObject GetUsersJoinTask(int taskID)
        {
            var users = from u in _context.Users
                        join utr in _context.UserTaskRoles on u.Id equals utr.UserId
                        where utr.TaskId == taskID
                        select u;
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
                    Message = Message.NotFound,
                    Data = null
                };
            }
        }

      

        public ResponseObject CreateUser(User user)
        {

            string patternemail = @"^[a-za-z0-9._%+-]+@gmail.com$";
            if (user.Email != null && !Regex.IsMatch(user.Email, patternemail))
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest +" Email is not valid",
                    Data = null
                };
            }

            string patternphone = @"^0\d{9}$";
            if (user.Email != null && !Regex.IsMatch(user.Email, patternphone))
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest + " Phone is not valid",
                    Data = null
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


            var userMap = _mapper.Map<User>(user);
            userMap.Password = user.Password; // Assign hashed password to userMap.Password property

            _context.Users.Add(user);
            
            if (Save())
            {

                return new ResponseObject
                {
                    Status = Status.Created,
                    Message = Message.Created,
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
        public ResponseObject UpdateUser(User user)
        {
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
                    Message = Message.BadRequest,
                    Data = null
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
                    Message = Message.BadRequest,
                    Data = null
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
                    Data = null
                };
            }
        }
    }
}
