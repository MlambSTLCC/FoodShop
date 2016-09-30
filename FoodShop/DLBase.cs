using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

namespace FoodShop
{
    public class DLBase
    {
       
        public DLBase() { }

        public DLBase(DataRow row) 
        {
            var columnMap = Util.MapColumns(row);

            foreach (PropertyInfo pi in this.GetType().GetProperties())
            {
                if (columnMap.ContainsKey(pi.Name))
                {
                    object value = row.ItemArray[columnMap[pi.Name]];
                    ReflectionSetValue(pi, value);
                }
            }
        }

        public DLBase(Dictionary<string, Control> controlMap, string tabName) 
        {
            foreach (PropertyInfo pi in this.GetType().GetProperties())
            {
                var controlKey = string.Format("{0}|{1}", tabName, pi.Name).ToLower();
                
                if (controlMap.ContainsKey(controlKey))
                {
                    object value = Util.GetControlValue(controlMap[controlKey]);
                    ReflectionSetValue(pi, value);
                }
            }
        }

        private void ReflectionSetValue(PropertyInfo pi, object value) 
        {
            if (pi.Name.ToLower().Contains("date"))
            {
                DateTime dateTime;
                if (DateTime.TryParse(value.ToString(), out dateTime))
                    value = dateTime;
            }

            if (pi.CanWrite && !string.IsNullOrEmpty(value.ToString()))
            {
                try
                {
                    Int32 result = 0;
                    if(Int32.TryParse(value.ToString(), out result))
                        value = result;

                    pi.SetValue(this, value);
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
