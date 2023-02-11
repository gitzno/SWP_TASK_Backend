using SWP_Login.Models;

namespace SWP_Login.Repository
{
    public interface IAccountRepository
    {
         Account login(Account acc);
    }
}
