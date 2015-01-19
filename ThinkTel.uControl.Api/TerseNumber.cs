using System;
using System.Runtime.Serialization;

namespace ThinkTel.uControl.Api
{
    [DataContract(Namespace = "http://schemas.thinktel.ca/ucontrol/api/", Name = "TerseNumber")]
    public class TerseNumber
    {
        [DataMember]
        public string Label { get; set; }
        [DataMember]
        public long Number { get; set; }
    }
}