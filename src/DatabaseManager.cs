using System;
using Microsoft.Data.Sqlite;
using System.IO;
using Windows.Storage;
using Windows.Foundation.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace OpheliasOasis.src
{
    public class DatabaseManager
    {
        // This should only ever be 'created' during the constuctor
        readonly SqliteConnection Connection;
        
        
        // Database 
        readonly ReservationMap   ReservationLookup;
        readonly PricePerDay      PriceLookup;
        readonly CustomerIDMap    CustomerLookup;
        //readonly
        private bool _run_event_handler;

        const string SQLTIME = "yyyy-MM-dd HH:mm:ss";

        ~DatabaseManager()
        {
            Connection.Close();
        }

        public DatabaseManager()
        {
            PriceLookup       = new PricePerDay();
            ReservationLookup = new ReservationMap();
            CustomerLookup    = new CustomerIDMap();

            // This needs to be false when refreshing the list from the DB
            _run_event_handler = false;

            // Add all the call back functions
            PriceLookup.MapChanged += PriceLookup_MapChanged;
            ReservationLookup.MapChanged += Reservations_MapChanged;
            CustomerLookup.MapChanged += CustomerLookup_MapChanged;

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
                System.Diagnostics.Debug.WriteLine(e.Message);
                return;
            }

            // If the database is empty then it needs to be constructed
            if (new FileInfo(dbFile.Path).Length == 0)
            {
                // Customer table
                var cmd =
                    @"CREATE TABLE ""Customers"" (
                      ""ID""    INTEGER NOT NULL UNIQUE,
                      ""EMAIL"" TEXT,
	                  ""FIRST_NAME""    TEXT NOT NULL,
	                  ""LAST_NAME"" TEXT NOT NULL,
	                  ""PHONE"" TEXT NOT NULL,
	                  ""CREDIT_CARD_NUMBER"" TEXT,
	                  ""CREDIT_CARD_DATE"" TEXT,
	                  ""CREDIT_CARD_NAME"" TEXT,
	                  PRIMARY KEY(""ID"" AUTOINCREMENT),
	                  UNIQUE(""ID""))";
                var command = new SqliteCommand(cmd, Connection);
                command.ExecuteNonQuery();

                // Days and Rates table
                cmd =
                    @"CREATE TABLE ""Dates"" (
                      ""DAY""   TEXT NOT NULL UNIQUE,
                      ""RATE""  REAL NOT NULL,
	                  PRIMARY KEY(""Day""))";
                command = new SqliteCommand(cmd, Connection);
                command.ExecuteNonQuery();
                
                // Reservation table
                cmd =
                    @"CREATE TABLE ""Reservations"" (
                      ""ID""    INTEGER NOT NULL UNIQUE,
                      ""TYPE""  TEXT NOT NULL,
	                  ""CUSTOMER_ID"" INTEGER NOT NULL,
	                  ""STATUS""     TEXT,
	                  ""ROOM_ID""    TEXT,
                      ""PRICES""     TEXT NOT NULL,
	                  ""START_DATE"" TEXT NOT NULL,
	                  ""END_DATE""   TEXT NOT NULL,
	                  PRIMARY KEY(""ID"" AUTOINCREMENT))";
                command = new SqliteCommand(cmd, Connection);
                command.ExecuteNonQuery();
            }
            GetReservations();
            GetPricePerDay();
            GetCustomers();
            _run_event_handler = true;
        }

        private void PriceLookup_MapChanged(IObservableMap<DateTime, double> sender, IMapChangedEventArgs<DateTime> @event)
        {
            if (@event == null || !_run_event_handler) return;

            switch (@event.CollectionChange)
            {
                case CollectionChange.Reset:
                    _run_event_handler = false;
                    GetPricePerDay();
                    _run_event_handler = true;
                    break;
                case CollectionChange.ItemInserted:
                    // Send back to db
                    InsertIntoDB(sender[@event.Key], false);
                    break;
                case CollectionChange.ItemRemoved:
                    throw new NotImplementedException();
                case CollectionChange.ItemChanged:
                    InsertIntoDB((@event.Key, sender[@event.Key]), true);
                    break;
                // No reason? Do nothing
                default:
                    break;
            }
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
                    InsertIntoDB(sender[@event.Key], false);
                    break;
                case CollectionChange.ItemRemoved:
                    throw new NotImplementedException();
                case CollectionChange.ItemChanged:
                    InsertIntoDB(sender[@event.Key], true);
                    break;
                // No reason? Do nothing
                default:
                    break;
            }
        }

        private void CustomerLookup_MapChanged(IObservableMap<int, Customer> sender, IMapChangedEventArgs<int> @event)
        {
            if (@event == null || !_run_event_handler) return;

            switch (@event.CollectionChange)
            {
                case CollectionChange.Reset:
                    _run_event_handler = false;
                    GetCustomers();
                    _run_event_handler = true;
                    break;
                case CollectionChange.ItemInserted:
                    // Send back to db
                    InsertIntoDB(sender[@event.Key], false);
                    break;
                case CollectionChange.ItemRemoved:
                    throw new NotImplementedException();
                case CollectionChange.ItemChanged:
                    InsertIntoDB(sender[@event.Key], true);
                    break;
                // No reason? Do nothing
                default:
                    break;
            }
        }

        // Handles changes back into the DB. Update needs to be set if the value is only being modified and not created
        // for the first time.
        private void InsertIntoDB(object item, bool update) 
        {
            if (item == null) return;
            try
            {
                string cmd = "";
                SqliteCommand command = new SqliteCommand();
                command.Connection = Connection;
                
                cmd += "INSERT OR REPLACE INTO ";
                

                if (item is Reservation)
                {
                    Reservation reservation = (Reservation)item;
                    
                    cmd += string.Format("Reservations(ID, TYPE, CUSTOMER_ID, STATUS, " +
                                        "ROOM_ID, PRICES, START_DATE, END_DATE) VALUES(@id, @type, @CustomerID, @Status, @ROOM_ID, " +
                                        "@Prices, @StartDate, @EndDate)");
                   
                    command.CommandText = cmd;
                    command.Parameters.AddWithValue("@id", reservation.ReservationID);
                    
                    switch (reservation.Type)
                    {
                        case ReservationType.SixtyDays:
                            command.Parameters.AddWithValue("@type", "SixtyDays");
                            break;
                        case ReservationType.Prepaid:
                            command.Parameters.AddWithValue("@type", "Prepaid");
                            break;
                        case ReservationType.Conventional:
                            command.Parameters.AddWithValue("@type", "Conventional");
                            break;
                        case ReservationType.Incentive:
                            command.Parameters.AddWithValue("@type", "Incentive");
                            break;
                        default:
                            break;
                    }

                    command.Parameters.AddWithValue("@CustomerID", reservation.CustomerID);
                    switch (reservation.Status)
                    {
                        case PaymentStatus.NotPaid:
                            command.Parameters.AddWithValue("@Status", "NotPaid");
                            break;
                        case PaymentStatus.Paid:
                            command.Parameters.AddWithValue("@Status", "Paid");
                            break;
                        case PaymentStatus.Cancled:
                            command.Parameters.AddWithValue("@Status", "Cancled");
                            break;
                        default:
                            break;
                    }
                    command.Parameters.AddWithValue("@ROOM_ID", reservation.RoomID);
                    command.Parameters.AddWithValue("@Prices", reservation.Prices.ToString());
                    command.Parameters.AddWithValue("@StartDate", reservation.StartDate.ToString());
                    command.Parameters.AddWithValue("@EndDate", reservation.EndDate.ToString());
                    
                }
                else if (item is Customer)
                {
                    Customer customer = (Customer)item;
                    cmd += string.Format("Customers(ID, EMAIL, FIRST_NAME, LAST_NAME, PHONE, CREDIT_CARD_NUMBER, CREDIT_CARD_DATE" +
                                        "CREDIT_CARD_NAME) VALUES(@id, @email, @FirstName, @LastName, @phone, @ccnumber, @ccd, @ccname)");
            
                    command.CommandText = cmd;
                    command.Parameters.AddWithValue("@id",customer.Id);
                    command.Parameters.AddWithValue("@email",customer.Email);
                    command.Parameters.AddWithValue("@FirstName",customer.FirstName);
                    command.Parameters.AddWithValue("@LastName",customer.LastName);
                    command.Parameters.AddWithValue("@phone",customer.Phone);
                    command.Parameters.AddWithValue("@ccnumber",customer.CardOnFile.CardNumbers);
                    command.Parameters.AddWithValue("@ccd",customer.CardOnFile.ExpirationDate);
                    command.Parameters.AddWithValue("@ccname",customer.CardOnFile.Name);
                }
                else if (item is ValueTuple<DateTime, Double>)
                {
                    ValueTuple<DateTime, Double> DatePrice = (ValueTuple<DateTime, Double>)item;
                    
                    cmd += string.Format("Dates(DAY, RATE) VALUES(@day, @rate)");

                    command.CommandText = cmd;
                    command.Parameters.AddWithValue("@day", DatePrice.Item1.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@rate", DatePrice.Item2);
                }
                // TODO Could do some validiation check here
                command.ExecuteNonQuery();
            }
            catch (SqliteException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        private List<double> parseString(string input)
        {
            List<double> result = new List<double>();
            input = input.Trim();
            string[] splitInput = input.Split(' ');

            foreach (var item in splitInput)
            {
                result.Add(double.Parse(item));
            }

            return result;
        }

        private PaymentStatus prasePayment(string input)
        {
            PaymentStatus status = new PaymentStatus();
            if (input == "NotPaid")
            {
                status = PaymentStatus.NotPaid;
            }
            else if (input == "Paid")
            {
                status = PaymentStatus.Paid;
            }
            else if (input == "Cancled")
            {
                status = PaymentStatus.Cancled;
            }
            return status;
        }

        private ReservationType praseType(string input)
        {
            ReservationType type = new ReservationType();

            if (input == "Conventional") type = ReservationType.Conventional;
            if (input == "Prepaid")      type = ReservationType.Prepaid;
            if (input == "Incentive")    type = ReservationType.Incentive;
            if (input == "SixtyDays")    type = ReservationType.SixtyDays;


            return type;
        }

        public ReservationMap GetReservations()
        {
            _run_event_handler = false;
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
                        var res = new Reservation();
                        reservations.Add(new Reservation
                        {
                            ReservationID = reader.GetInt32(0),
                            Type          = praseType(reader.GetString(1)),
                            CustomerID    = reader.GetInt32(2),
                            Status        = prasePayment(reader.GetString(3)),
                            RoomID        = reader.GetInt32(4),
                            Prices        = parseString(reader.GetString(5)),
                            StartDate     = DateTime.Parse(reader.GetString(6)),
                            EndDate       = DateTime.Parse(reader.GetString(7)),
                        });
                    }
                }
            }
            catch (SqliteException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            // Only add new items. This really only should happen once during init
            foreach (var res in reservations)
            {
                if (!ReservationLookup.ContainsKey(res.ReservationID))
                { 
                    ReservationLookup.Add(res.ReservationID, res);
                }
            }
            _run_event_handler = true;
            return ReservationLookup;
        }

        public PricePerDay GetPricePerDay()
        {
            _run_event_handler = false;
            var currentDate = DateTime.Today;
            var cmd = string.Format("SELECT * FROM Dates", currentDate.ToString(SQLTIME));

            var priceList = new PricePerDay();
            var command = new SqliteCommand(cmd, Connection);

            try
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime date = DateTime.Parse(reader.GetString(0));
                        var price = (double)reader.GetValue(1);
                        priceList[date.Date] = price;
                    }
                }
            }
            catch (SqliteException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            // Only add new items. This really only should happen once during init
            foreach (var res in priceList)
            {
                if (!PriceLookup.ContainsKey(res.Key))
                {
                    PriceLookup.Add(res.Key, res.Value);
                }
            }
            _run_event_handler = true;
            return PriceLookup;
        }

        public CustomerIDMap GetCustomers()
        {
            _run_event_handler = false;
            var currentDate = DateTime.Today;
            var cmd = string.Format("SELECT * FROM Customers", currentDate.ToString(SQLTIME));

            var customerList = new List<Customer>();
            var command = new SqliteCommand(cmd, Connection);

            try
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        customerList.Add(new Customer
                        {
                            Id = reader.GetInt32(0),
                            Email = reader.GetString(1),
                            FirstName = reader.GetString(2),
                            LastName = reader.GetString(3),
                            Phone = reader.GetString(4),
                            CardOnFile = new CreditCard
                            {
                                CardNumbers = reader.GetString(5),
                                ExpirationDate = reader.GetString(6),
                                Name = reader.GetString(7),
                            }
                        });
                    }
                }
            }
            catch (SqliteException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            // Only add new items. This really only should happen once during init
            foreach (var res in customerList)
            {
                if (!CustomerLookup.ContainsKey(res.Id))
                {
                    CustomerLookup.Add(res.Id, res);
                }
            }
            _run_event_handler = true;
            return CustomerLookup;
        }

    }
}
