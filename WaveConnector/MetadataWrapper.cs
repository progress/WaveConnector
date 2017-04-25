using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace WaveConnector
{
    [DataContract]
    public class MetadataWrapper
    {
        [DataContract]
        public class Rootobject
        {
            [DataMember]
            public Fileformat fileFormat { get; set; }
            [DataMember]
            public Object[] objects { get; set; }
        }

        [DataContract]
        public class Fileformat
        {
            [DataMember]
            public string charsetName { get; set; }
            [DataMember]
            public string fieldsEnclosedBy { get; set; }
            [DataMember]
            public string fieldsDelimitedBy { get; set; }
            [DataMember]
            public int? numberOfLinesToIgnore { get; set; }
        }

        [DataContract]
        public class Object
        {
            [DataMember]
            public string connector { get; set; }
            [ScriptIgnore]
            public string description { get; set; }
            [DataMember]
            public string fullyQualifiedName { get; set; }
            [DataMember]
            public string label { get; set; }

            [DataMember]
            public string name { get; set; }
            [ScriptIgnore]
            public string rowLevelSecurityFilter { get; set; }
            [DataMember]
            public Field[] fields { get; set; }
        }

        [DataContract]
        public class Field
        {
            [ScriptIgnore]
            public string description { get; set; }
            [DataMember]
            public string fullyQualifiedName { get; set; }
            [DataMember]
            public string label { get; set; }
            [DataMember]
            public string name { get; set; }
            [ScriptIgnore]
            public bool? isSystemField { get; set; }
            [DataMember]
            public bool? isUniqueId { get; set; }
            [ScriptIgnore]
            public bool? isMultiValue { get; set; }
            [DataMember]
            public string type { get; set; }
            [ScriptIgnore]
            public string defaultValue { get; set; }
            [ScriptIgnore]
            public int? precision { get; set; }

            [ScriptIgnore]
            public int? scale { get; set; }
            [ScriptIgnore]
            public string format { get; set; }
            [ScriptIgnore]
            public int? fiscalMonthOffset { get; set; }
            [ScriptIgnore]
            public bool? canTruncateValue { get; set; }
            [ScriptIgnore]
            public string decimalSeparator { get; set; }
            [ScriptIgnore]
            public bool? isYearEndFiscalYear { get; set; }
            [ScriptIgnore]
            public int? firstDayOfWeek { get; set;}
            [ScriptIgnore]
            public bool? isSkipped { get; set;}
        }

    }
}
