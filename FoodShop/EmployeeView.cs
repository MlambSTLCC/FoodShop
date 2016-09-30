using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FoodShop
{
    public class EmployeeView : Employee
    {
        public string StoreName { get; set; }
        public string JobName { get; set; }

        public EmployeeView() { }

        public EmployeeView(Int32 id) 
        {
            ID = id;
        }

        public EmployeeView(DataRow dr) : base(dr)
        {
        }

        public EmployeeView(Dictionary<string, Control> controlMap, string tabName) 
            : base(controlMap, tabName) 
        {
        }
    }
}

