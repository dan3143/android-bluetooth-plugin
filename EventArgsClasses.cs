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

    public class MessageReceivedEventArgs : EventArgs {
        public BluetoothDevice Sender { get; set; }
        public string Message { get; set; }
    }

    public class ServerStateChangedEventArgs: EventArgs {
        public ServerState State { get; set; } 
    }

    public class DeviceInfoEventArgs {
        public BluetoothDevice Device { get; set; }
    }

}