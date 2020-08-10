using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace UnityAndroidBluetooth {
    
    public struct BluetoothDevice {
        public string name;
        public string address;
        public BluetoothDevice(string n, string a)
        {
            name = n;
            address = a;
        }
    }
    public abstract class Bluetooth {

        /* ========== CONSTANTS ========== */
        private const string COULD_NOT_READ = "socket.error.COULD_NOT_READ";
        private const string COULD_NOT_WRITE = "socket.error.COULD_NOT_WRITE";
        private const string MODE_DISCOVERABLE = "bluetooth.mode.discoverable";
        private const string MODE_CONNECTABLE = "bluetooth.mode.connectable";
        private const string MODE_NONE = "bluetooth.mode.none";
        private const string ON = "bluetooth.on";
        private const string OFF = "bluetooth.off";
        private const string INTERFACE_MESSAGE_NAME = "com.guevara.bluetooth.BluetoothService$OnBluetoothMessageListener";
        private const string INTERFACE_STATUS_NAME = "com.guevara.bluetooth.BluetoothService$OnBluetoothStatusListener";
        protected string className = "com.guevara.bluetooth.BluetoothService";
        private const string btServiceClass = "com.guevara.bluetooth.BluetoothService";
        public const string SERIAL_UUID = "00001101-0000-1000-8000-00805F9B34FB";

        /* ========== EVENT HANDLING ========== */

        // Events
        public event EventHandler<BluetoothStateChangedEventArgs> StateChanged;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        public event EventHandler<BluetoothModeChangedEventArgs> ModeChanged;
        
        protected virtual void OnModeChanged(BluetoothModeChangedEventArgs e) {
            ModeChanged?.Invoke(this, e);
        }

        protected virtual void OnStateChanged(BluetoothStateChangedEventArgs e) {
            StateChanged?.Invoke(this, e);
        }

        protected virtual void OnMessageReceived(MessageReceivedEventArgs e) {
            MessageReceived?.Invoke(this, e);
        }

        // JNI Interfaces
        protected class OnAndroidMessage : AndroidJavaProxy {
            protected Bluetooth bluetooth;
            public OnAndroidMessage(Bluetooth bt) : base(INTERFACE_MESSAGE_NAME) {
                bluetooth = bt;
            }
            public virtual void OnMessage (string message, string address) {
                MessageReceivedEventArgs e = new MessageReceivedEventArgs();
                e.Message = message;
                e.Sender = GetDevice(address);
                bluetooth.OnMessageReceived(e);
            }
        }

        protected class OnAndroidStatus : AndroidJavaProxy {
            Bluetooth bluetooth;
            public OnAndroidStatus(Bluetooth bt) : base(INTERFACE_STATUS_NAME) {
                bluetooth = bt;
            }

            public virtual void OnStatus(string status) {
                BluetoothModeChangedEventArgs e = new BluetoothModeChangedEventArgs();
                BluetoothStateChangedEventArgs e2 = new BluetoothStateChangedEventArgs();
                switch(status) {
                    case Bluetooth.COULD_NOT_READ:
                        throw new BluetoothException("Could not read from bluetooth socket");
                    case Bluetooth.COULD_NOT_WRITE:
                        throw new BluetoothException("Could not write into bluetooth socket");
                    case Bluetooth.MODE_CONNECTABLE:
                        e.Mode = BluetoothMode.CONNECTABLE;
                        bluetooth.OnModeChanged(e);
                        break;
                    case Bluetooth.MODE_DISCOVERABLE:
                        e.Mode = BluetoothMode.DISCOVERABLE;
                        bluetooth.OnModeChanged(e);
                        break;
                    case Bluetooth.MODE_NONE:
                        e.Mode = BluetoothMode.NONE;
                        bluetooth.OnModeChanged(e);
                        break;
                    case Bluetooth.ON:
                        e2.IsOn = true;
                        bluetooth.OnStateChanged(e2);
                        break;
                    case Bluetooth.OFF:
                        bluetooth.OnStateChanged(e2);
                        e2.IsOn = false;
                        break;
                }
            }
        }

    
        /* ========== JNI METHODS ========== */        
        private static AndroidJavaClass _serviceClass;
        private AndroidJavaClass _class;
        private AndroidJavaObject _instance;
        public List<BluetoothDevice> foundDevices;
        protected AndroidJavaClass PluginClass {
            get {
                if (_class == null){
                    _class = new AndroidJavaClass(className);
                }
                return _class;
            }
        }
        protected AndroidJavaObject PluginInstance {
            get {
                if (_instance == null){
                    Debug.Log(className);
                    _instance = new AndroidJavaObject(className);
                }
                return _instance;
            }
        }
        private static AndroidJavaClass ServiceClass {
            get {
                if (_serviceClass == null) {
                    _serviceClass = new AndroidJavaClass("com.guevara.bluetooth.BluetoothService");
                }
                return _serviceClass;
            }
        }
        public static bool IsEnabled {
            get {
                return ServiceClass.CallStatic<bool>("isEnabled");
            }
        }
        
        public Bluetooth(string classname)
        {
            this.className = classname;
            SetOnAndroidMessage(new OnAndroidMessage(this));
            SetOnAndroidStatus(new OnAndroidStatus(this));
        }
        
        public static List<BluetoothDevice> GetBondedDevices() {
            return GetDevices(0);
        }

        public static BluetoothDevice GetDevice(string address) 
        {
            return new BluetoothDevice(ServiceClass.CallStatic<string>("getDeviceName", address), address);    
        }

        public static List<BluetoothDevice> GetDiscoveredDevices() {
            return GetDevices(1);
        }

        private static List<BluetoothDevice> GetDevices(int type) {
            AndroidJavaObject array;
            if (type == 0) {
                array = ServiceClass.CallStatic<AndroidJavaObject>("u_getBondedDevices");
            } else {
                array = ServiceClass.CallStatic<AndroidJavaObject>("u_getDiscoveredDevices");
            }
            List<BluetoothDevice> bondedDevices = new List<BluetoothDevice>();
            if (array.GetRawObject().ToInt32() != 0) {
                string[] devices = AndroidJNIHelper.ConvertFromJNIArray<string[]>(array.GetRawObject());
                foreach(string device in devices) {
                    string[] tokens = device.Split(',');
                    BluetoothDevice dev;
                    dev.address = tokens[0];
                    dev.name = tokens[1];
                    bondedDevices.Add(dev);
                }
            }
            return bondedDevices;
        }

        
        public void RequestEnableBluetooth() {
            PluginInstance.Call("requestEnableBluetooth");
        }

        public void RequestEnableDiscoverability() {
            PluginInstance.Call("requestEnableDiscoverability");
        }
            
        public static void SearchDevices() 
        {
            ServiceClass.CallStatic("searchDevices");
        }

        protected void SetOnAndroidMessage(AndroidJavaProxy listener) {
            PluginInstance.Call("setOnMessageListener", listener);
        }

        protected void SetOnAndroidStatus(AndroidJavaProxy listener) {
            PluginInstance.Call("setOnStatusListener", listener);
        }

    }   
}
