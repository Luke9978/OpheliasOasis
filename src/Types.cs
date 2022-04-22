using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation.Collections;

namespace OpheliasOasis
{
    public class MapChangeReason : IMapChangedEventArgs<int>
    {
        public CollectionChange CollectionChange { get; set; }

        public int Key { get; set; }
    }

    public class ReservationMap : IObservableMap<int, Reservation>
    {
        private readonly Dictionary<int, Reservation> _items;

        public event MapChangedEventHandler<int, Reservation> MapChanged;

        private void TiggerEvent(CollectionChange reason, int key)
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

        public void Add(int key, Reservation value)
        {
            _items.Add(key, value);
            TiggerEvent(CollectionChange.ItemInserted, key);
        }    

        public bool ContainsKey(int key)
        {
            return _items.ContainsKey(key);
        }

        public bool Remove(int key)
        {
            TiggerEvent(CollectionChange.ItemRemoved, key);
            return (_items.Remove(key));
        }

        public bool TryGetValue(int key, out Reservation value)
        {
            return _items.TryGetValue(key, out value);
        }

        public Reservation this[int key] { get => _items[key]; 
            set 
            {
                TiggerEvent(CollectionChange.ItemChanged, key);    
            }}

        public ICollection<int> Keys => _items.Keys.ToList();

        public ICollection<Reservation> Values => _items.Values.ToList();

        public void Add(KeyValuePair<int, Reservation> item)
        {
            TiggerEvent(CollectionChange.ItemInserted, item.Key);
            _items.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            TiggerEvent(CollectionChange.Reset, 0);
            _items.Clear();
        }

        public bool Contains(KeyValuePair<int, Reservation> item)
        {
            if (! _items.ContainsKey((int)item.Key))
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
            TiggerEvent(CollectionChange.ItemRemoved, item.Key);
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