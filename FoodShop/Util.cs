using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Configuration;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace FoodShop
{
    public static class Util
    {
        public static string GetConnectionString() 
        {
            //return ConfigurationManager.ConnectionStrings["DBPath"].ConnectionString;
            return ConfigurationSettings.AppSettings["DBPath"];
        }

        public static void ClearData(Control colCtl)
        {
            for (int i = 0; i < colCtl.Controls.Count; i++)
            {
                if (colCtl.Controls[i] is TextBox)
                    ((TextBox)colCtl.Controls[i]).Text = String.Empty;
                else if (colCtl.Controls[i] is ComboBox)
                    ((ComboBox)colCtl.Controls[i]).SelectedIndex = -1;
                else if (colCtl.Controls[i] is CheckBox)
                    ((CheckBox)colCtl.Controls[i]).Checked = false;
                else if (colCtl.Controls[i].HasChildren)
                    ClearData(colCtl.Controls[i]);
                else
                {
                    //no action
                }
            }
        }

        public static object GetControlValue(Control ctl)
        {
            if (ctl is TextBox)
                return ((TextBox)ctl).Text;
            else if (ctl is ComboBox)
                return Util.GetComboBoxValue((ComboBox)ctl);
            else if (ctl is CheckBox)
                return ((CheckBox)ctl).Checked;
            else
            {
                //no action
            }
            return null;
        }

        public static CommandType GetCommandType(string sql)
        {
            return "select.update.insert".Contains(sql.Trim().ToLower().Split(" ".ToCharArray())[0]) ? CommandType.Text : CommandType.StoredProcedure;
        }

        public static void LoadCombo(DataTable dt, ref ComboBox cbo, 
            string noChoice = null, int colID = 0, int colName = 1) 
        {
            cbo.DisplayMember = "Text";
            cbo.ValueMember = "ID";

            foreach (DataRow row in dt.Rows) 
                cbo.Items.Add(new ListItem(Convert.ToInt32(row[colID]), row[colName].ToString()));

            if (noChoice != null)
            {
                cbo.Items.Insert(0, new ListItem(0, noChoice));
                cbo.SelectedIndex = 0;
            }
        }

        public static object GetComboBoxValue(ComboBox cbo)
        {
            if (cbo.SelectedItem is ListItem)
                return cbo.SelectedItem != null ? (cbo.SelectedItem as ListItem).ID : 0;
            else
                return cbo.Text.Trim();
        }

        public static void SetComboBoxValue(ComboBox cbo, Int32 id)
        {
            foreach (ListItem li in cbo.Items)
            {
                if (li.ID == id)
                {
                    cbo.SelectedIndex = cbo.FindStringExact(li.Text);
                    break;
                }
            }
        }

        public static DataTable GetData(string connectionString, string sql, List<SqlParameter> parms = null, bool throwError = true)
        {
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        conn.Open();
                        cmd.CommandType = GetCommandType(sql);

                        if (parms != null)
                            cmd.Parameters.AddRange(parms.ToArray());

                        using (SqlDataReader dr = cmd.ExecuteReader())
                            if (dr != null) dt.Load(dr);
                    }
                }
            }
            catch (Exception ex) { if (throwError) throw ex; }

            return dt;
        }

        public static List<SqlParameter> UpdateData(string connectionString, string sql, List<SqlParameter> parms = null, bool throwError = true)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        conn.Open();
                        cmd.CommandType = GetCommandType(sql);

                        if (parms != null)
                            cmd.Parameters.AddRange(parms.ToArray());

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex) { if (throwError) throw ex; }

            return parms;
        }

        public static void SaveScreenTo(Form _form, string jpgFilePath, bool fullScreen = false)
        {
            if (fullScreen)
            {
                var _with1 = Screen.PrimaryScreen.WorkingArea;
                Rectangle bounds = new Rectangle(_with1.Top, _with1.Left, _with1.Width, _with1.Height);
                Bitmap bit = new Bitmap(bounds.Width, bounds.Height);
                //Dim bit As New Bitmap(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height)
                Graphics gs = Graphics.FromImage(bit);
                gs.CopyFromScreen(new Point(0, 0), new Point(0, 0), bit.Size);
                bit.Save(jpgFilePath, ImageFormat.Jpeg);
            }
            else
            {
                Rectangle bounds = _form.Bounds;
                Bitmap bit = new Bitmap(bounds.Width, bounds.Height);
                Graphics gs = Graphics.FromImage(bit);
                gs.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                bit.Save(jpgFilePath, ImageFormat.Jpeg);
            }
        }

        public static List<Control> GetControls(Control form, string excludeList)
        {
            var controlList = new List<Control>();

            foreach (Control childControl in form.Controls)
            {
                // Recurse child controls.
                controlList.AddRange(GetControls(childControl, excludeList));

                if (!excludeList.Contains(childControl.GetType().Name))
                    controlList.Add(childControl);
            }
            return controlList;
        }

        public static void MapControls(string group, List<Control> availControls, 
            ref Dictionary<string, Control> controlMap)
        {
            foreach (Control ctl in availControls)
            {
                var controlKey = string.Format("{0}|{1}", group, Util.GetControlAlias(ctl));
                if (!controlMap.ContainsKey(controlKey))
                    controlMap.Add(controlKey, ctl);
            }
        }

        public static void LoadControlsFromObject(object o, Dictionary<string, Control> controlMap)
        {
            foreach (PropertyInfo pi in o.GetType().GetProperties())
            {
                var controlKey = string.Format("{0}|{1}", "detail", pi.Name).ToLower();

                if (controlMap.ContainsKey(controlKey)) 
                {
                    var controlClass = controlMap[controlKey].GetType().Name;

                    if (pi.GetValue(o) == null)
                        continue;

                    switch (controlClass)
                    {
                        case "TextBox":
                            (controlMap[controlKey] as TextBox).Text = pi.GetValue(o).ToString();
                            break;

                        case "ComboBox":
                            ComboBox cbo = controlMap[controlKey] as ComboBox;
                            var value = pi.GetValue(o);
                            int id;

                            if (value != null)
                                if (Int32.TryParse(value.ToString(), out id))
                                    Util.SetComboBoxValue(cbo, id);
                                else
                                    cbo.Text = value.ToString();
                            break;

                        case "ListBox":
                            ListBox lst = controlMap[controlKey] as ListBox;
                            break;

                        case "CheckBox":
                            (controlMap[controlKey] as CheckBox).Checked = (bool)pi.GetValue(o);
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        public static Dictionary<string, Int32> MapColumns(DataRow dr) 
        {
            var columnMap = new Dictionary<string, Int32>();

            for (int i = 0; i < dr.Table.Columns.Count; i++)
                columnMap.Add(dr.Table.Columns[i].ColumnName, i);

            return columnMap;
        }

        public static string GetControlAlias(Control ctl) 
        {
            const string CTL_PREFIXES = "txt|cbo|lst|chk";

            if (ctl.Tag != null)
                if (ctl.Tag.ToString().Length > 0)
                    return ctl.Tag.ToString();

            var alias = ctl.Name;
            foreach (string prefix in CTL_PREFIXES.Split("|".ToCharArray()))
            {
                if (ctl.Name.StartsWith(prefix))
                {
                    alias = ctl.Name.Replace(prefix, string.Empty);
                    break;
                }
            }

            return alias.ToLower();
        }

        public static void HideGridIDColumns(DataGridView dgv)
        {
            for (int i = 0; i < dgv.Columns.Count; i++)
                if (dgv.Columns[i].Name.ToLower().EndsWith("id"))
                    dgv.Columns[i].Visible = false;
        }

        public static string GetInsertSQL<T>(T bobj)
        {
            var propList = new List<string>();
            var valueList = new List<string>();

            foreach (PropertyInfo pi in bobj.GetType().GetProperties())
            {
                if (pi.Name.ToLower() != "id") 
                {
                    valueList.Add(string.Format("@{0}", pi.Name));
                    propList.Add(string.Format("[{0}]", pi.Name));
                }
            }
            var sql = string.Format("Insert Into dbo.{0}s ({1}) Values ({2}); ", bobj.GetType().Name,
                string.Join(", ", propList.ToArray()), string.Join(", ", valueList.ToArray()));
            
            return sql;
        }

        public static string GetUpdateSQL<T>(T bobj)
        {
            var pairList = new List<string>();

            foreach (PropertyInfo pi in bobj.GetType().GetProperties())
                if (pi.Name.ToLower() != "id") 
                    pairList.Add(string.Format("[{0}] = @{0}", pi.Name));
            
            var sql = string.Format("Update dbo.{0}s Set {1} Where [ID] = @ID; ", bobj.GetType().Name,
                string.Join(", ", pairList.ToArray()));
            
            return sql;
        }

        public static List<SqlParameter> GetSQLParameters<T>(T bobj, string sql) 
        {
            var paramList = new List<SqlParameter>();

            foreach (PropertyInfo pi in bobj.GetType().GetProperties())
            {
                if (sql.ToLower().Contains("@" + pi.Name.ToLower()))
                {
                    if (sql.ToLower().StartsWith("select"))
                    {
                        var makeString = pi.GetValue(bobj) as string;
                        object value = (makeString != null) ? 
                            string.Format("%{0}%", pi.GetValue(bobj)) : pi.GetValue(bobj);
                        paramList.Add(new SqlParameter("@" + pi.Name, value));
                    }
                     else
                    {
                        if (pi.GetValue(bobj) == null)
                        {
                            paramList.Add(new SqlParameter("@" + pi.Name, string.Empty));
                        }
                        else if (IsNumeric(pi.GetValue(bobj).ToString()))
                        {
                            paramList.Add(new SqlParameter("@" + pi.Name, pi.GetValue(bobj)));
                        }
                        else 
                        {
                            var makeString = pi.GetValue(bobj) as string;
                            object value = (makeString != null) ? pi.GetValue(bobj) : string.Empty;
                            paramList.Add(new SqlParameter("@" + pi.Name, value));
                        }
                    }
                }
            }
            return paramList;
        }

        public static string BuildWhereClause<T>(T bobj, T init)
        {
            var conditionList = new List<string>();
            var whereClause = string.Empty;

            PropertyInfo[] propBobj = (from c in bobj.GetType().GetProperties() select c).ToArray();
            PropertyInfo[] propInit = (from c in init.GetType().GetProperties() select c).ToArray();

            for (int i = 0; i < propBobj.Count(); i++) 
            {
                if (propBobj[i].Name.ToLower() == "id" && propBobj[i].GetValue(bobj).ToString() !="0")
                {
                    //special handling for ID column
                    conditionList.Add(string.Format("[ID] = @ID", propBobj[i].Name));
                    break;
                }

                object value = propBobj[i].GetValue(bobj);
                var testForString = value as string;
                if (testForString == null)
                    continue;                   //only using string fields for selection now!

                if (propBobj[i].GetValue(bobj) != propInit[i].GetValue(init))
                    conditionList.Add(string.Format("[{0}] Like @{0}", propBobj[i].Name));
            }

            if (conditionList.Count() > 0)
                whereClause = string.Format("Where {0}", String.Join(" And ", conditionList.ToArray()));

            return whereClause;
        }

        public static bool IsNumeric(string s)
        {
            double myNum = 0;
            if (Double.TryParse(s, out myNum))
            {
                if (s.Contains(",")) return false;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}


//### to return inserted ID ###
//var outputID = new SqlParameter("@ID", SqlDbType.Int);
//outputID.Direction = ParameterDirection.Output;
//var parmsReturn = Util.UpdateData(Util.GetConnectionString(), sql,
//    new List<SqlParameter>() { outputID });

//outputID = parmsReturn[0];

//var id = Int32.Parse(outputID.Value.ToString());            if 

