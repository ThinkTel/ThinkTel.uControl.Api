using System;
using System.Runtime.Serialization;

namespace ThinkTel.uControl.Api
{
    [DataContract(Namespace = "http://schemas.thinktel.ca/ucontrol/api/", Name = "NumberItem")]
    public class NumberItem
    {
        [DataMember(IsRequired = true)]
        public long Number { get; set; }
    }
}