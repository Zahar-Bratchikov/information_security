using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserAccessControl
{
    public class User
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public bool IsBlocked { get; set; }
        public bool PasswordRestrictions { get; set; }

        public User(string name, string password, bool isBlocked, bool passwordRestrictions)
        {
            Name = name;
            Password = password;
            IsBlocked = isBlocked;
            PasswordRestrictions = passwordRestrictions;
        }
    }
}