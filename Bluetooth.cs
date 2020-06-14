using UnityEngine;


public class Bluetooth {
    const string pluginName = "com.example.bluetooth.BluetoothService";
    private static AndroidJavaClass _pluginClass;
    private static AndroidJavaObject _pluginInstance;
    
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
                _pluginInstance = PluginClass.CallStatic<AndroidJavaObject>("getInstance");
            }
            return _pluginInstance;
        }
    }

    public void Start() {
        PluginInstance.Call("startServer");
    }

    public void Start(string name) {
        PluginInstance.Call("startServer", name);
    }

    public void Start(string name, string uuid) {
        PluginInstance.Call("startServer", name, uuid);
    }

    public void Stop(){
        PluginInstance.Call("stopServer");
    }

    public void Write(string data) {
        PluginInstance.Call("write", data);
    }

    public bool Enable() {
        return PluginInstance.Call<bool>("enable");
    }

    public bool Disable() {
        return PluginInstance.Call<bool>("disable");
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

    public bool IsListening {
        get {
            return PluginInstance.Call<bool>("isListening");
        }
    }

    public string PlayerObject {
        set {
            PluginClass.CallStatic("setGameObject", value);    
        }
        get {
            return PluginClass.CallStatic<string>("getGameObject");    
        }
    }
    public string ServerObject {
        set {
            PluginClass.CallStatic("setServerObject", value);    
        }
        get {
            return PluginClass.CallStatic<string>("getServerObject");    
        }
    }

}   
