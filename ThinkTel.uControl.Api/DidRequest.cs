using System;
using System.Runtime.Serialization;

namespace ThinkTel.uControl.Api
{
    [DataContract(Namespace = "http://schemas.thinktel.ca/ucontrol/api/", Name = "DidRequest")]
    public class DidRequest
    {
        [DataMember(IsRequired = true)]
        public long Number { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is DidRequest)
			{
				var other = (DidRequest)obj;
				return Number == other.Number;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return Number.GetHashCode();
		}
    }
}