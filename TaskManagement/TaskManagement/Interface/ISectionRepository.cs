using TaskManagement.Models;

namespace TaskManagement.Interface
{
    public interface ISectionRepository
    {
        ICollection<Section> GetSections();
        Section GetSectionById(int SectionID);
        bool CreateSection(int userID, int roleID, Section section);
        bool UpdateSection(Section section);    
        bool DeleteSection(Section section);
        bool SectionExists(int sectionID);
        bool Save();

    }
}
