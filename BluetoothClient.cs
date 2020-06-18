using UnityEngine;
public class BluetoothClient: Bluetooth {
    public const string COULD_NOT_CREATE_SOCKET = "client.error.COULD_NOT_CREATE_SOCKET";
    public const string COULD_NOT_CONNECT = "client.error.COULD_NOT_CONNECT";
    public const string CONNECTION_LOST = "client.connection_lost";

    public BluetoothClient() {
        base.className = "com.guevara.bluetooth.BluetoothClient";
    }

    public void Send(string message)
    {
        PluginInstance.Call("send", message);
    }

    public void Connect(string address, string uuid)
    {
        PluginInstance.Call("connect", address, uuid);
    }

    public void Connect(string address)
    {
        PluginInstance.Call("connect", address);
    }

    public void Disconnect()
    {
        PluginInstance.Call("disconnect");
    }
}