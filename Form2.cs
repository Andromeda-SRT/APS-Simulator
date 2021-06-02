using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Configuration;
using System.Data;
using System.Data.SqlClient;


namespace APS_Simulator
{
    public partial class Form2 : Form
    {
        private SqlConnection sqlConnection = null;

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["cS_db"].ConnectionString);

            //Open connection to database 
            sqlConnection.Open();
            //Функция заполнения(обновления) для ListView
            updateLV();
        }

        private void updateLV()
        {
            listView1.Items.Clear();

            SqlDataReader dataReader = null;

            try
            {
                string sqlQuery = "SELECT uid_emp, lastName, firstName, middleName, phoneNumber FROM employe LEFT JOIN timesheet ON employe.uid_emp = timesheet.id_emp WHERE timesheet.id_emp IS NULL";

                SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlCommand.CommandType = System.Data.CommandType.Text;

                dataReader = sqlCommand.ExecuteReader();

                ListViewItem item = null;

                while (dataReader.Read())
                {
                    item = new ListViewItem(new string[]
                    { Convert.ToString(dataReader["uid_emp"]),
                      Convert.ToString(dataReader["lastName"]) + " " +
                      Convert.ToString(dataReader["firstName"]) + " " +
                      Convert.ToString(dataReader["middleName"]),
                      Convert.ToString(dataReader["phoneNumber"])
                      });

                    listView1.Items.Add(item);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exeption", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (dataReader != null && !dataReader.IsClosed)
                {
                    dataReader.Close();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            updateLV();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    string empID = listView1.SelectedItems[0].SubItems[0].Text;

                    execMove(empID);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
            else
            {
                MessageBox.Show("Необходимо выбрать элемнт списка", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            updateLV();
        }


        private void execMove(string idEmp) // true - значит уже на парковке, false - за пределами парковки
        {
            try
            {
                string id = idEmp;
                string route = "1";

                string sqlQuery = "DECLARE @gcode INT SELECT @gcode = code FROM g_code WHERE g_code.id_emp = N'" + id + "' EXEC AddInformationIntoTimeSheet @gcode, " + route + ", 1; ";

                SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlCommand.CommandType = System.Data.CommandType.Text;


                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exeption", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
