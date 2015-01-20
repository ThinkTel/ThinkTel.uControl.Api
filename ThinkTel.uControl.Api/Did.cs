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

		public override bool Equals(object obj)
		{
			if (obj is Did)
			{
				var other = (Did)obj;
				return Number == other.Number && string.Equals(Label, other.Label) && TranslatedNumber == other.TranslatedNumber;
			}
			else
			{
				return false;
			}
		}
		public override int GetHashCode()
		{
			return string.Format("{0}-{1}-{2}", Number, Label, TranslatedNumber).GetHashCode();
		}
    }
}