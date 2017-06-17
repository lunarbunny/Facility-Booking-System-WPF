using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace AssignmentS2P2
{
    static class Extensions
    {
        /// <summary>
        /// Returns a string's salted SHA256 Hash value.
        /// </summary>
        internal static string ToSaltedSha256Hash(this string text, string salt) // Computes salted SHA256 hash of a string using given salt
        {
            if (String.IsNullOrWhiteSpace(text))
                return String.Empty;

            using (SHA256Managed sha = new SHA256Managed())
            {
                string saltedText = String.Concat(text, salt); // Combine plain text with given salt
                byte[] textData = Encoding.UTF8.GetBytes(saltedText); // Convert into bytes
                byte[] hash = sha.ComputeHash(textData); // Compute hash with bytes
                return BitConverter.ToString(hash).Replace("-", String.Empty); // Convert bytes into hexadecimal
            }
        }

        /// <summary>
        /// Generates a random salt.
        /// </summary>
        internal static string GenerateSalt() // Generate a random salt value
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                var bytes = new Byte[32];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
        }

        /// <summary>
        /// Returns a string containing the order type and which index the order is at.
        /// </summary>
        internal static string GetOrderCount(this Order order) // Get order number (index + 1) and type from a cart item
        {
            Type orderType = order.GetType();
            string type = String.Empty;
            if (orderType == typeof(ResourceHotel))
            {
                type = "(Hotel Order)";
            }
            else if (orderType == typeof(ResourceSport))
            {
                type = "(Sport Order)";
            }

            return String.Format("Order {0} {1}", (Cart.userCart.IndexOf(order) + 1), type);
        }

        /// <summary>
        /// Returns detailed info for a booking.
        /// </summary>
        internal static List<string> GetOrderInfo(this Order order, bool returnHeader)
        {
            List<string> orderDataList = new List<string>();

            if (returnHeader)
                orderDataList.Add(order.GetOrderCount());

            ResourceHotel hotelObj;
            ResourceSport sportObj;
            if (order is ResourceHotel)
            {
                hotelObj = (ResourceHotel)order;
                orderDataList.Add(String.Format("Username: {0}", hotelObj.booking_User));
                orderDataList.Add(String.Format("Check in: {0}", hotelObj.checkInDate.ToString("dd MMMM yyyy")));
                orderDataList.Add(String.Format("Check out: {0}", hotelObj.checkOutDate.ToString("dd MMMM yyyy")));
                orderDataList.Add(String.Format("Room ID: {0}", hotelObj.roomID.ToString()));
                orderDataList.Add(String.Format("Days of stay: {0}", hotelObj.daysOfStay.ToString()));
                orderDataList.Add(String.Format("Room Class: {0}", hotelObj.roomChoice.ConvertIndexToString(hotelObj, 1, (hotelObj.roomChoice - 1))));
                orderDataList.Add(String.Format("Bed Choice: {0}", hotelObj.bedChoice.ConvertIndexToString(hotelObj, 2, (hotelObj.bedChoice - 1))));
                orderDataList.Add(String.Format("View Choice: {0}", hotelObj.viewChoice.ConvertIndexToString(hotelObj, 3, (hotelObj.viewChoice - 1))));
                orderDataList.Add(String.Format("Wi-Fi Network: {0}", hotelObj.services[0].ToString()));
                orderDataList.Add(String.Format("Room Service: {0}", hotelObj.services[1].ToString()));
                orderDataList.Add(String.Format("House Keeping: {0}", hotelObj.services[2].ToString()));
                orderDataList.Add(String.Format("VIP Express Queue: {0}", hotelObj.services[3].ToString()));
                orderDataList.Add(String.Format("Price: {0:C2}", hotelObj.price));
            }
            else if (order is ResourceSport)
            {
                sportObj = (ResourceSport)order;
                orderDataList.Add(String.Format("Username: {0}", sportObj.booking_User));
                orderDataList.Add(String.Format("Booking date: {0}", sportObj.bookingDate.ToString("dd MMMM yyyy")));
                orderDataList.Add(String.Format("Sports Facility: {0}", sportObj.facilityChoice.ConvertIndexToString(sportObj, 1, (sportObj.facilityChoice - 1))));
                orderDataList.Add(String.Format("Time Slot: {0}", sportObj.bookingSlot.ConvertIndexToString(sportObj, 2, (sportObj.bookingSlot - 1))));
                orderDataList.Add(String.Format("Duration: {0} Hour(s)", sportObj.bookingDuration.ToString()));
                orderDataList.Add(String.Format("Price: {0:C2}", sportObj.price));
            }

            return orderDataList;
        }
        /// <summary>
        /// Returns detailed info for a booking.
        /// </summary>
        internal static List<string> GetOrderInfo(this HotelBooking order) // Get detailed view of data in a booking object
        {
            return ((Order)order).GetOrderInfo(false);
        }
        /// <summary>
        /// Returns detailed info for a booking.
        /// </summary>
        internal static List<string> GetOrderInfo(this SportBooking order) // Get detailed view of data in a booking object
        {
            return ((Order)order).GetOrderInfo(false);
        }

        /// <summary>
        /// Converts index based selections from combobox to readable text.
        /// <para></para>
        /// For 2nd parameter: (Types)
        /// <para></para>
        /// Hotel: <para></para>
        /// 1 => Room class <para></para>
        /// 2 => Bed type <para></para>
        /// 3 => View choice <para></para>
        /// Sport: <para></para>
        /// 1 => Facility <para></para>
        /// 2 => Time Slot <para></para>
        /// </summary>
        internal static string ConvertIndexToString(this int i, object obj, int type, int index)
        {
            if (obj.GetType() == typeof(ResourceHotel) || obj.GetType() == typeof(HotelBooking))
            {
                switch (type)
                {
                    case 1:
                        return ResourceCollection.hotelRoomTypes[index];
                    case 2:
                        return ResourceCollection.hotelBedTypes[index];
                    case 3:
                        return ResourceCollection.hotelRoomViews[index];
                }
            }
            else if (obj.GetType() == typeof(ResourceSport) || obj.GetType() == typeof(SportBooking))
            {
                switch (type)
                {
                    case 1:
                        return ResourceCollection.sportFacilityTypes[index];
                    case 2:
                        return ResourceCollection.sportTimeSlots[index];
                }
            }
            return String.Empty;
        }

        /// <summary>
        /// Convert TimeSpan to readable string
        /// </summary>
        public static string ToReadableString(this TimeSpan span) // Convert timespan to readable string (For transaction time eclapsed)
        {
            // "{0:0}" => If value is present replace it, otherwise display as 0.
            // (?:) => If LHS is true return LHS, otherwise return RHS. Eg. [bool x = y : z] means [x = y (y is true)] or [x = z (y is false)]
            string formatted = String.Format("{0}{1}{2}",
                span.Duration().Hours > 0 ? String.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? String.Empty : "s") : String.Empty,
                span.Duration().Minutes > 0 ? String.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? String.Empty : "s") : String.Empty,
                span.Duration().Seconds > 0 ? String.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? String.Empty : "s") : String.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (String.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }
    }
}