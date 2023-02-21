using TaskManagement.Models;
using Task = TaskManagement.Models.Task;

namespace TaskManagement.Interface
{
    public interface ISectionRepository
    {
        ICollection<Section> GetSections();
        Section GetSectionById(int SectionID);
        ICollection<Task> GetTasksBySection(int sectionID);
        ICollection<User> GetUsersBySection(int sectionID);
        bool AddMemberIntoSection(int sectionID, string userName, int roleID);
        bool CheckUserInWorkSpace(int userID, int workSpaceID);
        bool CreateSection(int userID, int roleID, Section section);
        bool UpdateSection(Section section);    
        bool DeleteSection(Section section);
        bool SectionExists(int sectionID);
        bool Save();

    }
}
