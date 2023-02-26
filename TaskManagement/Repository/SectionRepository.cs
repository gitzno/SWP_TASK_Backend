using AutoMapper;
using TaskManagement.Dto;
using TaskManagement.Interface;
using TaskManagement.Models;
using TaskManagement.Utils;
//using static System.Collections.Specialized.BitVector32;

namespace TaskManagement.Repository
{
    public class SectionRepository : ISectionRepository
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
        public SectionRepository(TaskManagementContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public Section GetSectionById(int SectionID)
        {
            return  _context.Sections.Where(s => s.Id == SectionID).FirstOrDefault();
          
        }

        public ResponseObject GetSections()
        {
            var sections = _context.Sections.ToList();
            var sectionMap = _mapper.Map<List<SectionDto>>(sections);
            if (sections != null)
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = sectionMap
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
        public ResponseObject GetSectionsByName(int workspaceID,string sectionName)
        {
            
            var sections = _context.Sections.Where(s => s.Title == sectionName && s.WorkSpaceId == workspaceID).ToList();
            var sectionMap = _mapper.Map<List<SectionDto>>(sections);
            if (sections != null)
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = sectionMap
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

        public ResponseObject GetSectionsInWorkspace(int WorkspaceID)
        {
            var sections = _context.Sections.Where(s => s.WorkSpaceId == WorkspaceID).ToList();
            var sectionsMap = _mapper.Map<List<SectionDto>>(sections);
            if (sections != null)
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = sectionsMap
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

        public ResponseObject GetSectionsUserJoined(int workspaceID, int userID)
        {
            var sections = from s in _context.Sections
                           join usr in _context.UserSectionRoles on s.Id equals usr.SectionId
                           where usr.UserId == userID
                           select s;
            sections.Where(s => s.WorkSpaceId == workspaceID).ToList();

            var sectionsMap = _mapper.Map<List<SectionDto>>(sections);
            if (sections != null)
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = sectionsMap
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

       
        public ResponseObject AddMemberIntoSection(int sectionID, int userID, int roleID)
        {
            var user = _context.Users.Where(u => u.Id == userID).FirstOrDefault();
            var role = _context.Roles.FirstOrDefault(r => r.Id == roleID);
            var section = _context.Sections.FirstOrDefault(s => s.Id== sectionID);

            if (section == null|| user == null || role == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound,
                    Data = null,
                };
            }
            var roleSection = new UserSectionRole
            {
                UserId = user.Id,
                RoleId = roleID,
                SectionId = sectionID,
            };
            _context.UserSectionRoles.Add(roleSection);
            var userMap = _mapper.Map<UserDto>(user);
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

       

        public ResponseObject CreateSection(int userID,int roleID, Section section)
        {
            var user = _context.Users.Where(u => u.Id == userID).FirstOrDefault();
            var role = _context.Roles.Where(r => r.Id == roleID).FirstOrDefault();
            
            if (user == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " userID",
                    Data = null
                };
            }
            if (role == null)
            {
                return new ResponseObject
                {
                    Status = Status.NotFound,
                    Message = Message.NotFound + " roleID",
                    Data = null
                };
            }
            var userSectionRole = new UserSectionRole
            {
                UserId = userID,
                RoleId = roleID,
                SectionId = section.Id,
                Role = role,
                User = user,
                Section = section
            };
            _context.Add(userSectionRole);
            _context.Sections.Add(section);

            var sectionMap = _mapper.Map<SectionDto>(section);
            if (Save())
            {
                
                return new ResponseObject
                {
                    Status = Status.Created,
                    Message = Message.Created,
                    Data = sectionMap
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

        public ResponseObject DeleteSection(Section section)
        {
            _context.Sections.Remove(section);
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
                    Status = "Error",
                    Message = "Error",
                    Data = null
                };
            }
        }

     
       

        public ResponseObject UpdateSection(Section section)
        {
            _context.Sections.Update(section);
            var sectionMap = _mapper.Map<SectionDto>(section);
            if (Save())
            {
                return new ResponseObject
                {
                    Status = Status.Success,
                    Message = Message.Success,
                    Data = sectionMap
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = "Error",
                    Message = "Error",
                    Data = null
                };
            }
        }


        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool SectionExists(int sectionID)
        => _context.Sections.Any(s => s.Id == sectionID);

       
    }
}
