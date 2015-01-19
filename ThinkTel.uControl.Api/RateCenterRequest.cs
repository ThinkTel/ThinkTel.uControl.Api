using System;
using System.Runtime.Serialization;

namespace ThinkTel.uControl.Api
{
    [DataContract(Namespace = "http://schemas.thinktel.ca/ucontrol/api/", Name = "RateCenterRequest")]
    public class RateCenterRequest
    {
        [DataMember(IsRequired = true)]
        public string RateCenterName { get; set; }
        [DataMember(IsRequired = true)]
        public int Quantity { get; set; }
    }
}