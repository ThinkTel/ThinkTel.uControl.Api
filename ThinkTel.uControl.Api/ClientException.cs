using System;

namespace ThinkTel.uControl.Api
{
    public class ClientException : Exception
    {
        public ClientException(string message, Exception innerException) : base(message, innerException) { }
    }
}