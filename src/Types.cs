using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation.Collections;

namespace OpheliasOasis
{
    public class ReservationMap : IObservableMap<int, Reservation>
    {
        private readonly Dictionary<int, Reservation> _items;

        public event MapChangedEventHandler<int, Reservation> MapChanged;

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

        public ReservationMap()
        {
            _items = new Dictionary<int, Reservation>();
        }

        // Param: Set key to 0
        public void Add(int key, Reservation value)
        {
            key = _items.Max(t => t.Key) + 1;

            // Avoid key collision
            while (this.ContainsKey(key))
            {
                key++;
            }

            _items.Add(key, value);
            TriggerEvent(CollectionChange.ItemInserted, key);
        }

        public void Add(Reservation value)
        {
            int key = _items.Max(t => t.Key) + 1;

            // Avoid key collision
            while(this.ContainsKey(key))
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

        public bool TryGetValue(int key, out Reservation value)
        {
            return _items.TryGetValue(key, out value);
        }

        public Reservation this[int key] { get => _items[key];
            set
            {
                _items[key] = value;
                TriggerEvent(CollectionChange.ItemChanged, key);
            } }

        public ICollection<int> Keys => _items.Keys.ToList();

        public ICollection<Reservation> Values => _items.Values.ToList();

        public void Add(KeyValuePair<int, Reservation> item)
        {
            TriggerEvent(CollectionChange.ItemInserted, item.Key);
            _items.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            TriggerEvent(CollectionChange.Reset, 0);
            _items.Clear();
        }

        public bool Contains(KeyValuePair<int, Reservation> item)
        {
            if (!_items.ContainsKey((int)item.Key))
            {
                return false;
            }

            var res = _items[item.Key];
            if (res == null || res != item.Value)
            {
                return false;
            }

            return true;
        }

        public void CopyTo(KeyValuePair<int, Reservation>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<int, Reservation> item)
        {
            TriggerEvent(CollectionChange.ItemRemoved, item.Key);
            return (_items.Remove(item.Key));
        }

        public int Count => _items.Count();

        public bool IsReadOnly => false;

        public IEnumerator<KeyValuePair<int, Reservation>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Reservation : INotifyPropertyChanged
    {
        public int id { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public int paid { get; set; }
        public DateTime startDate;
        public DateTime endDate;
        public IDictionary<DateTime, float> pricePerDay { get; set; }
        public int CustomerId { get; set; }
        public int RoomId { get; set; }

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
            MapChanged.Invoke(this, aChangeEvent);
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

    enum reportType 
    {
        ExpectedOccupancy,
        ExpectedRoomIncome,
        Incentive,
        DailyArrivals,
        DailyOccupancy,
        AccommodationBill
    }

}