using System;
using System.Collections.Generic;

namespace OpheliasOasis
{

    struct DateRange
    {
        DateTime start;
        DateTime end;
    }

    struct reservation
    {
        int id;
        DateRange date;
        IDictionary<DateTime, float> pricePerDay;
        int CustomerId;
        int RoomId;
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