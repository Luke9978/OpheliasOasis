using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace OpheliasOasis
{
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