using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodShop
{
    public class ListItem
    {
        public Int32 ID { get; set; }
        public string Text { get; set; }
                
        public ListItem(Int32 id, string text) 
        {
            this.ID = id;
            this.Text = text;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
