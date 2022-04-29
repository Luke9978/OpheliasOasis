using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation.Collections;

namespace OpheliasOasis
{
    public class MapCallBack<V> : IObservableMap<int, V>
    {
        private readonly Dictionary<int, V> _items;

        public event MapChangedEventHandler<int, V> MapChanged;

        private class MapChangeReason : IMapChangedEventArgs<int>
        {
            public CollectionChange CollectionChange { get; set; }

            public int Key { get; set; }
        }

        private void TriggerEvent(CollectionChange reason, int key)
        {
            var aChangeEvent = new MapChangeReason();
            aChangeEvent.Key = key;
            aChangeEvent.CollectionChange = reason;
            MapChanged.Invoke(this, aChangeEvent);
        }

        public MapCallBack()
        {
            _items = new Dictionary<int, V>();
        }

        // Param: Set key to 0
        public void Add(int key, V value)
        {
            key = 0;

            // Avoid key collision
            while (this.ContainsKey(key))
            {
                key++;
            }

            _items.Add(key, value);
            TriggerEvent(CollectionChange.ItemInserted, key);
        }


        public bool ContainsKey(int key)
        {
            return _items.ContainsKey(key);
        }

        public bool Remove(int key)
        {
            TriggerEvent(CollectionChange.ItemRemoved, key);
            return (_items.Remove(key));
        }

        public bool TryGetValue(int key, out V value)
        {
            return _items.TryGetValue(key, out value);
        }

        public V this[int key]
        {
            get => _items[key];
            set
            {
                _items[key] = value;
                TriggerEvent(CollectionChange.ItemChanged, key);
            }
        }

        public ICollection<int> Keys => _items.Keys.ToList();

        public ICollection<V> Values => _items.Values.ToList();

        public void Add(KeyValuePair<int, V> item)
        {
            TriggerEvent(CollectionChange.ItemInserted, item.Key);
            _items.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            TriggerEvent(CollectionChange.Reset, 0);
            _items.Clear();
        }

        public bool Contains(KeyValuePair<int, V> item)
        {
            if (!_items.ContainsKey(item.Key))
            {
                return false;
            }

            var res   = _items[item.Key];
            var _comp = item.Value;
            // Wut?
            if (res == null)// || res != _comp)
            {
                return false;
            }

            return true;
        }


        public bool Remove(KeyValuePair<int, V> item)
        {
            TriggerEvent(CollectionChange.ItemRemoved, item.Key);
            return (_items.Remove(item.Key));
        }

        public int Count => _items.Count();

        public bool IsReadOnly => throw new NotImplementedException();

        public IEnumerator<KeyValuePair<int, V>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(KeyValuePair<int, V>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
    }

    public class ReservationMap : MapCallBack<Reservation>
    {
        public void Add(Reservation value)
        {
            int key = 0;

            // Avoid key collision
            while (this.ContainsKey(key))
            {
                key++;
            }
            
            value.ReservationID = key;

            Add(key, value);
        }
    }

    public class Reservation : INotifyPropertyChanged
    {
        public int ReservationID { get; set; }
        public ReservationType Type { get; set; }
        public PaymentStatus Status { get; set; }
        
        public DateTime StartDate;

        public DateTime EndDate;

        public List<double> Prices;
        public int CustomerID { get; set; }
        public int RoomID { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class PricePerDay : IObservableMap<DateTime, double>
    {
        private readonly IDictionary<DateTime, double> _pricePerDay;
        public event MapChangedEventHandler<DateTime, double> MapChanged;
        private class MapChangeReason : IMapChangedEventArgs<DateTime>
        {
            public CollectionChange CollectionChange { get; set; }

            public DateTime Key { get; set; }
        }

        private void TriggerEvent(CollectionChange reason, DateTime key)
        {
            var aChangeEvent = new MapChangeReason();
            aChangeEvent.Key = key;
            aChangeEvent.CollectionChange = reason;
            MapChanged?.Invoke(this, aChangeEvent);
        }

        public PricePerDay()
        {
            _pricePerDay = new Dictionary<DateTime, double>();
        }

        public void Add(DateTime key, double value)
        {
            _pricePerDay[key.Date] = value;
            TriggerEvent(CollectionChange.ItemInserted, key);
        }

        public bool ContainsKey(DateTime key)
        {
            return (_pricePerDay.ContainsKey(key.Date));
        }

        public bool Remove(DateTime key)
        {
            TriggerEvent(CollectionChange.ItemRemoved, key.Date);
            return (_pricePerDay.Remove(key.Date));
        }

        public bool TryGetValue(DateTime key, out double value)
        {
            return _pricePerDay.TryGetValue(key.Date, out value);
        }

        public double this[DateTime key] { get => _pricePerDay[key.Date];
            set
            {
                _pricePerDay[key] = value;
                TriggerEvent(CollectionChange.ItemChanged, key.Date);
            } }

        public ICollection<DateTime> Keys => _pricePerDay.Keys.ToList();

        public ICollection<double> Values => _pricePerDay.Values.ToList();

        public void Add(KeyValuePair<DateTime, double> item)
        {
            _pricePerDay.Add(item);
            TriggerEvent(CollectionChange.ItemInserted, item.Key.Date);
        }

        public void Clear()
        {
            _pricePerDay.Clear();
            TriggerEvent(CollectionChange.Reset, DateTime.Now.Date);
        }

        public bool Contains(KeyValuePair<DateTime, double> item)
        {
            return (_pricePerDay.Contains(item));
        }

        public void CopyTo(KeyValuePair<DateTime, double>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<DateTime, double> item)
        {
            return _pricePerDay.Remove(item);
        }

        public int Count => _pricePerDay.Count();

        public bool IsReadOnly => false;

        public IEnumerator<KeyValuePair<DateTime, double>> GetEnumerator()
        {
            return _pricePerDay.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _pricePerDay.GetEnumerator();
        }
    }

    public class CustomerIDMap : MapCallBack<Customer>
    {
        public void Add(Customer value)
        {
            int key = 0;

            // Avoid key collision
            while (this.ContainsKey(key))
            {
                key++;
            }

            value.Id = key;

            Add(key, value);
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int ReservationID { get; set; }
        public CreditCard CardOnFile { get; set; }
    }

    public class CreditCard
    {
        public string Name { get; set; }
        public string ExpirationDate { get; set; }
        public string CardNumbers { get; set; }
    }

    public enum ReservationType
    {
        Conventional,
        Prepaid,
        Incentive,
        SixtyDays        
    }

    public enum PaymentStatus
    {
        NotPaid,
        Paid,
        Cancled
    }

    public enum reportType 
    {
        ExpectedOccupancy,
        ExpectedRoomIncome,
        Incentive,
        DailyArrivals,
        DailyOccupancy,
        AccommodationBill
    }

}