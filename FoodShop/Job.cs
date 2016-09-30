using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodShop
{
    public class Job : DLBase
    {
        public Int32 ID { get; set; }
        public string JobName { get; set; }
        public decimal BaseRate { get; set; }

        public Job() { }

        public Job(DataRow dr) : base(dr)
        {
        }

        public override string ToString()
        {
            return JobName;
        }
    }
}

