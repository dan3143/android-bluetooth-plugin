
namespace UnityAndroidBluetooth {
    [System.Serializable]
    public class CouldNotReadException : System.Exception
    {
        public CouldNotReadException() { }
        public CouldNotReadException(string message) : base(message) { }
        public CouldNotReadException(string message, System.Exception inner) : base(message, inner) { }
        protected CouldNotReadException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }   
    
    [System.Serializable]
    public class CouldNotWriteException : System.Exception
    {
        public CouldNotWriteException() { }
        public CouldNotWriteException(string message) : base(message) { }
        public CouldNotWriteException(string message, System.Exception inner) : base(message, inner) { }
        protected CouldNotWriteException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [System.Serializable]
    public class ClientException : System.Exception
    {
        public ClientException() { }
        public ClientException(string message) : base(message) { }
        public ClientException(string message, System.Exception inner) : base(message, inner) { }
        protected ClientException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [System.Serializable]
    public class ServerException : System.Exception
    {
        public ServerException() { }
        public ServerException(string message) : base(message) { }
        public ServerException(string message, System.Exception inner) : base(message, inner) { }
        protected ServerException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}