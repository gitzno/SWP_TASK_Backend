using Microsoft.AspNetCore.Mvc;
using SWP_Login.Models;
using SWP_Login.Utils;

namespace SWP_Login.Service
{
    public interface IAccountService
    {
        ResponseObject login(Account model);
    }
}
