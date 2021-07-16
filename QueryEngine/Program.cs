using System;
using System.Collections.Generic;

namespace QueryEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            #region TEST_DATA
            // TEST DATA
            List<User> users = new List<User>();
            users.Add(new User("selected.databases@ravendb.net", "John Doe", 44));
            users.Add(new User("jobs@ravendb.net", "foo", 44));
            users.Add(new User("jobs@hibernatingrhinos.com", "kayle", 22));
            users.Add(new User("selected.databases@ravendb.net", "bar", 66));
            users.Add(new User("kayle@ravendb.net", "John Doe", 12));
            users.Add(new User("ash@ravendb.net", "foo", 130));

            Data data = new Data { Users = users };
            #endregion

            // Example of use Engine
            Engine engine = new Engine(data);
            string sql =    "from Users " +
                            "where (FullName = 'foo' or FullName = 'bar' and True) and Age < 99 " +
                            "select Email, Age";
            try
            {
                engine.Query(sql);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }
        }
    }
}
