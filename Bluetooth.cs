using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct BluetoothDevice {
    public string name;
    public string address;
}

public abstract class Bluetooth {
    public const string COULD_NOT_READ = "socket.error.COULD_NOT_READ";
    public const string COULD_NOT_WRITE = "socket.error.COULD_NOT_WRITE";
    public const string SOCKET_CONNECTED = "socket.connected";
    public const string ON = "bluetooth.on";
    public const string OFF = "bluetooth.off";

    const string pluginName = "com.example.bluetooth.BluetoothService";
    private static AndroidJavaClass _pluginClass;
    private static AndroidJavaObject _pluginInstance;
    public static List<BluetoothDevice> foundDevices;
    
    
    public static AndroidJavaClass PluginClass {
        get {
            if (_pluginClass == null){
                _pluginClass = new AndroidJavaClass(pluginName);
            }
            return _pluginClass;
        }
    }

    public static AndroidJavaObject PluginInstance {
        get {
            if (_pluginInstance == null){
                _pluginInstance = PluginClass.CallStatic<AndroidJavaObject>("createInstance");
            }
            return _pluginInstance;
        }
    }

    public void Stop(){
        PluginInstance.Call("stop");
    }

    public void Write(string data) {
        PluginInstance.Call("write", data);
    }

    public static bool Enable() {
        return PluginClass.CallStatic<bool>("enableAdapter");
    }

    public static bool Disable() {
        return PluginClass.CallStatic<bool>("disableAdapter");
    }

    public static void SearchDevices() {
        PluginClass.CallStatic("searchDevices");
    }

    private static List<BluetoothDevice> getDevices(int type) {
        AndroidJavaObject array;
        if (type == 0) {
            array = PluginClass.CallStatic<AndroidJavaObject>("u_getBondedDevices");
        } else {
            array = PluginClass.CallStatic<AndroidJavaObject>("u_getDiscoveredDevices");
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

    public static List<BluetoothDevice> getBondedDevices() {
        return getDevices(0);
    }

    public static List<BluetoothDevice> getDiscoveredDevices() {
        return getDevices(1);
    }

    public bool IsEnabled {
        get {
            return PluginInstance.Call<bool>("isEnabled");
        }
    }

    public bool IsConnected {
        get {
            return PluginInstance.Call<bool>("isConnected");
        }
    }

    public string PlayerObject {
        set {
            PluginInstance.Call("setGameObject", value);    
        }
        get {
            return PluginInstance.Call<string>("getGameObject");    
        }
    }

    public string DefaultUUID {
        get { return PluginClass.CallStatic<string>("getSerialUUID"); }
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
