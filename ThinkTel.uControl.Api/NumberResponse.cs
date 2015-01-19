using System;
using System.Runtime.Serialization;

namespace ThinkTel.uControl.Api
{
    [DataContract(Namespace = "http://schemas.thinktel.ca/ucontrol/api/", Name = "NumberResponse")]
    public class NumberResponse
    {
        [DataMember(IsRequired = true)]
        public long Number { get; set; }
        [DataMember(IsRequired = true)]
        public ResponseCode Reply { get; set; }
        [DataMember]
        public string Message { get; set; }

		public override string ToString()
		{
			return string.Format("[{2} {1}] {0}", Message, Reply, Number);
		}
    }
}