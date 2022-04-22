using System;
using Microsoft.Data.Sqlite;
using System.IO;
using Windows.Storage;
using Windows.Foundation.Collections;
using System.Collections.ObjectModel;

namespace OpheliasOasis.src
{
    public class DatabaseManager
    {
        // This should only ever be 'created' during the constuctor
        readonly SqliteConnection Connection;
        
        
        // Database 
        readonly ReservationMap   Reservations;
        readonly PricePerDay      PriceLookup;
        //readonly
        private bool _run_event_handler;

        const string SQLTIME = "yyyy-MM-dd HH:mm:ss";

        ~DatabaseManager()
        {
            Connection.Close();
        }

        public DatabaseManager()
        {
            PriceLookup = new PricePerDay();
            Reservations = new ReservationMap();
            // This needs to be false when refreshing the list from the DB
            _run_event_handler = false;
            Reservations.MapChanged += Reservations_MapChanged;

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


            Connection = new SqliteConnection(_sql.ConnectionString);
            try
            {
                Connection.Open();
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
                var command = new SqliteCommand(cmd, Connection);
                command.ExecuteNonQuery();

                cmd =
                    @"CREATE TABLE ""Dates"" (
                      ""Day""   TEXT NOT NULL UNIQUE,
                      ""Rate""  REAL NOT NULL,
	                  PRIMARY KEY(""Day""))";
                command = new SqliteCommand(cmd, Connection);
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
                command = new SqliteCommand(cmd, Connection);
                command.ExecuteNonQuery();
            }
            GetReservations();
            _run_event_handler = true;
        }

        // Send the change to the list back to the database
        private void Reservations_MapChanged(IObservableMap<int, Reservation> sender, IMapChangedEventArgs<int> @event)
        {
            if (@event == null || !_run_event_handler) return;

            switch (@event.CollectionChange)
            {
                case CollectionChange.Reset:
                    _run_event_handler = false;
                    GetReservations();
                    _run_event_handler = true;
                    break;
                case CollectionChange.ItemInserted:
                    // Send back to db

                    break;
                case CollectionChange.ItemRemoved:

                    break;
                case CollectionChange.ItemChanged:

                    break;
                // No reason? Do nothing
                default:
                    break;
            }
        }

        // Can take a CustomerID, Reservation or Date Range
        private void InsertIntoDB(object item) 
        {
            
        }

        public IObservableMap<int, Reservation> GetReservations()
        {
            var currentDate = DateTime.Today;
            var cmd         = string.Format("SELECT * FROM Reservations", currentDate.ToString(SQLTIME));
            
            var reservations = new ObservableCollection<Reservation>();
            var command      = new SqliteCommand(cmd, Connection);

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

            // Only add new items. This really only should happen once during init
            foreach (var res in reservations)
            {
                if (!Reservations.ContainsKey(res.id))
                { 
                    Reservations.Add(res.id, res);
                }
            }

            return Reservations;
        }

        public IObservableMap<DateTime, double> GetPricePerDay()
        {
            var currentDate = DateTime.Today;
            var cmd = string.Format("SELECT * FROM Reservations", currentDate.ToString(SQLTIME));

            var priceList = new ObservableCollection<PricePerDay>();
            var command = new SqliteCommand(cmd, Connection);

            // TODO I expect this to error out
            try
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //TODO
                        //DateTime date = DateTime.Parse(reader.GetString(1));

                    }
                }
            }
            catch (SqliteException e)
            {
                System.Console.Error.WriteLine(e.ToString());
            }

            return PriceLookup;
        }

    }
}
