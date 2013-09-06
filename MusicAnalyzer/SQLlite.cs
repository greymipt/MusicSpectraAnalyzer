using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.IO;
using System.Data;
using System.Data.Common;

namespace SongMarker
{
    class CSQLiteConnect
    {
        private SQLiteConnectionStringBuilder cb;
        private string dbFileName = "songs.db";

        public CSQLiteConnect()
        {
            Initialize();
        }

        private void Initialize()
        {
            cb = new SQLiteConnectionStringBuilder();
            cb.DataSource = dbFileName;
            if (!File.Exists(dbFileName))
            {
                SQLiteConnection.CreateFile(this.dbFileName);
                CreateSchema("songs");
            }
            
         }
        private void CreateSchema(string dbname)
        {
            using (SQLiteConnection connection = new SQLiteConnection(cb.ConnectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = "CREATE TABLE ["+dbname+"] ("+
                    "[song_id] integer PRIMARY KEY AUTOINCREMENT,"+
                    "[artist] char(100) ,"+
                    "[title] char(100) ,"+
                    "[path] varchar(100) ," +
                    "[time] varchar(10) ," +
                    "[data] BLOB"+ 
                    ");";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Drop schema and creates the same one
        /// </summary>
        public bool ClearDB()
        {
            if (File.Exists(dbFileName))
            {
                GC.Collect(2, GCCollectionMode.Forced);
                File.Delete(this.dbFileName);
                SQLiteConnection.CreateFile(this.dbFileName);
                CreateSchema("songs");
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Inserts processed music data into DataBase 
        /// </summary>
        /// <param name="tds">Class containing music data</param>
        public void Insert(TransferDataSong tds)
        {
            string query = "INSERT INTO [songs] ([song_id], [title], [artist], [path], [time], [data]) VALUES (null, @title, @artist, @path, @time, @data);";
            using (SQLiteConnection connection = new SQLiteConnection(cb.ConnectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@title",tds.title);
                    command.Parameters.AddWithValue("@artist", tds.artist);
                    command.Parameters.AddWithValue("@path", tds.path);
                    command.Parameters.AddWithValue("@time", tds.duration);
                    command.Parameters.AddWithValue("@data", tds.data);
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Gets list of all records (except data field) from DataBase 
        /// </summary>
        public List<TransferDataSong> GetAllRecords()
        {
            List<TransferDataSong> list = new List<TransferDataSong>();
            string query = "SELECT song_id, artist, title, time, path, data FROM songs";
            using (SQLiteConnection connection = new SQLiteConnection(cb.ConnectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;
                    using (SQLiteDataReader Reader = command.ExecuteReader())
                    {
                        while (Reader.Read())
                        {
                            TransferDataSong tds = new TransferDataSong();
                            tds.id = Convert.ToInt32(Reader["song_id"]);
                            tds.artist = Reader["artist"].ToString();
                            tds.title= Reader["title"].ToString();
                            tds.duration = Reader["time"].ToString();
                            tds.path = Reader["path"].ToString();
                            int DataRowNumber = Reader.GetOrdinal("data");
                            byte[] buffer = new byte[Reader.GetBytes(DataRowNumber, 0, null, 0, int.MaxValue)];
                            Reader.GetBytes(DataRowNumber, 0, buffer, 0, buffer.Length);
                            tds.data = buffer;
                            list.Add(tds);
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Gets DataSet containing all fields except data from DataBase 
        /// </summary>
        public DataSet GetDataSet()
        {
            DataSet ds=new DataSet();
            string query = "SELECT song_id, time, artist, title, path FROM songs ORDER BY artist, title";
            using (SQLiteConnection connection = new SQLiteConnection(cb.ConnectionString))
            {
                connection.Open();
                SQLiteDataAdapter da = new SQLiteDataAdapter(query, connection);
                ds.Reset();
                da.Fill(ds); 
            }
            return ds;
        }

        /// <summary>
        /// Updates specified DataTable  
        /// </summary>
        /// <param name="table">DataTable to update</param>
        public void LoadDataSet(DataTable table)
        {
            string query = "SELECT song_id, artist, title, time, path FROM songs";
            using (SQLiteConnection connection = new SQLiteConnection(cb.ConnectionString))
            {
                connection.Open();
                SQLiteDataAdapter da = new SQLiteDataAdapter(query, connection);
                SQLiteCommandBuilder cb2 = new SQLiteCommandBuilder(da);
                da.DeleteCommand = cb2.GetDeleteCommand();
                da.UpdateCommand = cb2.GetUpdateCommand();
                da.InsertCommand = cb2.GetInsertCommand(); 
                da.Update(table);
            }
        }

        /// <summary>
        /// Gets song path via artist and title  
        /// </summary>
        /// <param name="artist">song artist</param>
        /// <param name="title">song title</param>
        public string GetPath(string artist, string title)
        {
            string resPath=null;
            string query = "SELECT path FROM songs WHERE artist=@artist AND title=@title";
            using (SQLiteConnection connection = new SQLiteConnection(cb.ConnectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@title", title);
                    command.Parameters.AddWithValue("@artist", artist);
                    using (SQLiteDataReader Reader = command.ExecuteReader())
                    {
                        while (Reader.Read())
                        {
                            resPath = Reader["path"].ToString();
                        }
                    }
                }
            }
            return resPath;
        }

        /// <summary>
        /// Gets song data via song_id  
        /// </summary>
        /// <param name="song_id">song_id in DataBase</param>
        public TransferDataFFT GetSongData(int song_id)
        {
            TransferDataSong tds = new TransferDataSong();
            TransferDataFFT tdf = new TransferDataFFT();
            string query = "SELECT data FROM songs WHERE song_id=" + song_id;
            using (SQLiteConnection connection = new SQLiteConnection(cb.ConnectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;
                    using (SQLiteDataReader Reader = command.ExecuteReader())
                    {
                        while (Reader.Read())
                        {
                            int DataRowNumber = Reader.GetOrdinal("data");
                            byte[] buffer = new byte[Reader.GetBytes(DataRowNumber, 0, null, 0, int.MaxValue)];
                            Reader.GetBytes(DataRowNumber, 0, buffer, 0, buffer.Length);
                            tds.data = buffer;
                            tdf = tds.DeserializeData();
                        }
                    }
                }
            }
            return tdf;
        }

        /// <summary>
        /// Removes duplicates from DataBase  
        /// </summary>
        public void RemoveDuplicates()
        {
            string query = "DELETE FROM songs WHERE song_id NOT IN (SELECT MIN(song_id) FROM songs GROUP BY title, artist);";
            using (SQLiteConnection connection = new SQLiteConnection(cb.ConnectionString))
            {
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = query;
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
