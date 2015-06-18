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
using System.Diagnostics;
using System.IO.Ports;

namespace kAvatar
{
    public partial class Form1 : Form
    {
        SqlConnection conn;
        string connString = "Data Source=INKARNE\\INKARNESERVER; Initial Catalog=InkarneDBS;User ID=PRIYASH;Password=Inkarne123";
        Int64 user_ID;
       
        public Form1()
        {
            conn = new SqlConnection(connString);
            InitializeComponent();
        }


        private DataTable getUserID(Int64 mobile)
        {
            DataTable userDetailsTable = new DataTable();
            userDetailsTable.Columns.Add(new DataColumn("User_ID", typeof(Int64)));
            if (conn.State.ToString() == "Closed")
            {
                conn.Open();
            }

            string query = "SELECT User_ID FROM UserDetails WHERE MobileNo='" + mobile + "';";
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    userDetailsTable.Rows.Add(reader["User_ID"]);
                }
            }
            reader.Close();
            conn.Close();
            return userDetailsTable;
        }
        private Int64 iCreateNewUser(string firstName, string lastName, int age, char gender,Int64 mobile)
        {

            if (conn.State.ToString() == "Closed")
            {
                conn.Open();
            }

            /*string query = "INSERT INTO UserDetails (FirstName,LastName,MobileNo,Password,Gender,Age) VALUES('" + firstName + "','" + lastName + "','" + mobile + "','" + 1414 + "','" + gender + "','" +
                                                               age  +"');";*/

            string query = "INSERT INTO UserDetails (FirstName,LastName,MobileNo,Password,Gender,Age) VALUES(@fname,@lname,@mobileno,@pwd,@gender,@age)";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@fname", SqlDbType.VarChar).Value = this.FirstName.Text;
            cmd.Parameters.Add("@lname", SqlDbType.VarChar).Value = this.LastName.Text;
            cmd.Parameters.Add("@mobileno", SqlDbType.BigInt).Value = Convert.ToInt64(this.mobile.Text);
            cmd.Parameters.Add("@pwd", SqlDbType.VarChar).Value = "1414";
            cmd.Parameters.Add("@gender", SqlDbType.VarChar).Value = this.comboBox1.SelectedItem.ToString();
            cmd.Parameters.Add("@age", SqlDbType.Int).Value = Convert.ToInt32(this.age.Text);

            cmd.ExecuteNonQuery();
            DataTable data = getUserID(mobile);
            DataRow row = data.Rows[0];
            Int64 ID = row.Field<Int64>(0);
            return ID;
        }

        //INITIALIZE SCAN BUTTON
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string fname = this.FirstName.Text.ToString();
                string lname = this.LastName.Text.ToString();
                string mobile = this.mobile.Text.ToString();
                string gender = this.comboBox1.SelectedItem.ToString();
                //string dob = this.dateTimePicker1.Value.ToString("dd-MM-yyyy");
                int _age = Convert.ToInt32(this.age.Text);
                //int _age = 25;
                string kAvatar = "";
                char g = gender[0];

                //CREATION OF NEW USER 
                user_ID = iCreateNewUser(fname, lname, _age, g, Convert.ToInt64(mobile));

                //RECONSTRUCTME ENGINE START
                ReconstructMe reme = new ReconstructMe(user_ID);
                //reme.Start_Scan();

                //PASSING THE VALUES ONTO THE NEXT PAGE
                PostScan ps = new PostScan(user_ID, fname, lname, mobile, gender, _age, kAvatar);
                ps.Show();
                this.FirstName.Text = "";
                this.LastName.Text = "";
                this.age.Text = "";
                this.mobile.Text = "";
                this.comboBox1.SelectedIndex = 0;

            }
            catch(FormatException fe)
            {
                MessageBox.Show("Inkarne Exception : "+fe.Message,"Inkarne_Exception");
            }          
        }
     
        //LOOK UP BUTTON
        private void button2_Click(object sender, EventArgs e)
        {
            LookUpScreen ls = new LookUpScreen();
            ls.Show();
        }
    }
}
