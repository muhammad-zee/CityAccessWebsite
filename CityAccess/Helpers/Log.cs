 using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace CityAccess
{
    public class Log
    {
        public static Logobj Changes(object A, object B)
        {
            string diferences = "-";
            string A_Valstr;
            string B_Valstr;
            Logobj logobj = new Logobj();
            Boolean count = false;

            Type A_Type = A.GetType();
            if (B.GetType() != A_Type)
            {
                logobj.changes = "Erro de tipo";
                return logobj; 
            }


            string Htmlchanges = "<br/><br><table cellpadding='4' border='1' style='line-height:1.5;font-size:12px;border-style:groove;border-color:rgb(63, 150, 170);border-width:1px;border-collapse:collapse;'><thead style ='background-color:rgb(63,150,170);color:white;'>"
                + "<tr><th>Field</th><th>Old value</th><th>New value</th></tr></thead><tbody>";

            foreach (PropertyInfo propertyInfo in A_Type.GetProperties())
            {
                if (propertyInfo.CanRead)
                {
                    object A_Value = propertyInfo.GetValue(A, null);
                    object B_Value = propertyInfo.GetValue(B, null);

                    Boolean check = false;

                    if (propertyInfo.PropertyType.IsGenericType && Nullable.GetUnderlyingType(propertyInfo.PropertyType) == null)
                    {
                        check = true;
                    }


                    if (!object.Equals(A_Value, B_Value) && check == false)
                    {
                        object Name = propertyInfo.Name;

                        string obj_chg = Name.ToString();

                        if(obj_chg == "stateID")
                        {
                            obj_chg = "Status";

                        }
                        if (obj_chg == "price")
                        {
                            obj_chg = "Price";
                        }
                        if (obj_chg == "eventDate")
                        {
                            obj_chg = "Date";
                        }
                        if (obj_chg == "eventTime")
                        {
                            obj_chg = "Time";
                        }
                        if (obj_chg == "notes")
                        {
                            obj_chg = "Notes";
                        }
                        if (obj_chg == "contactName")
                        {
                            obj_chg = "Client name";
                        }
                        if (obj_chg == "contactEmail")
                        {
                            obj_chg = "Client e-mail";
                        }
                        if (obj_chg == "contactPhone")
                        {
                            obj_chg = "Client phone";
                        }
                        if (obj_chg == "nrPersons")
                        {
                            obj_chg = "Number of persons";
                        }
                        if (obj_chg == "pickupLocation")
                        {
                            obj_chg = "Pick up location";
                        }
                        if (obj_chg == "dropoffLocation")
                        {
                            obj_chg = "Drop off location";
                        }
                        if (obj_chg == "flightNr")
                        {
                            obj_chg = "Flight number";
                        }
                        if (obj_chg == "returnDate")
                        {
                            obj_chg = "Return date";
                        }
                        if (obj_chg == "returnTime")
                        {
                            obj_chg = "Return time";
                        }
                        if (obj_chg == "returnPickup")
                        {
                            obj_chg = "Return pick up location";
                        }
                        if (obj_chg == "returnDropoff")
                        {
                            obj_chg = "Return drop off location";
                        }
                        if (obj_chg == "reference")
                        {
                            obj_chg = "Reference";
                        }
                        if (obj_chg == "returnFlight")
                        {
                            obj_chg = "Return flight";
                        }

                        if (obj_chg == "ClientNotes")
                        {
                            obj_chg = "Client Notes";
                        }

                        if (obj_chg != "password" && obj_chg != "Oldpassword" && obj_chg != "passwordConfirm") 
                        {
                            if (A_Value != null && B_Value != null)
                            {
                                A_Valstr = A_Value.ToString();
                                B_Valstr = B_Value.ToString();
                                diferences = diferences + obj_chg + "  from " + A_Valstr + " to " + B_Valstr + "\n";
                                Htmlchanges = Htmlchanges + "<tr><td>" + obj_chg + "</td><td> " + A_Valstr + " </td><td> " + B_Valstr + " </td></tr>";
                            }
                            else
                            {

                                if (A_Value == null)
                                {
                                    B_Valstr = B_Value.ToString();
                                    diferences = diferences + obj_chg + "  from " + " no value" + " to " + B_Value.ToString() + "\n";
                                    Htmlchanges = Htmlchanges + "<tr><td>" + obj_chg + "</td><td> no value </td><td> " + B_Valstr + " </td></tr>";
                                }
                                else
                                {
                                    A_Valstr = A_Value.ToString();
                                    diferences = diferences + obj_chg + "  from " + A_Value.ToString() + " to " + " no value" + "\n";
                                    Htmlchanges = Htmlchanges + "<tr><td>" + obj_chg + "</td><td> " + A_Valstr + " </td><td> no value </td></tr>";
                                }
                            }
                        }
                        else
                        {
                            if (count != true)
                            {
                                count = true;
                                diferences = diferences + "User password was changed \n";
                            }
                        }
                    }
                }
            }
        
            Htmlchanges = Htmlchanges + "</tbody></table>";

            logobj.Htmlchanges = Htmlchanges;
            logobj.changes = diferences; 
            return logobj;
        }
    }
}