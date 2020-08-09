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
        public const string COULD_NOT_READ = "socket.error.COULD_NOT_READ";
        public const string COULD_NOT_WRITE = "socket.error.COULD_NOT_WRITE";
        public const string MODE_DISCOVERABLE = "bluetooth.mode.discoverable";
        public const string MODE_CONNECTABLE = "bluetooth.mode.connectable";
        public const string MODE_NONE = "bluetooth.mode.none";
        public const string ON = "bluetooth.on";
        public const string OFF = "bluetooth.off";
        public const string INTERFACE_MESSAGE_NAME = "com.guevara.bluetooth.BluetoothService$OnBluetoothMessageListener";
        public const string INTERFACE_STATUS_NAME = "com.guevara.bluetooth.BluetoothService$OnBluetoothStatusListener";
        protected string className = "com.guevara.bluetooth.BluetoothService";
        private const string btServiceClass = "com.guevara.bluetooth.BluetoothService";

        /* ========== EVENT HANDLING ========== */

        // Events
        public event EventHandler<BluetoothModeChangedEventArgs> ModeChanged;
        public event EventHandler<BluetoothStateChangedEventArgs> StateChanged;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
        
        protected virtual void OnModeChanged(BluetoothModeChangedEventArgs e)
        {
            var handler = ModeChanged;
            handler?.Invoke(this, e);
        }

        protected virtual void OnStateChanged(BluetoothStateChangedEventArgs e) {
            var handler = StateChanged;
            handler?.Invoke(this, e);
        }

        protected virtual void OnMessageReceived(MessageReceivedEventArgs e) {
            var handler = MessageReceived;
            handler?.Invoke(this, e);
        }

        // JNI Interface
        protected class OnAndroidMessage : AndroidJavaProxy {
            protected Bluetooth bluetooth;
            public OnAndroidMessage(Bluetooth bt) : base(INTERFACE_MESSAGE_NAME) {
                bluetooth = bt;
            }
            public virtual void OnMessage (string message, string address) {
                MessageReceivedEventArgs e = new MessageReceivedEventArgs();
                e.Message = message;
                e.SenderAddress = address;
                bluetooth.OnMessageReceived(e);
            }
        }

        protected class OnAndroidStatus : AndroidJavaProxy {
            Bluetooth bluetooth;
            public OnAndroidStatus(Bluetooth bt) : base(INTERFACE_STATUS_NAME) {
                bluetooth = bt;
            }

            public virtual void OnStatus(string status) {
                BluetoothModeChangedEventArgs modeArgs = new BluetoothModeChangedEventArgs();
                BluetoothStateChangedEventArgs stateArgs = new BluetoothStateChangedEventArgs();
                switch(status) {
                    case Bluetooth.COULD_NOT_READ:
                        throw new CouldNotReadException("Could not read from bluetooth socket");
                    case Bluetooth.COULD_NOT_WRITE:
                        throw new CouldNotWriteException("Could not write into bluetooth socket");
                    case Bluetooth.MODE_CONNECTABLE:
                        modeArgs.Mode = BluetoothMode.CONNECTABLE;
                        bluetooth.OnModeChanged(modeArgs);
                        break;
                    case Bluetooth.MODE_DISCOVERABLE:
                        modeArgs.Mode = BluetoothMode.DISCOVERABLE;
                        bluetooth.OnModeChanged(modeArgs);
                        break;
                    case Bluetooth.MODE_NONE:
                        modeArgs.Mode = BluetoothMode.NONE;
                        bluetooth.OnModeChanged(modeArgs);
                        break;
                    case Bluetooth.ON:
                        stateArgs.IsOn = true;
                        bluetooth.OnStateChanged(stateArgs);
                        break;
                    case Bluetooth.OFF:
                        bluetooth.OnStateChanged(stateArgs);
                        stateArgs.IsOn = false;
                        break;
                }
            }
        }

    
        /* ========== JNI METHODS ========== */        
        private static AndroidJavaClass _serviceClass;
        private AndroidJavaClass _class;
        private AndroidJavaObject _instance;
        
        public List<BluetoothDevice> foundDevices;

        public Bluetooth(string classname)
        {
            this.className = classname;
            SetOnAndroidMessage(new OnAndroidMessage(this));
            SetOnAndroidStatus(new OnAndroidStatus(this));
        }
        
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

        public static void SearchDevices() 
        {
            ServiceClass.CallStatic("searchDevices");
        }

        public static string GetSerialUUID() 
        {
            return ServiceClass.CallStatic<string>("getSerialUUID");
        }

        public static BluetoothDevice GetDevice(string address) 
        {
            return new BluetoothDevice(ServiceClass.CallStatic<string>("getDeviceName", address), address);    
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

        public static List<BluetoothDevice> GetBondedDevices() {
            return GetDevices(0);
        }

        public static List<BluetoothDevice> GetDiscoveredDevices() {
            return GetDevices(1);
        }

        public static bool IsEnabled {
            get {
                return ServiceClass.CallStatic<bool>("isEnabled");
            }
        }

        // Message received from Plugin
        protected void SetOnAndroidMessage(AndroidJavaProxy listener) {
            PluginInstance.Call("setOnMessageListener", listener);
        }

        // Status received from Plugin
        protected void SetOnAndroidStatus(AndroidJavaProxy listener) {
            PluginInstance.Call("setOnStatusListener", listener);
        }

        public void RequestEnableBluetooth() {
            PluginInstance.Call("requestEnableBluetooth");
        }

        public void RequestEnableDiscoverability() {
            PluginInstance.Call("requestEnableDiscoverability");
        }

        public string PlayerObject {
            set {
                PluginInstance.Call("setGameObject", value);    
            }
            get {
                return PluginInstance.Call<string>("getGameObject");    
            }
        }
    }   

}
