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
    public partial class Form1 : Form
    {
        private SqlConnection sqlConnection = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView lv = new ListView();

            lv = (ListView)sender;

            if (lv.SelectedItems.Count > 0)
            {
                listView2.SelectedItems.Clear();
            }

        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListView lv = new ListView();

            lv = (ListView)sender;

            if (lv.SelectedItems.Count > 0)
            {
                listView1.SelectedItems.Clear();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string flagMod = string.Empty;
            if (radioButton1.Checked) { flagMod = "2"; }
            else { flagMod = "1"; }

            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    string empID = listView1.SelectedItems[0].SubItems[3].Text;

                    execMove(empID, false, flagMod);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
            }
            else if (listView2.SelectedItems.Count > 0)
            {
                try
                {
                    string empID = listView2.SelectedItems[0].SubItems[3].Text;

                    execMove(empID, true, flagMod);
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

            updateLV_part1();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["cS_db"].ConnectionString);

            //Open connection to database 
            sqlConnection.Open();
            //Функция заполнения(обновления) для ListView
            updateLV_part1();
            updateLV_part2();
        }


        private void updateLV_part1()
        {
            listView1.Items.Clear();
            listView2.Items.Clear();

            SqlDataReader dataReader = null;

            try
            {
                string sqlQuery = "SELECT id_emp, ts_lastName, ts_firstName, ts_route, phoneNumber  FROM timesheet AS ab INNER JOIN employe ON employe.uid_emp = id_emp WHERE ts_time = (SELECT MAX(ts_time) FROM timesheet WHERE id_emp = ab.id_emp) AND ts_date = (SELECT MAX(ts_date) FROM timesheet WHERE id_emp = ab.id_emp)";

                SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlCommand.CommandType = System.Data.CommandType.Text;

                dataReader = sqlCommand.ExecuteReader();

                ListViewItem item = null;

                while (dataReader.Read())
                {
                    item = new ListViewItem(new string[]
                    { Convert.ToString(dataReader["ts_lastName"]),
                      Convert.ToString(dataReader["ts_firstName"]),
                      Convert.ToString(dataReader["ts_route"]),
                      Convert.ToString(dataReader["id_emp"]),
                      Convert.ToString(dataReader["phoneNumber"])});

                    if (Convert.ToString(dataReader["ts_route"]) == "Вход")
                    {
                        listView2.Items.Add(item);
                    }
                    else if (Convert.ToString(dataReader["ts_route"]) == "Выход")
                    {
                        listView1.Items.Add(item);
                    }
                    else { MessageBox.Show("Ошибка получаемых параметров 'ts_route' ", "Error",MessageBoxButtons.OK, MessageBoxIcon.Error); }
                    
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

        private void updateLV_part2()
        {
            listView3.Items.Clear();

            SqlDataReader dataReader = null;

            try
            {
                string sqlQuery = "SELECT lastName, firstName, middleName, code FROM g_code INNER JOIN employe ON employe.uid_emp = g_code.id_emp WHERE code IS NOT NULL";

                SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlCommand.CommandType = System.Data.CommandType.Text;

                dataReader = sqlCommand.ExecuteReader();

                ListViewItem item = null;

                while (dataReader.Read())
                {
                    item = new ListViewItem(new string[]
                    { Convert.ToString(dataReader["lastName"]) + " " +
                      Convert.ToString(dataReader["firstName"]) + " " +
                      Convert.ToString(dataReader["middleName"]),
                      Convert.ToString(dataReader["code"])
                      });

                    listView3.Items.Add(item);

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
            updateLV_part2();
            updateLV_part1();
        }


        private void execMove(string idEmp,bool flagPosition, string flagMod) // true - значит уже на парковке, false - за пределами парковки
        {
            try
            {
                string id = idEmp;
                string route = string.Empty;

                //Если мы сейчас на парковке (flagPosition true), нам нужно отправить запрос на выезд (route 0)
                if (flagPosition)
                {
                    route = "0";
                }
                else
                {
                    route = "1";
                }

                string sqlQuery = "DECLARE @gcode INT SELECT @gcode = code FROM g_code WHERE g_code.id_emp = N'"+id+"' EXEC AddInformationIntoTimeSheet @gcode, "+route+", "+flagMod+"; ";

                SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConnection);

                sqlCommand.CommandType = System.Data.CommandType.Text;


                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exeption", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (Form2 form = new Form2())
            {
                form.ShowDialog();
            }
            updateLV_part1();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Включить режим ЧП?", "Вы уверены?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                for (int i = 0; i < listView2.Items.Count; i++)
                {
                    try
                    {
                        string empID = listView2.Items[i].SubItems[3].Text;
                        //MessageBox.Show(empID);
                        execMove(empID, true, "99");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                updateLV_part1();
            }
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Данная программа предназначена для симуляции въезда и выезда, на и из парковки соответственно. Работает совместно с общей БД, под управлением СУБД APS Desktop и предназначена исключительно для её тестирования. \n Author Voloshenko Vadim", "О программе", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
