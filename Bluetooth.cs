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
    public const string MODE_DISCOVERABLE = "bluetooth.mode.discoverable";
    public const string MODE_CONNECTABLE = "bluetooth.mode.connectable";
    public const string MODE_NONE = "bluetooth.mode.none";
    public const string ON = "bluetooth.on";
    public const string OFF = "bluetooth.off";

    const string pluginName = "com.example.bluetooth.BluetoothService";
    protected static AndroidJavaClass _pluginClass;
    protected AndroidJavaObject _pluginInstance;
    public static List<BluetoothDevice> foundDevices;
    
    public static AndroidJavaClass PluginClass {
        get {
            if (_pluginClass == null){
                _pluginClass = new AndroidJavaClass(pluginName);
            }
            return _pluginClass;
        }
    }

    public AndroidJavaObject PluginInstance {
        get {
            if (_pluginInstance == null){
                AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
                _pluginInstance = PluginClass.CallStatic<AndroidJavaObject>("createInstance", activity);
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

    public static void requestEnableBluetooth() {
        PluginClass.CallStatic("requestEnableBluetooth");
    }

    public static void requestEnableDiscoverability() {
        PluginClass.CallStatic("requestEnableDiscoverability");
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

    public bool IsBluetoothEnabled {
        get {
            return PluginClass.CallStatic<bool>("isEnabled");
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
