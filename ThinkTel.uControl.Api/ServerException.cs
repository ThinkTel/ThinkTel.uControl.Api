using System;
using System.Runtime.Serialization;

namespace ThinkTel.uControl.Api
{
	[DataContract(Namespace = "http://schemas.thinktel.ca/ucontrol/api/", Name = "Exception")]
	public class ServerException
	{
		[DataMember]
		public string ID { get; set; }
		[DataMember]
		public string Message { get; set; }

		public override string ToString()
		{
			return string.Format("[{0}] {1}", ID, Message);
		}
	}
}