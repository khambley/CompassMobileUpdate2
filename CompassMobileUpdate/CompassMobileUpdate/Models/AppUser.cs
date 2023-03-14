using System;
using CompassMobileUpdate.Helpers;
using SQLite;
using Xamarin.Auth;

namespace CompassMobileUpdate.Models
{
    public class AppUser
    {
        [PrimaryKey]
        public string UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string CommonName
        {
            get
            {
                if (FirstName != null && LastName != null)
                {
                    return LastName + ", " + FirstName;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string JWT { get; set; }
        public DateTime JWTExpirationUTC { get; set; }
        public AppUser()
        {

        }
        public AppUser(string userID)
        {
            UserID = userID;
        }

        public Account GetAccountFromUser()
        {
            Account account = new Account();
            account.Username = this.UserID;
            if (!string.IsNullOrWhiteSpace(this.FirstName))
            {
                account.Properties.Add("FirstName", this.FirstName);
            }
            if (!string.IsNullOrWhiteSpace(this.LastName))
            {
                account.Properties.Add("LastName", this.LastName);
            }
            account.Properties.Add("JWT", this.JWT);

            return account;
        }

        public static AppUser GetUserFromAccount(Account account)
        {
            AppUser user = new AppUser();
            user.UserID = account.Username;
            if (account.Properties.ContainsKey("FirstName"))
            {
                user.FirstName = account.Properties["FirstName"];
            }
            if (account.Properties.ContainsKey("LastName"))
            {
                user.LastName = account.Properties["LastName"];
            }
            user.JWT = account.Properties["JWT"];
            user.JWTExpirationUTC = JWTHelper.GetExpirationTimeFromJWT(user.JWT);
            return user;
        }
    }
}

