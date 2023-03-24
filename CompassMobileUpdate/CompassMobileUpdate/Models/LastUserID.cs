using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace CompassMobile.Models
{
    public class LastUserID
    {
        [PrimaryKey]
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

