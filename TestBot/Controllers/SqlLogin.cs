using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Data;
using System.Threading;
using System.Device.Location; 
namespace TestBot.Controllers
{
    public class SqlLogin
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        //Constructor
        public SqlLogin()
        {
            Initialize();
        }

        //Initialize values
        private void Initialize()
        {
            //bluemix credential
            
            server = "us-cdbr-iron-east-04.cleardb.net";
            database = "ad_d50e86b971b02f2";
            uid = "bf87fb0cec2402";
            password = "60fcb38d";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            

            //localhost credential
            /*
            server = "localhost";
            database = "shoppingmall  ";
            uid = "root";
            password = "1234";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            */
            connection = new MySqlConnection(connectionString);


        }

        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        Debug.WriteLine("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        Debug.WriteLine("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        //Close connection
        private bool CloseConnection()
        {

            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        //Select statement
        public String Select(string inp, GeoCoordinate userLocation)
        {
            string query = inp;

            //Create a list to store the result

            //Open connection
            if (this.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();


                //Read the data and store them in the list
                String result = "";
                double lat, lon, res;
                string val;
                bool oneTime;
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        oneTime = true;
                        for (int i = 0; i < dataReader.FieldCount - 2; ++i)
                        {
                            lat = dataReader.GetDouble("latitude");
                            lon = dataReader.GetDouble("longitude");

                            
                            if (oneTime)
                            {
                                if ((res = userLocation.GetDistanceTo(new GeoCoordinate(lat, lon)) / 1000.0) > 20)
                                {
                                    Debug.WriteLine(res);
                                    break;
                                }
                            }
                            
                            oneTime = false;
                            val = dataReader.GetValue(i).ToString();
                            
                            result += val;
                            result += " - ";

                        }
                        result += "\n";
                    }
                    result += "\n";
                }
                //close Data Reader
                dataReader.Close();

                //close Connection
                this.CloseConnection();

                //return list to be displayed
                return result;
            }
            else
            {
                return null;
            }
        }

        //Count statement
        public int Count(string inp)
        {
            string query = inp;
            int Count = -1;

            //Open Connection
            if (this.OpenConnection() == true)
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //ExecuteScalar will return one value
                Count = int.Parse(cmd.ExecuteScalar() + "");

                //close Connection
                this.CloseConnection();

                return Count;
            }
            else
            {
                return Count;
            }
        }
    }
}