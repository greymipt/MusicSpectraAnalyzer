using MySql.Data.MySqlClient;
using System.Windows;
using System.Data;
using NAudio;
using System.IO;
using System.Collections.Generic;
using System.Transactions;
using System.Runtime.Serialization.Formatters.Binary;

namespace SongMarker
{
    class DBConnect
    {
        private MySqlConnectionStringBuilder connectionString;

        //Constructor
        public DBConnect()
        {
            CheckDBExistence("songs");
        }

        //Initialize values
        private void Initialize(string DBname)
        {
            connectionString = new MySqlConnectionStringBuilder();
            connectionString.UserID = "root";
            connectionString.Password = "root";
            connectionString.Server = "localhost";
            connectionString.Database = DBname;
            connectionString.Pooling = true;
            connectionString.MaximumPoolSize = 100;
        }

        //Insert statement
        public void Insert(TransferDataSong tds)
        {
            //using (TransactionScope scope = new TransactionScope())
            //{
                string query = "INSERT INTO song_info(song_id, title, artist, data) VALUES (null, @title, @artist, @data);";
                List<MySqlParameter> parms = new List<MySqlParameter>();
                parms.Add(new MySqlParameter("title", tds.title));
                parms.Add(new MySqlParameter("artist", tds.artist));
                parms.Add(new MySqlParameter("data", tds.data));
                MySqlHelper.ExecuteNonQuery(connectionString.GetConnectionString(true), query, parms.ToArray());

            //    scope.Complete();
            //}
        }

        //Get all data from DB
        public  List<TransferDataSong> GetAllRecords()
        {
            List<TransferDataSong> list = new List<TransferDataSong>();
            string query;
            query = "SELECT artist, title, data FROM song_info";
            MySqlDataReader Reader = MySqlHelper.ExecuteReader(connectionString.GetConnectionString(true), query);
            while (Reader.Read())
            {
                TransferDataSong tds = new TransferDataSong();
                tds.artist = Reader.GetString("artist");
                tds.title = Reader.GetString("title");

                MemoryStream ms = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(ms);
                int DataRowNumber = Reader.GetOrdinal("data");
                long startIndex = 0;
                int bufferSize = 1000;
                byte[] outByte = new byte[bufferSize];
                long retval = Reader.GetBytes(DataRowNumber, startIndex, outByte, 0, bufferSize);

                while (retval == bufferSize)
                {
                    writer.Write(outByte);
                    writer.Flush();
                    startIndex += bufferSize;
                    retval = Reader.GetBytes(DataRowNumber, startIndex, outByte, 0, bufferSize);
                }

                writer.Write(outByte, 0, (int)retval);
                writer.Flush();
                tds.data = ms.ToArray();
                list.Add(tds);

                ms.Close();
                writer.Close();
            }
            
            return list;
        }

        //Get DataSet From DB (w/o data field)
        public DataSet GetDataSet()
        {
            string query = "SELECT song_id, artist, title FROM song_info";
            return MySqlHelper.ExecuteDataset(connectionString.GetConnectionString(true),query);
        }

        // Checks if DB exists and if not creates it
        private void CheckDBExistence(string DBname)
        {
                Initialize("test");
                string query = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA";
                MySqlDataReader Reader = MySqlHelper.ExecuteReader(connectionString.GetConnectionString(true),query);
                string row = "";
                while (Reader.Read())
                {
                    for (int i = 0; i < Reader.FieldCount; i++)
                        row += Reader.GetValue(i).ToString() + ", ";
                }
                Reader.Close();
                if (!row.Contains(DBname))
                {
                    query = "CREATE DATABASE " + DBname + " CHARACTER SET utf8";
                    MySqlHelper.ExecuteNonQuery(connectionString.GetConnectionString(true), query);
                    Initialize(DBname);
                    CreateDBSchema(DBname);
                }
                else
                {
                    Initialize(DBname);
                }
        }

        //Creates DB schema
        private void CreateDBSchema(string DBname)
        {
                //Create Tables
                string query="CREATE TABLE song_info"+
                                "(song_id INT AUTO_INCREMENT,"+
                                "title VARCHAR(100),"+
                                "artist VARCHAR(100),"+
                                "data MEDIUMBLOB," +
                                "CONSTRAINT pk_person PRIMARY KEY (song_id))ENGINE=INNODB";

                MySqlHelper.ExecuteNonQuery(connectionString.GetConnectionString(true), query);
        }
    }
}
