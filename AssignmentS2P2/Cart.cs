using System.Collections.Generic;
using System.Linq;

namespace AssignmentS2P2
{
    static class Cart
    {
        // Cart for handling all bookings.
        // Has methods for adding booking objects/counting items in cart/get current cart/clearing cart/checkout.
        internal static List<Order> userCart = new List<Order>();
        private static BookingSystemDBEntities context;
        internal static void AddItem(Order resourceObject) // Add an item into cart
        {
            userCart.Add(resourceObject);
            userCart.Sort((a, b) => a.GetType().FullName.CompareTo(b.GetType().FullName));
        }

        internal static void RemoveItem(Order resourceObject) // Remove an item from cart
        {
            userCart.Remove(resourceObject);
            userCart.Sort((a, b) => a.GetType().FullName.CompareTo(b.GetType().FullName)); // Sort hotel bookings above sports bookings
        }

        internal static void Clear() // Clear cart
        {
            userCart.Clear();
        }

        internal static int GetCartCount() // Return number of orders in cart
        {
            int cartCount = 0;
            foreach (Order x in userCart)
            {
                cartCount++;
            }
            return cartCount;
        }

        internal static decimal GetCartTotalPrice() // Calculate total price for all items in cart
        {
            decimal totalPrice = 0m;
            foreach (Order orderObj in userCart)
            {
                totalPrice += orderObj.price;
            }
            return totalPrice;
        }

        internal static void Checkout() // Checkout and process items in cart
        {
            UserTransaction.transactionSession.TransactionEndEvent(); // End tranasction

            using (context = new BookingSystemDBEntities())
            {
                // Process current transaction details
                Transaction transDBObj = new Transaction()
                {
                    TransactionDate = UserTransaction.transactionSession.TransactionDate,
                    SessionStart = UserTransaction.transactionSession.SessionStart,
                    SessionEnd = UserTransaction.transactionSession.SessionEnd,
                    SessionDuration = UserTransaction.transactionSession.SessionDuration
                };
                context.Transactions.Add(transDBObj);

                // Process hotel room booking orders
                foreach (ResourceHotel hotel in userCart.OfType<ResourceHotel>())
                {
                    // Instance of a row in HotelRoomServices Table
                    HotelRoomService serviceDBObj = new HotelRoomService
                    {
                        WiFi_Service = hotel.services[0],
                        Room_Service = hotel.services[1],
                        House_Keeping = hotel.services[2],
                        Express_Queue = hotel.services[3],
                        Bed_Type = hotel.bedChoice
                    };
                    context.HotelRoomServices.Add(serviceDBObj);

                    // Instance of a row in HotelBookings Table
                    HotelBooking bookingDBObj = new HotelBooking
                    {
                        Booking_User = hotel.booking_User,
                        RoomID = hotel.roomID,
                        CheckInDate = hotel.checkInDate,
                        CheckOutDate = hotel.checkOutDate,
                        ServiceID = serviceDBObj.ServiceID,
                        Price = hotel.price,
                        TransactionID = transDBObj.TransactionID
                    };
                    context.HotelBookings.Add(bookingDBObj);
                    context.SaveChanges(); // Update to refresh identity column, prevent reuse of Primary Key IDs.
                }
                
                // Process facility booking orders
                foreach (ResourceSport sport in userCart.OfType<ResourceSport>())
                {
                    // Instance of a row in SportBookings Table
                    SportBooking bookingDBObj = new SportBooking
                    {
                        Booking_User = sport.booking_User,
                        BookingDate = sport.bookingDate,
                        FacilityId = sport.facilityChoice,
                        TimeSlot = sport.bookingSlot,
                        Duration = sport.bookingDuration,
                        Price = sport.price,
                        TransactionID = transDBObj.TransactionID
                    };
                    context.SportBookings.Add(bookingDBObj);
                }

                // Final update database
                context.SaveChanges();
            }
        }
    }
}