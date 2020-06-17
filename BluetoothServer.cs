using UnityEngine;
public class BluetoothServer : Bluetooth {
    public const string COULD_NOT_LISTEN = "server.error.COULD_NOT_LISTEN";
    public const string COULD_NOT_CONNECT = "server.error.COULD_NOT_CONNECT";

    public void Start()
    {
        Debug.Log("Trying to start server...");
        base.PluginInstance.Call("startServer");
    }

    public void Start(string name)
    {
        base.PluginInstance.Call("startServer", name);
    }

    public void Start(string name, string uuid)
    {
        base.PluginInstance.Call("startServer", name, uuid);
    }

}