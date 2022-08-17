using CityAccess.Controllers;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection;
using System.Web;

namespace CityAccess
{
    public static class auxMethods
    {
        public static object UpdateHelper(object base_req, object view_req)
        {
            if (base_req.GetType() != view_req.GetType())
            {
                return null;
            }

            foreach (PropertyInfo propertyInfo in base_req.GetType().GetProperties())
            {
                if (propertyInfo.CanRead)
                {
                    object view_reqValue = propertyInfo.GetValue(view_req, null);
                    if (view_reqValue != null)
                    {
                        propertyInfo.SetValue(base_req, view_reqValue);
                    }
                }
            }
            return base_req;
        }


        public static void ErrorHandling(DbEntityValidationException dbEx, String url, int? userID)
        {
            String error = string.Empty;

            foreach (var validationErrors in dbEx.EntityValidationErrors)
            {
                foreach (var validationError in validationErrors.ValidationErrors)
                {
                    System.Diagnostics.Debug.WriteLine("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    error = error + "Property:" + validationError.PropertyName + " Error:" + validationError.ErrorMessage;
                }
            }

            ErrorController errorlog = new ErrorController();

            if(userID == null)
            {
                userID = 0;
            }
            else
            {
                userID = (int)userID;
            }

            var usID = (int)userID;

            errorlog.ErrorLog(usID, error, url);
        }



        //This method is based on an ID - if the number of users/partners increases a lot - REWRITE this method due to memory issues
        public static long IdToEncode(long num)
        {
            long alteredNum = num + 10000 * num + num * 3303;

                return alteredNum;
        }

        public static long DecodeId (long num)
        {
            long decodedNum = num/13304;

                return decodedNum;
        }
    }
}