using TaskManagement.Models;

namespace TaskManagement.Interface
{
    public interface IUserRepository
    {   
        ICollection<User> GetUsers();
        User GetUserByID(int userId);
        bool UserExists(int userId);
        bool CreateUser(User user); 
        bool UpdateUser(User user);
        bool DeleteUser(User user);
        bool Save();
    }
}
