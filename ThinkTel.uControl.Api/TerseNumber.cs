using System;
using System.Runtime.Serialization;

namespace ThinkTel.uControl.Api
{
    [DataContract(Namespace = "http://schemas.thinktel.ca/ucontrol/api/", Name = "TerseNumber")]
    public class TerseNumber
    {
        [DataMember]
        public string Label { get; set; }
        [DataMember]
        public long Number { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is TerseNumber)
			{
				var other = (TerseNumber)obj;
				return string.Equals(Label, other.Label) && Number == other.Number;
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return string.Format("{0}-{1}", Label, Number).GetHashCode();
		}
    }
}