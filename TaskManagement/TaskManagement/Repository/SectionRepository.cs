using TaskManagement.Interface;
using TaskManagement.Models;

namespace TaskManagement.Repository
{
    public class SectionRepository : ISectionRepository
    {
        private readonly TaskManagementContext _context;

        public SectionRepository(TaskManagementContext context)
        {
            _context = context;
        }

        public bool AddMemberIntoSection(int sectionID, string userName, int roleID)
        {
            var user = _context.Users.Where(u => u.UserName == userName).FirstOrDefault();

            var roleSection = new UserSectionRole
            {
                UserId = user.Id,
                RoleId = roleID,
                SectionId = sectionID,
            };
            _context.UserSectionRoles.Add(roleSection);
            return Save();
        }

        public bool CheckUserInWorkSpace(int userID, int workSpaceID)
            => _context.UserWorkSpaceRoles
            .Any(u=> u.WorkSpaceId== workSpaceID && u.UserId == userID);

        public bool CreateSection(int userID, int roleID, Section section)
        {
            var user = _context.Users.Where(u => u.Id == userID).FirstOrDefault();
            var role = _context.Roles.Where(r => r.Id == roleID).FirstOrDefault();

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
            return Save();
        }

        public bool DeleteSection(Section section)
        {
            _context.Sections.Remove(section);
            return Save();
        }

        public Section GetSectionById(int SectionID)
        => _context.Sections.Where(s => s.Id == SectionID).FirstOrDefault();

        public ICollection<Section> GetSections()
        => _context.Sections.ToList();

        public ICollection<Models.Task> GetTasksBySection(int sectionID)
        => _context.Tasks.Where(s => s.Section.Id == sectionID).ToList();

        public ICollection<User> GetUsersBySection(int sectionID)
        => _context.UserSectionRoles.Where(w => w.SectionId == sectionID)
            .Select(u => u.User).ToList();
        public bool Save()
        {
            var saved = _context.SaveChanges(); 
            return saved > 0 ? true : false;
        }

        public bool SectionExists(int sectionID)
        => _context.Sections.Any(s => s.Id == sectionID);

        public bool UpdateSection(Section section)
        {
            _context.Sections.Update(section);
            return Save();  
        }
    }
}
