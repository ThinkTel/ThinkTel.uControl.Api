using System;

namespace ThinkTel.uControl.Api
{
    public class OperationException : Exception
    {
        public OperationException(object source) : base(source.ToString()) {}
    }
}