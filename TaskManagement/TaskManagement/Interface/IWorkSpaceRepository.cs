using TaskManagement.Models;

namespace TaskManagement.Interface
{
    public interface IWorkSpaceRepository
    {
        ICollection<WorkSpace> WorkSpaces();
        WorkSpace GetWorkSpaceByID(int workSpaceID);
        ICollection<Section> GetSectionsByWorkSpace(int workSpaceID);
        
        // danh sach thanh vien trong ws
        ICollection<User> GetUsersByWorkSpace(int workSpaceID);
        bool AddMemberIntoWorkspace(int workSpaceID, string userName, int roleID);
        bool CreateWorkSpace(int userID, int roleID, WorkSpace workSpaceCreate);
        bool UpdateWorkSpace(WorkSpace workSpace);
        bool DeleteWorkSpace(WorkSpace workSpace);
        bool WorkSpaceExists(int workSpaceID);
        bool Save();


    }
}
