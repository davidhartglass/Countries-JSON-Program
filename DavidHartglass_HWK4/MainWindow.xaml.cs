using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using HWClassLibrary;
using System.Runtime.Serialization.Json;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace DavidHartglass_HWK4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Country> countryList = new List<Country>();
        string connectionString;


        public MainWindow()
        {
            InitializeComponent();

            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(Environment.CurrentDirectory);
            configurationBuilder.AddJsonFile("config.json");
            IConfiguration config = configurationBuilder.Build();

            connectionString = config["Data:DefaultConnection:ConnectionString"];
                        
        }      
      
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //When clicked, open the file dialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            //Set the title, and filter to only allow json files
            openFileDialog.Title = "Open Countries From JSON";
            openFileDialog.Filter = "JSON files (*.json)|*.json";

            //Open in the current directory
            openFileDialog.InitialDirectory = Environment.CurrentDirectory;
            openFileDialog.ShowDialog();

            //Countries_Filename_textBox.Text = openFileDialog.FileName;
            string fileName = openFileDialog.FileName;

            //Set the filename to null or empty if no file was selected
            if (fileName == null || fileName == "")
            {
                Console.WriteLine("Canceled");
            }
            else
            {
                //Otherwise open the file, and reaad the file
                FileStream JSONreader = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                StreamReader JSONstreamReader = new StreamReader(JSONreader, Encoding.UTF8);
                string jsonString = JSONstreamReader.ReadToEnd();

                byte[] jsonByteArray = Encoding.UTF8.GetBytes(jsonString);
                MemoryStream stream = new MemoryStream(jsonByteArray);

                DataContractJsonSerializer JSONinputSerializer = new DataContractJsonSerializer(typeof(List<Country>));

                countryList = (List<Country>)JSONinputSerializer.ReadObject(stream);                                          
                stream.Close();
                               

                //***************************************************************************************
                //              DELETE DATABASE
                //***************************************************************************************

                var configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.SetBasePath(Environment.CurrentDirectory);
                configurationBuilder.AddJsonFile("config.json");
                IConfiguration config = configurationBuilder.Build();

                connectionString = config["Data:DefaultConnection:ConnectionString"];

                //Create a connection and open the connection
                SqlConnection sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();

                string sql = "DELETE FROM Countries";
                SqlCommand command = new SqlCommand(sql, sqlConn);

                // Retrieve the data from the database
                SqlDataReader reader = command.ExecuteReader();




                //***************************************************************************************
                //              INSERT to Database
                //***************************************************************************************
                sqlConn = new SqlConnection(connectionString);
                sqlConn.Open();

                foreach (var item in countryList)
                {
                    sql = string.Format(
                       "INSERT INTO Countries" +
                       "(Name, Capital, Region, Subregion, Population) Values" +
                       "(@param1, @param2, @param3, @param4, @param5)");

                    command = new SqlCommand(sql, sqlConn);

                    command.Parameters.Add(new SqlParameter("@param1", item.myName));
                    command.Parameters.Add(new SqlParameter("@param2", item.myCapital));
                    command.Parameters.Add(new SqlParameter("@param3", item.myRegion));
                    command.Parameters.Add(new SqlParameter("@param4", item.mySubRegion));
                    command.Parameters.Add(new SqlParameter("@param5", item.myPopulation));

                    int rowsAffected = command.ExecuteNonQuery();
                }
                //Close the connection
                sqlConn.Close();
            }

        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {           
            MessageBox.Show("Country GUI\nVersion 2\nWritten by David Hartglass");
        }

        private void countries_listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.SetBasePath(Environment.CurrentDirectory);
            configurationBuilder.AddJsonFile("config.json");
            IConfiguration config = configurationBuilder.Build();

            string connectionString = config["Data:DefaultConnection:ConnectionString"];

            //Create a connection and open the connection
            SqlConnection sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();

            // Retrieve the data from the database
            string sql = "SELECT * FROM Countries WHERE Name = @param1";

            //  SQL Command uses the event args item which is the country name,
            //  to only select all of the columns that are related to the country name
            SqlCommand command = new SqlCommand(sql, sqlConn);
            command.Parameters.Add(new SqlParameter("@param1", e.AddedItems[0]));

            // Retrieve the data from the database
            SqlDataReader reader = command.ExecuteReader();

            //  Field count should only return 1
            int fieldCount = reader.FieldCount;

            //  While still reading...
            while (reader.Read())
            {
                for (int i = 0; i < fieldCount; i++)
                {
                    // Set the text fields 
                    name_textBox.Text = Convert.ToString(reader["Name"]);
                    capital_textBox.Text = Convert.ToString(reader["Capital"]);
                    region_textBox.Text = Convert.ToString(reader["Region"]);
                    subregion_textBox.Text = Convert.ToString(reader["Subregion"]);
                    population_textBox.Text = Convert.ToString(reader["Population"]);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Create a connection and open the connection
            SqlConnection sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();

            string sql = "SELECT Name FROM Countries";
            SqlCommand command = new SqlCommand(sql, sqlConn);

            // Retrieve the data from the database
            SqlDataReader reader = command.ExecuteReader();

            int fieldCount = reader.FieldCount;
            while (reader.Read())
            {
                for (int i = 0; i < fieldCount; i++)
                {
                    //Add all the country names as Items of the ListBox
                    countries_listBox.Items.Add(reader["Name"]);
                }
            }
        }
    }
}