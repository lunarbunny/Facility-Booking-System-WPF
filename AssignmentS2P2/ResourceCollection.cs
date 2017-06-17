using System.Collections.Generic;
using System.Linq;

namespace AssignmentS2P2
{
    static class ResourceCollection
    {
        private static BookingSystemDBEntities context;

        internal static List<string> hotelRoomTypes;
        internal static List<string> hotelBedTypes;
        internal static List<string> hotelRoomViews;
        internal static void LoadHotelControlResources()
        {
            using (context = new BookingSystemDBEntities())
            {
                hotelRoomTypes = context.Database.SqlQuery<string>( // Load room classes
                   "SELECT RoomClass FROM dbo.HotelRoomTypes").ToList();
                hotelBedTypes = context.Database.SqlQuery<string>( // Load bed types
                   "SELECT BedType FROM dbo.HotelBedTypes").ToList();
                hotelRoomViews = context.Database.SqlQuery<string>( // Load view types
                   "SELECT ViewType FROM dbo.HotelRoomViews").ToList();
            }
        }
        internal static List<string> sportFacilityTypes;
        internal static List<string> sportTimeSlots;
        internal static List<string> durationList;
        internal static void LoadSportControlResources()
        {
            using (context = new BookingSystemDBEntities())
            {
                sportFacilityTypes = context.Database.SqlQuery<string>( // Load facility types
                   "SELECT FacilityName FROM dbo.SportFacilities").ToList();
                sportTimeSlots = context.Database.SqlQuery<string>( // Load duration types
                   "SELECT TimeSlot FROM dbo.SportTimeSlots").ToList();
                durationList = context.Database.SqlQuery<string>( // Load custom durations
                    "SELECT Hours FROM dbo.SportDurations").ToList();
            }
        }
    }
}