using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace WaveConnector.Utils
{
    class NullPropertiesConverter: JavaScriptConverter
    {
        public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Type> SupportedTypes
        {
            get { return GetType().Assembly.GetTypes(); }
        }

        public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
        {
            var json = new Dictionary<string, object>();
            foreach (var prop in obj.GetType().GetProperties())
            {

                var value = prop.GetValue(obj, BindingFlags.Public, null, null, null);
                if (value != null)
                    json.Add(prop.Name, value);
            }
            return json;
        }

    }
}
