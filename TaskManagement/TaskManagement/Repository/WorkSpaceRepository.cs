using TaskManagement.Interface;
using TaskManagement.Models;

namespace TaskManagement.Repository
{
    public class WorkSpaceRepository : IWorkSpaceRepository
    {
        private readonly TaskManagementContext _context;

        public WorkSpaceRepository(TaskManagementContext context)
        {
            _context = context;
        }

        public bool AddMemberIntoWorkspace(int workSpaceID, string nameUser, int roleID)
        {
            var user = _context.Users.Where(u => u.UserName == nameUser).FirstOrDefault();
            var role = _context.Roles.Where(r => r.Id == roleID).FirstOrDefault();
            var workSpace = _context.WorkSpaces.Where(w => w.Id == workSpaceID).FirstOrDefault();

            var userWorkSpace = new UserWorkSpaceRole
            {
                UserId = user.Id,
                RoleId = roleID,
                WorkSpaceId= workSpaceID,
                //Role = role,
                //User = user,
                //WorkSpace = workSpace
            };
            _context.UserWorkSpaceRoles.Add(userWorkSpace);
            return Save();
        }

        public bool CreateWorkSpace(int userID, int roleID, WorkSpace workSpaceCreate)
        {
            var user = _context.Users.Where(u => u.Id == userID).FirstOrDefault();
            var role = _context.Roles.Where(r => r.Id == roleID).FirstOrDefault();

            var userWorkSpace = new UserWorkSpaceRole
            {
                UserId= userID,
                RoleId= roleID,
                WorkSpaceId= workSpaceCreate.Id,
                Role = role,
                User = user,
                WorkSpace = workSpaceCreate
            };
            _context.UserWorkSpaceRoles.Add(userWorkSpace);
            _context.WorkSpaces.Add(workSpaceCreate);
            return Save();
        }

        

        public bool DeleteWorkSpace(WorkSpace workSpace)
        {
            _context.Remove(workSpace);
            return Save();
        }

        public ICollection<Section> GetSectionsByWorkSpace(int workSpaceID)
        => _context.Sections.Where(s => s.WorkSpace.Id == workSpaceID).ToList();

        public ICollection<User> GetUsersByWorkSpace(int workSpaceID)
        => _context.UserWorkSpaceRoles.Where(w=> w.WorkSpaceId== workSpaceID)
            .Select(u => u.User).ToList();

        public WorkSpace GetWorkSpaceByID(int workSpaceID)
        => _context.WorkSpaces.Where(w => w.Id == workSpaceID).FirstOrDefault();

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool UpdateWorkSpace(WorkSpace workSpace)
        {
            _context.Update(workSpace);
            return Save();  
        }

        public bool WorkSpaceExists(int workSpaceID)
        => _context.WorkSpaces.Any(p => p.Id==workSpaceID);

        public ICollection<WorkSpace> WorkSpaces()
        => _context.WorkSpaces.ToList();
    }
}
