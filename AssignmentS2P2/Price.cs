using System;

namespace AssignmentS2P2
{
    // The classes here are for loading price values, calculating booking price and contains models to store price values for booking items
    static class Price
    {
        internal static void LoadPrice()
        {
            using (BookingSystemDBEntities context = new BookingSystemDBEntities())
            {
                var priceList = context.PriceTables.SqlQuery("SELECT * from dbo.PriceTable");
                foreach (PriceTable row in priceList)
                {
                    switch (row.Item)
                    {
                        case "HotelRate-FirstDay":
                            HotelPriceModel.hotelFirstDay = row.Price;
                            break;
                        case "HotelRate-Subsequent":
                            HotelPriceModel.hotelSubsequent = row.Price;
                            break;
                        case "Room-Standard":
                            HotelPriceModel.roomStandard = row.Price;
                            break;
                        case "Room-Premium":
                            HotelPriceModel.roomPremium = row.Price;
                            break;
                        case "Room-Superior":
                            HotelPriceModel.roomSuperior = row.Price;
                            break;
                        case "Room-Deluxe":
                            HotelPriceModel.roomDeluxe= row.Price;
                            break;
                        case "Room-Mini-Suite":
                            HotelPriceModel.roomMiniSuite = row.Price;
                            break;
                        case "Room-Suite":
                            HotelPriceModel.roomSuite = row.Price;
                            break;
                        case "Room-Studio":
                            HotelPriceModel.roomStudio = row.Price;
                            break;
                        case "Bed-Single":
                            HotelPriceModel.bedSingle = row.Price;
                            break;
                        case "Bed-Double":
                            HotelPriceModel.bedDouble = row.Price;
                            break;
                        case "Bed-Triple":
                            HotelPriceModel.bedTriple = row.Price;
                            break;
                        case "Bed-Twin":
                            HotelPriceModel.bedTwin = row.Price;
                            break;
                        case "Bed-DoubleDouble":
                            HotelPriceModel.bedDoubleDouble = row.Price;
                            break;
                        case "View-City":
                            HotelPriceModel.viewCity = row.Price;
                            break;
                        case "View-Garden":
                            HotelPriceModel.viewGarden = row.Price;
                            break;
                        case "View-Island":
                            HotelPriceModel.viewIsland = row.Price;
                            break;
                        case "View-Ocean":
                            HotelPriceModel.viewOcean = row.Price;
                            break;
                        case "Extras-WifiNetwork":
                            HotelPriceModel.extrasWifiNetwork = row.Price;
                            break;
                        case "Extras-RoomService":
                            HotelPriceModel.extrasRoomService = row.Price;
                            break;
                        case "Extras-HouseKeeping":
                            HotelPriceModel.extrasHouseKeeping = row.Price;
                            break;
                        case "Extras-ExpressQueue":
                            HotelPriceModel.extrasExpressQueue = row.Price;
                            break;
                        case "SportRate-Flat":
                            SportPriceModel.rateFlat = row.Price;
                            break;
                        case "SportRate-Weekday":
                            SportPriceModel.rateWeekday = row.Price;
                            break;
                        case "SportRate-Weekend":
                            SportPriceModel.rateWeekend = row.Price;
                            break;
                        case "SportRate-Duration":
                            SportPriceModel.rateDuration = row.Price;
                            break;
                        case "SportFacility-Normal":
                            SportPriceModel.facilityNormal = row.Price;
                            break;
                        case "SportFacility-Special":
                            SportPriceModel.facilitySpecial = row.Price;
                            break;
                        case "SportTimeSlot-Morning":
                            SportPriceModel.timeSlotMorning = row.Price;
                            break;
                        case "SportTimeSlot-Afternoon":
                            SportPriceModel.timeSlotAfternoon = row.Price;
                            break;
                        case "SportTimeSlot-Night":
                            SportPriceModel.timeSlotNight = row.Price;
                            break;
                    }
                }
            }
        }

        internal static decimal CalculateHotelBookingPrice(int daysOfStay, int roomChoice, int bedChoice, int viewChoice, bool[] services)
        {
            decimal currentPrice = 0m;
            try
            {
                currentPrice += HotelPriceModel.hotelFirstDay + (daysOfStay * HotelPriceModel.hotelSubsequent);

                switch (roomChoice)
                {
                    case 1:
                        currentPrice += HotelPriceModel.roomStandard;
                        break;
                    case 2:
                        currentPrice += HotelPriceModel.roomPremium;
                        break;
                    case 3:
                        currentPrice += HotelPriceModel.roomSuperior;
                        break;
                    case 4:
                        currentPrice += HotelPriceModel.roomDeluxe;
                        break;
                    case 5:
                        currentPrice += HotelPriceModel.roomMiniSuite;
                        break;
                    case 6:
                        currentPrice += HotelPriceModel.roomMiniSuite;
                        break;
                    case 7:
                        currentPrice += HotelPriceModel.roomSuite;
                        break;
                }
                switch (bedChoice)
                {
                    case 1:
                        currentPrice += HotelPriceModel.bedSingle;
                        break;
                    case 2:
                        currentPrice += HotelPriceModel.bedDouble;
                        break;
                    case 3:
                        currentPrice += HotelPriceModel.bedTriple;
                        break;
                    case 4:
                        currentPrice += HotelPriceModel.bedTwin;
                        break;
                    case 5:
                        currentPrice += HotelPriceModel.bedDoubleDouble;
                        break;
                }
                switch (viewChoice)
                {
                    case 1:
                        currentPrice += HotelPriceModel.viewCity;
                        break;
                    case 2:
                        currentPrice += HotelPriceModel.viewGarden;
                        break;
                    case 3:
                        currentPrice += HotelPriceModel.viewIsland;
                        break;
                    case 4:
                        currentPrice += HotelPriceModel.viewOcean;
                        break;
                }
                if (services[0])
                    currentPrice += HotelPriceModel.extrasWifiNetwork;
                if (services[1])
                    currentPrice += HotelPriceModel.extrasRoomService;
                if (services[2])
                    currentPrice += HotelPriceModel.extrasHouseKeeping;
                if (services[3])
                    currentPrice += HotelPriceModel.extrasExpressQueue;
                return currentPrice;
            }
            finally
            {
                currentPrice = 0m; // Reset price
            }
        }

        internal static decimal CalculateSportBookingPrice(DateTime bookingDate, int facilityChoice, int timeSlotChoice, int duration)
        {
            decimal currentPrice = 0m;
            try
            {
                currentPrice += SportPriceModel.rateFlat;
                if (bookingDate.DayOfWeek == DayOfWeek.Saturday || bookingDate.DayOfWeek == DayOfWeek.Sunday)
                    currentPrice += SportPriceModel.rateWeekend;
                else
                    currentPrice += SportPriceModel.rateWeekday;

                switch (facilityChoice)
                {
                    case 4: case 6: case 7:
                        currentPrice += SportPriceModel.facilitySpecial;
                        break;
                    default:
                        currentPrice += SportPriceModel.facilityNormal;
                        break;
                }

                switch (timeSlotChoice)
                {
                    case 1: case 2: case 3:
                        currentPrice += SportPriceModel.timeSlotMorning;
                        break;
                    case 4: case 5: case 6:
                        currentPrice += SportPriceModel.timeSlotAfternoon;
                        break;
                    case 7: case 8:
                        currentPrice += SportPriceModel.timeSlotNight;
                        break;
                }

                currentPrice += duration * SportPriceModel.rateDuration;

                return currentPrice;
            }
            finally
            {
                currentPrice = 0m;
            }
        }
    }

    static class HotelPriceModel
    {
        internal static decimal hotelFirstDay { get; set; }
        internal static decimal hotelSubsequent { get; set; }
        internal static decimal roomStandard { get; set; }
        internal static decimal roomPremium { get; set; }
        internal static decimal roomSuperior { get; set; }
        internal static decimal roomDeluxe { get; set; }
        internal static decimal roomMiniSuite { get; set; }
        internal static decimal roomSuite { get; set; }
        internal static decimal roomStudio { get; set; }
        internal static decimal bedSingle { get; set; }
        internal static decimal bedDouble { get; set; }
        internal static decimal bedTriple { get; set; }
        internal static decimal bedTwin { get; set; }
        internal static decimal bedDoubleDouble { get; set; }
        internal static decimal viewCity { get; set; }
        internal static decimal viewGarden { get; set; }
        internal static decimal viewIsland { get; set; }
        internal static decimal viewOcean { get; set; }
        internal static decimal extrasWifiNetwork { get; set; }
        internal static decimal extrasRoomService { get; set; }
        internal static decimal extrasHouseKeeping { get; set; }
        internal static decimal extrasExpressQueue { get; set; }
    }

    static class SportPriceModel
    {
        internal static decimal rateFlat { get; set; }
        internal static decimal rateWeekday { get; set; }
        internal static decimal rateWeekend { get; set; }
        internal static decimal facilityNormal { get; set; }
        internal static decimal facilitySpecial { get; set; }
        internal static decimal timeSlotMorning { get; set; }
        internal static decimal timeSlotAfternoon { get; set; }
        internal static decimal timeSlotNight { get; set; }
        internal static decimal rateDuration { get; set; }
    }
}