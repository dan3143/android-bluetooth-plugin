
namespace UnityAndroidBluetooth {
    
    [System.Serializable]
    public class BluetoothException : System.Exception
    {
        public BluetoothException() { }
        public BluetoothException(string message) : base(message) { }
        public BluetoothException(string message, System.Exception inner) : base(message, inner) { }
        protected BluetoothException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}