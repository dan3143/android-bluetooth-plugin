using UnityEngine;
public class Bluetooth : MonoBehaviour {
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

    public string test(){
        return PluginInstance.Call<string>("test");
    }

    public void start(){
        PluginInstance.Call("start");
    }

    public void stop(){
        PluginInstance.Call("stop");
    }

    public bool isEnabled(){
        return PluginInstance.Call<bool>("isEnabled");
    }

    public void setGameObject(string name){
        PluginClass.CallStatic("setGameObject", name);
    }

    public string getGameObject(){
        return PluginClass.CallStatic<string>("getGameObject");
    }

    public void setServerObject(string name){
        PluginClass.CallStatic("setServerObject", name);
    }

    public string getServerObject(){
        return PluginClass.CallStatic<string>("getServerObject");
    }

}