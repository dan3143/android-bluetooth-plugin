using System;
namespace UnityAndroidBluetooth {
    public enum BluetoothMode {
        CONNECTABLE,
        DISCOVERABLE,
        NONE
    }

     public enum ServerState {
        STOPPED,
        LISTENING,
        NOT_LISTENING
    }

    public class BluetoothModeChangedEventArgs : EventArgs {
        public BluetoothMode Mode { get; set; }
    }

    public class BluetoothStateChangedEventArgs : EventArgs {
        public bool IsOn { get; set; }
    }

    public class BluetoothSocketConnectedEventArgs : EventArgs {
        public string Device { get; set; }
    }

    public class MessageReceivedEventArgs : EventArgs {
        public string SenderAddress { get; set; }
        public string Message { get; set; }
    }

    public class ClientConnectedEventArgs : EventArgs {
        public string ServerAddress { get; set; }
    }

    public class ServerStateChangedEventArgs: EventArgs {
        public ServerState State { get; set; } 
    }

    public class DeviceStatusChangedEventArgs : EventArgs {
        public string address { get; set; }
        public bool IsConnected { get; set; }
    }

}