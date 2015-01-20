using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ThinkTel.uControl.Api
{
	[DataContract(Namespace = "http://schemas.thinktel.ca/ucontrol/api/")]
	public class V911
	{
		[DataMember(IsRequired = true)]
		public long Number { get; set; }
		[DataMember(IsRequired = true), StringLength(38)]
		public string FirstName { get; set; }
		[DataMember(IsRequired = true), StringLength(100)]
		public string LastName { get; set; }
		[DataMember(IsRequired = true), StringLength(10)]
		public string StreetNumber { get; set; }
		[DataMember(IsRequired = true), StringLength(84)]
		public string StreetName { get; set; }
		[DataMember, StringLength(30)]
		public string SuiteNumber { get; set; }
		[DataMember(IsRequired = true), StringLength(38)]
		public string City { get; set; }
		[DataMember(IsRequired = true), StringLength(2)]
		public string ProvinceState { get; set; }
		[DataMember(IsRequired = true), StringLength(10)]
		public string PostalZip { get; set; }
		[DataMember, StringLength(38)]
		public string OtherInfo { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is V911)
			{
				var other = (V911)obj;
				return Number == other.Number && string.Equals(FirstName, other.FirstName) &&
					string.Equals(LastName, other.LastName) && string.Equals(StreetNumber, other.StreetNumber) &&
					string.Equals(StreetName, other.StreetName) && string.Equals(SuiteNumber, other.SuiteNumber) &&
					string.Equals(City, other.City) && string.Equals(ProvinceState, other.ProvinceState) &&
					string.Equals(OtherInfo, other.OtherInfo);
			}
			else
			{
				return false;
			}
		}

		public override int GetHashCode()
		{
			return string.Join("-", new object[] {
				Number, FirstName, LastName, StreetNumber, StreetName, SuiteNumber, City, ProvinceState, PostalZip, OtherInfo
			}).GetHashCode();
		}
	}
}