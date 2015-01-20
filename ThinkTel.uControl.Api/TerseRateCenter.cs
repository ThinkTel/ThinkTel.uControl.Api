using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ThinkTel.uControl.Api
{
    [DataContract(Namespace = "http://schemas.thinktel.ca/ucontrol/api/", Name = "TerseRateCenter")]
    public class TerseRateCenter
    {
        [DataMember(IsRequired = true), StringLength(255)]
        public string Name { get; set; }
        [DataMember, StringLength(2)]
        public string Country { get; set; }
        [DataMember]
        public uint Available { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is TerseRateCenter)
			{
				var other = (TerseRateCenter)obj;
				return string.Equals(Name, other.Name) && string.Equals(Country, other.Country) && Available == other.Available;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return string.Format("{0}-{1}-{2}", Name, Country, Available).GetHashCode();
		}
    }
}