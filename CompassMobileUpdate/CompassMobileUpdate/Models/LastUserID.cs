using System;
using System.Collections.Generic;
using System.Text;

namespace CompassMobile.Models
{
    public class LastUserID
    {
        public string UserID { get; set; }
        public LastUserID()
        {

        }
        public LastUserID(string userID)
        {
            this.UserID = userID;
        }
    }
}

