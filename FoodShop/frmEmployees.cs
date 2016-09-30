using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.ComponentModel;
using System.Collections.Generic;

namespace FoodShop
{
    public partial class frmEmployees : Form
    {
        const string EXCLUDED_CONTROL_LIST = "Label|Button";
        const string RETURN_ID_SQL = "Select @ID = SCOPE_IDENTITY(); ";

        enum PAGE
        {
            SEARCH,
            LIST,
            DETAIL
        }

        public Dictionary<string, Control> controlMap = new Dictionary<string, Control>();
        private List<Employee> listEmployees = new List<Employee>();
        private List<EmployeeView> listEmployeesView = new List<EmployeeView>();
        private EmployeeView employeeview = new EmployeeView();
        private Employee employee = new Employee();
        private string insertSQL = string.Empty;
        private string updateSQL = string.Empty;
        private string selectSQL = string.Empty;
       
        public frmEmployees()
        {
            InitializeComponent();
        }

        private void frmEmployees_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            this.tcPage.SelectedIndex = (int)PAGE.SEARCH;
            this.dgvGrid.MultiSelect = false;
            this.lblMessage.Text = string.Empty;
            this.txtID.Visible = false;
            this.txtLastNameSearch.Focus();

            insertSQL = Util.GetInsertSQL(new Employee());
            updateSQL = Util.GetUpdateSQL(new Employee());

            foreach (Control page in this.tcPage.TabPages) 
            {
                var availControls = Util.GetControls(page, EXCLUDED_CONTROL_LIST);
                Util.MapControls(page.Tag.ToString(), availControls, ref controlMap);
            }

            var sql = "SELECT [ID], [JobName] FROM dbo.Jobs ";
            DataTable dt = Util.GetData(Util.GetConnectionString(), sql);
            Util.LoadCombo(dt, ref this.cboJobID, "[Select]");
            Util.LoadCombo(dt, ref this.cboJobIDSearch, "[Select]");

            sql = "SELECT [ID], [StoreName] FROM dbo.Stores ";
            dt = Util.GetData(Util.GetConnectionString(), sql);
            Util.LoadCombo(dt, ref this.cboStoreID, "[Select]");
            Util.LoadCombo(dt, ref this.cboStoreIDSearch, "[Select]");
        }

        private void dgvGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            LoadEditData();
        }

        private void LoadEditData() 
        {
            if (this.dgvGrid.CurrentCell == null)
            {
                MessageBox.Show("Please select a record first before pressing [Edit].");
                return;
            }

            var currentID = Convert.ToInt32(this.dgvGrid.Rows[this.dgvGrid.CurrentCell.RowIndex]
                .Cells["ID"].Value.ToString());

            GetData(new EmployeeView(currentID), new EmployeeView());
        }

        private void GetData<T>(T bobj, T init) 
        {
            var className = bobj.GetType().Name.ToString();

            var tableOrView = className;
            tableOrView = (!className.ToLower().EndsWith("view") && !className.EndsWith("s")) ?
                className + "s" : className;

            var whereClause = Util.BuildWhereClause<T>(bobj, init);
            var sql = string.Format("Select * From {0} {1}; ", tableOrView, whereClause);

            DataTable dt = Util.GetData(Util.GetConnectionString(), sql, Util.GetSQLParameters(bobj, sql));
                            
            var listEmployeesView = new List<EmployeeView>();

            foreach (DataRow dr in dt.Rows)
                listEmployeesView.Add(new EmployeeView(dr));

            if (listEmployeesView.Count != 1)
            {
                this.dgvGrid.DataSource = listEmployeesView;

                Util.HideGridIDColumns(this.dgvGrid);
                this.dgvGrid.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                this.dgvGrid.ReadOnly = true;

                this.tcPage.SelectedIndex = (int)PAGE.LIST;
            }
            else 
            {
                this.tcPage.SelectedIndex = (int)PAGE.DETAIL;
                Util.LoadControlsFromObject(listEmployeesView[0], controlMap);
                this.lblMessage.Text = string.Empty;
                this.btnSave.Enabled = false;
                this.btnCopy.Enabled = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var employee = new Employee(controlMap, "detail");
            var sql = (employee.ID == 0) ? insertSQL : updateSQL;

            var parmsReturn = Util.UpdateData(Util.GetConnectionString(), sql, 
                Util.GetSQLParameters(employee, sql));

            this.btnSave.Enabled = false;
            this.btnCopy.Enabled = true;
            this.lblMessage.Text = "Data successfully saved.";
        }

        private void btnExitDetail_Click(object sender, EventArgs e)
        {
            if (this.btnSave.Enabled)
            {
                if (MessageBox.Show("Are you sure you want to leave without saving changes?",
                    "Please respond", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    return;
            }
            GetData(new EmployeeView(), new EmployeeView());
            this.tcPage.SelectedIndex = (int)PAGE.LIST;
        }

        private void frmEmployees_KeyPress(object sender, KeyPressEventArgs e)
        {
            this.btnSave.Enabled = true;
            this.btnCopy.Enabled = false;
            this.lblMessage.Text = string.Empty;
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            LoadEditData();
        }

        private void btnCloseSearch_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var employeeView = new EmployeeView(controlMap, "search");
            GetData(employeeView, new EmployeeView());
        }

        private void btnCloseList_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            this.tcPage.SelectedIndex = (int)PAGE.DETAIL;
            this.txtLastName.Focus();
            this.txtID.Text = "0";
            this.btnSave.Enabled = false;
        }

        private void btnClearDetail_Click(object sender, EventArgs e)
        {
            //var btn = sender as Button;
            //MessageBox.Show(string.Format("[Button] {0} with [Parent] {1}.", btn.Name, btn.Parent.Name));
            Util.ClearData(this.tcPage.TabPages[((Control)sender).Parent.Name]);

            //Util.ClearData(this.tcPage.TabPages[(int)PAGE.DETAIL]);
        }

        private void btnClearSearch_Click(object sender, EventArgs e)
        {
            //Util.ClearData(this.tcPage.TabPages[(int)PAGE.SEARCH]);
            Util.ClearData(this.tcPage.TabPages[((Control)sender).Parent.Name]);
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            this.txtID.Text = "0";
            this.btnSave.Enabled = true;
            this.btnSave.Focus();
            this.btnCopy.Enabled = false;
            this.lblMessage.Text = "Data successfully copied: make changes and save.";
        }

        private void SelectedIndexChanged(object sender, EventArgs e)
        {
            this.btnSave.Enabled = true;
        }
    }
}

