using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public const string COULD_NOT_READ = "socket.error.COULD_NOT_READ";
    public const string COULD_NOT_WRITE = "socket.error.COULD_NOT_WRITE";
    public const string SOCKET_CONNECTED = "socket.connected";
    public const string MODE_DISCOVERABLE = "bluetooth.mode.discoverable";
    public const string MODE_CONNECTABLE = "bluetooth.mode.connectable";
    public const string MODE_NONE = "bluetooth.mode.none";
    public const string ON = "bluetooth.on";
    public const string OFF = "bluetooth.off";
    protected string className = "com.guevara.bluetooth.BluetoothService";
    private const string btServiceClass = "com.guevara.bluetooth.BluetoothService";
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

    public string ServerObject {
        set {
            PluginInstance.Call("setServerObject", value);    
        }
        get {
            return PluginInstance.Call<string>("getServerObject");    
        }
    }

}   
