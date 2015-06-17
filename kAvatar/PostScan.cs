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
    public partial class PostScan : Form
    {
        string fname, lname, mobile, gender, kAvatar;
        int age;
        Int64 user_ID;
        SqlConnection conn;
        string connString = "Data Source=INKARNE\\INKARNESERVER; Initial Catalog=InkarneDBS;User ID=PRIYASH;Password=Inkarne123";

        public PostScan()
        {
            InitializeComponent();
        }

        public PostScan(Int64 user_ID,string fname,string lname,string mobile,string gender,int age,string kAvatar)
        {
            InitializeComponent();
            conn = new SqlConnection(connString);
            this.user_ID = user_ID;
            this.fname = fname;
            this.lname = lname;
            this.mobile = mobile;
            this.gender = gender;
            this.age = age;
            this.kAvatar = kAvatar;
        }

        
        private void iUserMeasurementsDB(string bodyType)
        {

            if (conn.State.ToString() == "Closed")
            {
                conn.Open();
            }

            //string query = "INSERT INTO UserDetails(UserBodyType) VALUES('"+bodyType + "')";
            string query = "UPDATE UserDetails SET UserBodyType=" + "'" + bodyType + "' " + "WHERE User_ID=" + user_ID;
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.ExecuteNonQuery();

           

            if (conn.State.ToString() == "Closed")
            {
                conn.Open();
            }

            // SECOND INSERTION OF THE INKARNE DATABASE INTO USER MEASUREMENTS TABLE
            SqlCommand cmd2 = new SqlCommand();
            cmd2.Connection = conn;
            cmd2.CommandText = "INSERT INTO UserMeasurements(User_ID,Weight,Height,Bust_Circumference,Waist_Circumference,Hips_Circumference,Neck_Circumference,Under_Bust_Circumference,Wrist_Circumference,Thigh_Circumference,Waist_to_hip,Upper_arm,Arm_length,Inseam,Outseam,Nape_to_Hip,Jacket_length,Shoulder_width,Short_sleeve_length,Waist_to_knee,Arm_hole,Half_hem,Nape_to_feet,Bust_to_Bust_point,Shoulder_to_pivot,Jacket_Waist_Circumference)" +
                                                             "VALUES(@user_id,@weight,@height,@bust_circum,@waist_circum,@hips_circum,@Neck_Circumference,@Under_Bust_Circumference,@Wrist_Circumference,@Thigh_Circumference,@Waist_to_hip,@Upper_arm,@Arm_length,@Inseam,@Outseam,@Nape_to_Hip,@Jacket_length,@Shoulder_width,@Short_sleeve_length,@Waist_to_knee,@Arm_hole,@Half_hem,@Nape_to_feet,@Bust_to_Bust_point,@Shoulder_to_pivot,@Jacket_Waist_Circumference)";

            cmd2.Parameters.Add("@user_id", SqlDbType.BigInt).Value = user_ID;
            cmd2.Parameters.Add("@weight", SqlDbType.Float).Value = float.Parse(this.weight.Text.ToString());
            cmd2.Parameters.Add("@height", SqlDbType.Float).Value = float.Parse(this.height.Text.ToString());
            cmd2.Parameters.Add("@bust_circum", SqlDbType.Float).Value = float.Parse(this.bust_circum.Text.ToString());
            cmd2.Parameters.Add("@waist_circum", SqlDbType.Float).Value = float.Parse(this.waist_circum.Text.ToString());
            cmd2.Parameters.Add("@hips_circum", SqlDbType.Float).Value = float.Parse(this.hips_circum.Text.ToString());


            cmd2.Parameters.Add("@Neck_Circumference", SqlDbType.BigInt).Value = Convert.ToInt64(this.neck_circum.Text.ToString());
            cmd2.Parameters.Add("@Under_Bust_Circumference", SqlDbType.BigInt).Value = Convert.ToInt64(this.under_bust_circum.Text.ToString());
            cmd2.Parameters.Add("@Wrist_Circumference", SqlDbType.BigInt).Value = Convert.ToInt64(this.wrist_circum.Text.ToString());
            cmd2.Parameters.Add("@Thigh_Circumference", SqlDbType.BigInt).Value = Convert.ToInt64(this.thigh_circum.Text.ToString());
            cmd2.Parameters.Add("@Waist_to_hip", SqlDbType.BigInt).Value = Convert.ToInt64(this.waist_to_hip.Text.ToString());
            cmd2.Parameters.Add("@Upper_arm", SqlDbType.BigInt).Value = Convert.ToInt64(this.upper_arm.Text.ToString());
            cmd2.Parameters.Add("@Arm_length", SqlDbType.BigInt).Value = Convert.ToInt64(this.arm_length.Text.ToString());
            cmd2.Parameters.Add("@Inseam", SqlDbType.BigInt).Value = Convert.ToInt64(this.inseam.Text.ToString());
            cmd2.Parameters.Add("@Outseam", SqlDbType.BigInt).Value = Convert.ToInt64(this.outseam.Text.ToString());
            cmd2.Parameters.Add("@Nape_to_Hip", SqlDbType.BigInt).Value = Convert.ToInt64(this.nape_to_hip.Text.ToString());
            cmd2.Parameters.Add("@Jacket_length", SqlDbType.BigInt).Value = Convert.ToInt64(this.Jacket_length.Text.ToString());
            cmd2.Parameters.Add("@Shoulder_width", SqlDbType.BigInt).Value = Convert.ToInt64(this.Shoulder_width.Text.ToString());
            cmd2.Parameters.Add("@Short_sleeve_length", SqlDbType.BigInt).Value = Convert.ToInt64(this.Short_sleeve_length.Text.ToString());
            cmd2.Parameters.Add("@Waist_to_knee", SqlDbType.BigInt).Value = Convert.ToInt64(this.waist_to_knee.Text.ToString());
            cmd2.Parameters.Add("@Arm_hole", SqlDbType.BigInt).Value = Convert.ToInt64(this.arm_hole.Text.ToString());
            cmd2.Parameters.Add("@Half_hem", SqlDbType.BigInt).Value = Convert.ToInt64(this.half_hem.Text.ToString());
            cmd2.Parameters.Add("@Nape_to_feet", SqlDbType.BigInt).Value = Convert.ToInt64(this.nape_to_feet.Text.ToString());
            cmd2.Parameters.Add("@Bust_to_Bust_point", SqlDbType.BigInt).Value = Convert.ToInt64(this.Bust_to_Bust_point.Text.ToString());
            cmd2.Parameters.Add("@Shoulder_to_pivot", SqlDbType.BigInt).Value = Convert.ToInt64(this.Shoulder_To_Pivot.Text.ToString());
            cmd2.Parameters.Add("@Jacket_Waist_Circumference", SqlDbType.BigInt).Value = Convert.ToInt64(this.Jacket_To_Waist_Circumference.Text.ToString());


            if(cmd2.ExecuteNonQuery()==1)
            {
                MessageBox.Show("Inserted Into Inkarne Database Successfully!");
            }
            conn.Close();
    
        }
        private void iUserAvatar(Int64 User_ID, string PB_ID)
        {
            if (conn.State.ToString() == "Closed")
            {
                conn.Open();
            }

            string query3 = "INSERT INTO UserAvatar (User_ID,AvatarStatus,PB_ID) VALUES('" + User_ID + "','" + 0 + "','" + PB_ID + "');";
            SqlCommand cmd3 = new SqlCommand(query3, conn);
            cmd3.ExecuteNonQuery();
            conn.Close();
        }

        //USER REGISTER BUTTON
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                char g = gender[0];
                //CLOSEST PB BASED ON USER HEIGHT,BUST,WAIST AND HIPS
                PBIdentification pb = new PBIdentification();
                string[] str1 = pb.PB_CATEGORIZER(g, float.Parse(this.height.Text.ToString()), float.Parse(this.bust_circum.Text.ToString()), float.Parse(this.waist_circum.Text.ToString()), float.Parse(this.hips_circum.Text.ToString()));
                string PBID = str1[0];
                float h1 = float.Parse(str1[1]);
                float h2 = float.Parse(str1[2]);
                string bodyType = pb.getBodyType();

                //INSERTING BODY_TYPE INTO USER_DETAILS
                iUserMeasurementsDB(bodyType);

                //USER AVATAR BASED ON PBID AND USERID
                iUserAvatar(user_ID, PBID);

                //HEAD STITCHING STARTS HERE 
                HeadStitch hs = new HeadStitch(h1, h2, user_ID, PBID);
                hs.copy();
                //hs.headStitchStart();

                //CLEANING AND MAKING FOLDER STARTS HERE
                Cleaner cl = new Cleaner(user_ID, PBID);
                //cl.MakeFolderAndClean();
                this.Close();
            }
            catch(Exception _e)
            {
                MessageBox.Show("Inkarne Exception! : " + _e.Message,"Inkarne_Exception");
            }
        }
        //LOOK UP BUTTON
        private void button1_Click(object sender, EventArgs e)
        {
            LookUpScreen ls = new LookUpScreen();
            ls.Show();
        }

        private void weight_validate(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar=='.')
            {
                if(this.weight.Text.Contains(".")&&!this.weight.SelectedText.Contains("."))
                {
                    e.Handled = true;
                }
            }
            else if(!Char.IsDigit(e.KeyChar)&&e.KeyChar!=0x08)
            {
                e.Handled = true;
            }
        }
    }
}
