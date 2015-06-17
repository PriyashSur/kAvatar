using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace kAvatar
{
    public partial class LookUpScreen : Form
    {
        SqlConnection conn;
        string connString = "Data Source=INKARNE\\INKARNESERVER; Initial Catalog=InkarneDBS;User ID=PRIYASH;Password=Inkarne123";
        public LookUpScreen()
        {
            conn = new SqlConnection(connString);
            InitializeComponent();
        }

        //SEARCH BUTTON 
        private void button3_Click(object sender, EventArgs e)
        {
            if (searchBox.Text.ToString() != "")
            {
                try
                {
                    if (conn.State.ToString() == "Closed")
                    {
                        conn.Open();
                    }

                    string query = "SELECT FirstName,LastName,Gender,Age,MobileNo FROM UserDetails WHERE FirstName=@fname OR LastName=@lname OR Gender=@gender";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.Add("@fname", SqlDbType.VarChar).Value = this.searchBox.Text;
                    cmd.Parameters.Add("@lname", SqlDbType.VarChar).Value = this.searchBox.Text;
                    cmd.Parameters.Add("@gender", SqlDbType.VarChar).Value = this.searchBox.Text;
                    //cmd.Parameters.Add("@mobileno", SqlDbType.BigInt).Value = Convert.ToInt64(this.searchBox.Text);
                    
                    DataTable dt = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    this.dataGridView1.DataSource = dt;
                    conn.Close();
                }
                catch(FormatException fe)
                {
                    MessageBox.Show("INKARNE EXCEPTION! "+fe.Message);
                }
                catch(SqlException sqe)
                {
                    MessageBox.Show("INKARNE EXCEPTION! " + sqe.Message);
                }
            }
            else
            {
                MessageBox.Show("Search Box is Empty!!!");
            }
        }
    }
}
