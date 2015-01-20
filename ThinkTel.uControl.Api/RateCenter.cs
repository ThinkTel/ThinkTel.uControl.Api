using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ThinkTel.uControl.Api
{
    [DataContract(Namespace = "http://schemas.thinktel.ca/ucontrol/api/", Name = "RateCenter")]
    public class RateCenter
    {
        [DataMember(IsRequired = true), StringLength(255)]
        public string Name { get; set; }
        [DataMember, StringLength(2)]
        public string Country { get; set; }
        [DataMember]
        public uint Available { get; set; }
        [DataMember]
        public bool Local { get; set; }
        [DataMember]
        public bool OnNet { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is RateCenter)
			{
				var other = (RateCenter)obj;
				return string.Equals(Name, other.Name) && string.Equals(Country, other.Country) &&
					Available == other.Available && Local == other.Local && OnNet == other.OnNet;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return string.Format("{0}-{1}-{2}-{3}-{4}", Name, Country, Available, Local, OnNet).GetHashCode();
		}
    }
}