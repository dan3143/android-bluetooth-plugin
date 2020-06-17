using UnityEngine;
public class BluetoothClient: Bluetooth {
    public const string COULD_NOT_CREATE_SOCKET = "client.error.COULD_NOT_CREATE_SOCKET";
    public const string COULD_NOT_CONNECT = "client.error.COULD_NOT_CONNECT";

    public void Connect(string address)
    {
        base.PluginInstance.Call("u_connect", address);
    }

    public void Connect(string address, string uuid) 
    {
        base.PluginInstance.Call("u_connect", address, uuid);
    }
}