using System.Linq;
using System.Data;

namespace AssignmentS2P2
{
    // This class handles authentication and registration services
    // Cannot be inherited
    sealed class Authentication
    {
        private readonly string username, password;
        private BookingSystemDBEntities context;

        internal Authentication(string _username,string _password)
        {
            this.username = _username;
            this.password = _password;
        }
        internal bool[] Login()
        {
            bool[] authenticated = { false, false };
            using (context = new BookingSystemDBEntities())
            {
                User user = context.Users
                    .Where(u => u.Username == this.username)
                    .FirstOrDefault();
                if (user != null)
                {
                    string passwordHash = this.password.ToSaltedSha256Hash(user.Password_Salt);
                    if (passwordHash.Equals(user.Password_Hash))
                    {
                        authenticated[0] = true;
                        if (user.User_Group.Equals("Admin"))
                            authenticated[1] = true;
                    }
                }
            }
            return authenticated;
        }

        internal bool Register()
        {
            using (context = new BookingSystemDBEntities())
            {
                string salt = Extensions.GenerateSalt();
                User user = new User() { Username = this.username, Password_Hash = this.password.ToSaltedSha256Hash(salt), Password_Salt = salt, User_Group = "User" };
                context.Users.Add(user);
                try
                {
                    context.SaveChanges();
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException) // Existing username conflict
                {
                    return false;
                }
                return true;
            }
        }
    }
}