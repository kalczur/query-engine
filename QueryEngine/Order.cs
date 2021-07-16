using System;
using System.Collections.Generic;
using System.Text;

namespace QueryEngine
{
    class Order
    {
        public int Id;
        public string Name;
        public string Date;

        public Order(int id, string name, string date)
        {
            this.Id = id;
            this.Name = name;
            this.Date = date;
        }
    }
}
