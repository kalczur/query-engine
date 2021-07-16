using System;
using System.Collections.Generic;
using System.Text;

namespace QueryEngine
{
    public class User
    {
        public string Email;
        public string FullName;
        public int Age;

        public User(string email, string fullName, int age)
        {
            this.Email = email;
            this.FullName = fullName;
            this.Age = age;
        }
    }
}
