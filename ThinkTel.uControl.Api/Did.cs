using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ThinkTel.uControl.Api
{
    [DataContract(Namespace = "http://schemas.thinktel.ca/ucontrol/api/", Name = "Did")]
    public class Did
    {
        [DataMember(IsRequired = true)]
        public long Number { get; set; }
        [DataMember, StringLength(64)]
        public string Label { get; set; }
        [DataMember]
        public long? TranslatedNumber { get; set; }
    }
}