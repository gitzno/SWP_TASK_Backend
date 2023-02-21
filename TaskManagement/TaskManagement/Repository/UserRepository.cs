using TaskManagement.Interface;
using TaskManagement.Models;
using static System.Collections.Specialized.BitVector32;

namespace TaskManagement.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly TaskManagementContext _context;

        public UserRepository(TaskManagementContext context)
        {
            _context = context;
        }

        public bool CreateUser(User user)
        {
            _context.Users.Add(user);
            return Save();
        }

        public bool DeleteUser(User user)
        {
           _context.Remove(user);
            return Save();
        }

        public User GetUserByID(int userId)
        {
            return _context.Users.Where(u => u.Id == userId).FirstOrDefault();
        }

        public ICollection<User> GetUsers()
        {
          return _context.Users.ToList();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool UpdateUser(User user)
        {
           _context.Update(user);
            return Save();
        }

        public bool UserExists(int userId)
        {
           return _context.Users.Any(u => u.Id == userId);
        }

        public bool UserNameExists(string userName)
        => _context.Users.Any(u => u.UserName == userName); 
    }
}
