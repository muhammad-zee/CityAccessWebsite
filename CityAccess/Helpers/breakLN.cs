using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace CityAccess
{
    public class BreakLN
    {

        
        /// <summary>
        /// Replaces special characteres \r\n with <br/> useful for html rendering 
        /// </summary>
        /// <param name="content"></param>
        /// <returns> The string with the changes </returns> 
        public static String ProcessBrkHTML (string content)
        {

            String new_cont;
            String aux = "<br/>";
            int k = content.Length;
            string myString = new string(' ', k + 132);
            StringBuilder sb = new StringBuilder(myString, 100000);
            int j = 0;

            for (int i = 0; i < content.Length - 1; i++)
            {


                if (content[i] == '\r' && content[i + 1] == '\n')
                {
                    sb[j] = aux[0];
                    j++;
                    sb[j] = aux[1];
                    j++;
                    sb[j] = aux[2];
                    j++;
                    sb[j] = aux[3];
                    j++;
                    sb[j] = aux[4];
                    j++;

                    i++;
                }
                else
                {
                    sb[j] = content[i];
                    j++;
                }
            }
            new_cont = sb.ToString();

            return new_cont;
        }

    }
}