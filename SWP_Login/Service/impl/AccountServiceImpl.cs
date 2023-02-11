using Microsoft.AspNetCore.Mvc;
using SWP_Login.Models;
using SWP_Login.Repository;
using SWP_Login.Repository.impl;
using SWP_Login.Utils;

namespace SWP_Login.Service.impl
{
    public class AccountServiceImpl : IAccountService
    {
        public ResponseObject login(Account model)
        {
            Console.WriteLine("service");

            IAccountRepository accountRepository = new AccountRepository();
            if (accountRepository.login(model) != null)
            {
                return new ResponseObject
                {
                    Status = "500",
                    Message = "Login successfully",
                    Data = GenerateToken.GenerateMyToken(model)
                };
            }
            else
            {
                return new ResponseObject
                {
                    Status = "400",
                    Message = "Login failed",
                    Data = null
                };
            }
        }

    }
}
