using System;
using System.Runtime.Serialization;

namespace ThinkTel.uControl.Api
{
    [DataContract(Namespace = "http://schemas.thinktel.ca/ucontrol/api/", Name = "DidRequest")]
    public class DidRequest
    {
        [DataMember(IsRequired = true)]
        public long Number { get; set; }
    }
}