using SWP_Login.Models;

namespace SWP_Login.Repository.impl
{
    public class AccountRepository : IAccountRepository
    {

        public Account login(Account acc)
        {
            Console.WriteLine("repository");
            using (var context = new TaskManagementContext())
            {
                var accDB = context.Accounts
                    .Where(u => u.UserName == acc.UserName && u.Password == acc.Password)
                    .FirstOrDefault();
                if (accDB != null)
                {
                    return acc;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
