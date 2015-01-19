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
	}
}