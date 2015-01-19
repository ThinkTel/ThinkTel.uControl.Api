using System;

namespace ThinkTel.uControl.Api
{
	public class ResponseException : Exception
	{
		public ResponseException(string msg) : base(msg) { }
	}
}