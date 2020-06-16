public class BluetoothServer: Bluetooth {
    public const string COULD_NOT_LISTEN = "server.error.COULD_NOT_LISTEN";
    public const string COULD_NOT_CONNECT = "server.error.COULD_NOT_CONNECT";

    public void Start()
    {
        PluginInstance.Call("startServer");
    }
}