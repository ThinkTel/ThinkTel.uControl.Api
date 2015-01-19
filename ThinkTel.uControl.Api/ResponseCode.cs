using System;
using System.Runtime.Serialization;

namespace ThinkTel.uControl.Api
{
    [DataContract(Namespace = "http://schemas.thinktel.ca/ucontrol/api/")]
    public enum ResponseCode
    {
        [EnumMember]
        Rejected,
        [EnumMember]
        Accepted,
        [EnumMember]
        Deferred,
        [EnumMember]
        Conditional,
        [EnumMember]
        Error
    }
}