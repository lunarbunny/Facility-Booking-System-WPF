using System.Linq;

namespace AssignmentS2P2
{
    abstract class Order // Represent a generic order
    {
        internal static Order orderObject;
        internal string booking_User;
        internal decimal price;

        internal Order(string _booking_User, decimal _price)
        {
            booking_User = _booking_User;
            price = _price;
        }

        internal abstract void CreateOrder();

        public static explicit operator Order(HotelBooking hb) // Convert HotelBooking row from database into object by explict casting
        {
            using (BookingSystemDBEntities context = new BookingSystemDBEntities())
            {
                var room = context.HotelRooms.Where(i => i.RoomID == hb.RoomID).FirstOrDefault();
                var services = context.HotelRoomServices.Where(i => i.ServiceID == hb.ServiceID).FirstOrDefault();

                Order res = new ResourceHotel()
                {
                    booking_User = hb.Booking_User,
                    price = hb.Price,
                    checkInDate = hb.CheckInDate,
                    checkOutDate = hb.CheckOutDate,
                    daysOfStay = (int)((hb.CheckOutDate - hb.CheckInDate).TotalDays),
                    roomID = hb.RoomID,
                    roomChoice = room.TypeID,
                    bedChoice = services.Bed_Type,
                    viewChoice = room.ViewID,
                    services = new bool[] { services.WiFi_Service, services.Room_Service, services.House_Keeping, services.Express_Queue }
                };

                return res;
            }
        }

        public static explicit operator Order(SportBooking sb) // Convert SportBooking row from database into object by explict casting
        {
            Order res = new ResourceSport()
            {
                booking_User = sb.Booking_User,
                price = sb.Price,
                bookingDate = sb.BookingDate,
                facilityChoice = sb.FacilityId,
                bookingDuration = sb.Duration,
                bookingSlot = sb.TimeSlot
            };

            return res;
        }
    }
}