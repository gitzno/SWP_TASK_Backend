using System;
using System.Collections.Generic;

namespace SWP_Login.Models
{
    public partial class Account
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
