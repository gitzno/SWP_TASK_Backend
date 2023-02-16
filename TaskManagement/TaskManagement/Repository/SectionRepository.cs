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
