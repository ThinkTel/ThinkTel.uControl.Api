using System;
using System.Runtime.Serialization;

namespace ThinkTel.uControl.Api
{
    [DataContract(Namespace = "http://schemas.thinktel.ca/ucontrol/api/", Name = "NumberRange")]
    public class NumberRange
    {
        [DataMember(IsRequired = true)]
        public long FirstNumber { get; set; }
        [DataMember(IsRequired = true)]
        public long LastNumber { get; set; }
    }
}