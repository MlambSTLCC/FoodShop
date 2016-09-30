using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace FoodShop
{
    class SQLInstance
    {
        public string Text { get; set; }
        public List<SqlParameter> Params { get; set; }

        public SQLInstance() 
        { }
    }
}
