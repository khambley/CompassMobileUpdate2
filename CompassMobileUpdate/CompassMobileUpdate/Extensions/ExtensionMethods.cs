using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CompassMobileUpdate.Extensions
{
    public static class ExtensionMethods
    {
        public static List<T> ToList<T>(this SQLite.TableQuery<T> tq)
        {
            List<T> returnList = new List<T>();
            foreach (var item in tq)
            {
                returnList.Add(item);
            }
            return returnList;
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> thisCollection)
        {
            if (thisCollection == null) return null;
            var oc = new ObservableCollection<T>();

            foreach (var item in thisCollection)
            {
                oc.Add(item);
            }

            return oc;
        }

        public static string GetInnermostMessage(this Exception ex)
        {
            Exception current = ex;
            string message = string.Empty;
            while (current != null)
            {
                message = current.Message;
                current = current.InnerException;
            }

            return message;
        }
    }
}

