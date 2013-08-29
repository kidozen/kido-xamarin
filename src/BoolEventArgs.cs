using System;

namespace KidoZen
{
    public class BoolEventArgs : EventArgs
    {
        public BoolEventArgs(bool value, string message) { Result = value; Message = message; }
        public bool Result { get; private set; }
        public string Message { get; private set; }
    }
}
