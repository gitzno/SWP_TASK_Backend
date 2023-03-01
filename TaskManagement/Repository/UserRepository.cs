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
using SWP_Login.Utils;
using TaskManagement.Service;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using static System.Collections.Specialized.BitVector32;

namespace TaskManagement.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly TaskManagementContext _context;
        private readonly IMapper _mapper;
        private readonly Cloudinary cloudinary;

        public UserRepository(TaskManagementContext context ,IMapper mapper, IConfiguration configuration)
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

        public ResponseObject GetUsersJoinWorkSpace(int workSpaceID)
        {
            var users = from u in _context.Users
                        join uwr in _context.UserWorkSpaceRoles on u.Id equals uwr.UserId
                        where uwr.WorkSpaceId == workSpaceID
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
            var _user = _context.Users.Any(o => o.UserName == user.UserName);
            if (_user)
            {
                return new ResponseObject
                {
                    Status = Status.BadRequest,
                    Message = Message.BadRequest + "user name already exists",
                    Data = null
                };
            }

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
            if (user.Email != null && !Regex.IsMatch(user.Phone, patternphone))
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

      
    }
}
