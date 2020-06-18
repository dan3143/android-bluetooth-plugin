using UnityEngine;
public class BluetoothServer : Bluetooth {
    public const string COULD_NOT_LISTEN = "server.error.COULD_NOT_LISTEN";
    public const string COULD_NOT_CONNECT = "server.error.COULD_NOT_CONNECT";
    public const string NOT_LISTENING = "server.not_listening";

    public BluetoothServer() {
        base.className = "com.guevara.bluetooth.BluetoothServer";
    }

    public void Start()
    {
        base.PluginInstance.Call("start");
    }

    public void Start(string name)
    {
        base.PluginInstance.Call("start", name);
    }

    public void Start(string name, string uuid)
    {
        base.PluginInstance.Call("start", name, uuid);
    }

    public void Stop()
    {
        base.PluginInstance.Call("stop");
    }

    public void SetMaxConnections(int value)
    {
        base.PluginInstance.Call("setMaxConnections", value);
    }

    public void SendToAll(string message)
    {
        base.PluginInstance.Call("sendAll", message);
    }

    public void SendTo(string message, string address)
    {
        base.PluginInstance.Call("sendTo", message, address);
    }
}