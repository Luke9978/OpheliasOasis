using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.IO;
using Windows.Storage;
using System.Collections.ObjectModel;

namespace OpheliasOasis.src
{
    public class DatabaseManager
    {
        SqliteConnection connection;
        //TODO make this a map
        ObservableCollection<Reservation> Reservations;


        const string SQLTIME = "yyyy-MM-dd HH:mm:ss";

        ~DatabaseManager()
        {
            connection.Close();
        }

        public DatabaseManager()
        {
            // File needs to be where the application is allowed to be in because
            // the application is "sandboxed" to only access that file 
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            // No idea why I can't use a regular non asyc funtion here 
            var fileTask = storageFolder.CreateFileAsync("Database.db", CreationCollisionOption.OpenIfExists);
            fileTask.AsTask().Wait();
            var dbFile = fileTask.GetResults();

            SqliteConnectionStringBuilder _sql = new SqliteConnectionStringBuilder
            {
                DataSource = dbFile.Path,
                Mode = SqliteOpenMode.ReadWriteCreate
            };


            connection = new SqliteConnection(_sql.ConnectionString);
            try
            {
                connection.Open();
            }
            catch (SqliteException e)
            {
                Console.WriteLine("Program was unable to open database at: " + dbFile.Path);
                Console.WriteLine(e.ToString());
                return;
            }

            // If the database is empty then it needs to be constructed
            if (new FileInfo(dbFile.Path).Length == 0)
            {
                var cmd =
                    @"CREATE TABLE ""Customers"" (
                      ""ID""    INTEGER NOT NULL UNIQUE,
                      ""EMAIL"" TEXT,
	                  ""FIRST_NAME""    TEXT NOT NULL,
	                  ""LAST_NAME"" TEXT NOT NULL,
	                  ""PHONE"" TEXT NOT NULL,
	                  ""CREDIT_CARD""   TEXT,
	                  PRIMARY KEY(""ID"" AUTOINCREMENT),
	                  UNIQUE(""ID""))";
                var command = new SqliteCommand(cmd, connection);
                command.ExecuteNonQuery();

                cmd =
                    @"CREATE TABLE ""Dates"" (
                      ""Day""   TEXT NOT NULL UNIQUE,
                      ""Rate""  REAL NOT NULL,
	                  PRIMARY KEY(""Day""))";
                command = new SqliteCommand(cmd, connection);
                command.ExecuteNonQuery();

                cmd =
                    @"CREATE TABLE ""Reservations"" (
                      ""ID""    INTEGER NOT NULL UNIQUE,
                      ""TYPE""  TEXT,
	                  ""CUSTOMER_ID""   INTEGER NOT NULL,
	                  ""STATUS""    TEXT,
	                  ""PAID""  INTEGER NOT NULL,
	                  ""ROOM""  TEXT,
	                  ""START_DATE""    TEXT NOT NULL,
	                  ""END_DATE""  TEXT NOT NULL,
	                  PRIMARY KEY(""ID"" AUTOINCREMENT))";
                command = new SqliteCommand(cmd, connection);
                command.ExecuteNonQuery();

               

            }
        }
        public ObservableCollection<Reservation> GetReservations()
        {
            var currentDate = DateTime.Today;
            var cmd = String.Format("SELECT * FROM Reservations", currentDate.ToString(SQLTIME));
            
            var reservations = new ObservableCollection<Reservation>();
            var command = new SqliteCommand(cmd, connection);
            System.Console.WriteLine("SQL result ");
            try
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reservations.Add(new Reservation
                        {
                            id         = reader.GetInt32(0),
                            type       = reader.GetString(1),
                            CustomerId = reader.GetInt32(2),
                            status     = reader.GetString(3),
                            paid       = reader.GetInt32(4),
                            RoomId     = reader.GetInt32(5),
                            startDate  = DateTime.Parse(reader.GetString(6)),
                            endDate    = DateTime.Parse(reader.GetString(7)),

                        });
                    }
                }
            }
            catch (SqliteException e)
            {
                System.Console.Error.WriteLine(e.ToString());
            }

            Reservations = reservations;

            return Reservations;
        }
            
    }
}
