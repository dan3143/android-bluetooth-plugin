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

    public void Start(){
        PluginInstance.Call("start");
    }

    public void Stop(){
        PluginInstance.Call("stop");
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
