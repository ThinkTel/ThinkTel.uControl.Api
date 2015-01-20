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

		public override bool Equals(object obj)
		{
			if (obj is NumberRange)
			{
				var other = (NumberRange)obj;
				return FirstNumber == other.FirstNumber && LastNumber == other.LastNumber;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return string.Format("{0}-{1}", FirstNumber, LastNumber).GetHashCode();
		}
    }
}