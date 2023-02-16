using TaskManagement.Models;

namespace TaskManagement.Interface
{
    public interface IWorkSpaceRepository
    {
        ICollection<WorkSpace> WorkSpaces();
        WorkSpace GetWorkSpaceByID(int workSpaceID);
        ICollection<Section> GetSectionsByWorkSpace(int workSpaceID);
        bool CreateWorkSpace(int userID, int roleID, WorkSpace workSpaceCreate);
        bool UpdateWorkSpace(WorkSpace workSpace);
        bool DeleteWorkSpace(WorkSpace workSpace);
        bool WorkSpaceExists(int workSpaceID);
        bool Save();


    }
}
