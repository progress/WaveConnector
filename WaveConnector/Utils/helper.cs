using Microsoft.Data.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveConnector.Utils
{
    public class helper
    {
        public string getODataToSFDCType(string datatype)
        {

            if(datatype.Equals("Edm.String") || datatype.Equals("Edm.Binary") || datatype.Equals("Null") || datatype.Equals("Edm.Guid") || datatype.Equals("Edm.Time") || datatype.Equals("Edm.Boolean"))
            {
                return "Text";
            } else if(datatype.Equals("Edm.DateTime") || datatype.Equals("Edm.DateTimeOffset"))
            {
                return "Date";
            }else
            {
                return "Numeric";
            }

        }

        public string[] getfieldProperties(string typeinfo)
        {
            
            string temp = typeinfo.Trim(new char[] { '[',']' });
            char[] seperator = new char[1];
            seperator[0] = ' ';
            return temp.Split(seperator);
        }
    }
}
