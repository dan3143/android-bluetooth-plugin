# Bluetooth for Unity
This is a plugin for Unity that enables Bluetooth communication for Android devices.
## How to use
Just import `android-bluetooth-plugin.unitypackage` into your project, and use the `BluetoothServer` and `BluetoothClient` classes at your convenience.

For example, if you want to start a server that sends "Hello" to any client that connects to it, you could attacth this to your script:

```c#
BluetoothServer server = new BluetoothServer();
server.Start();
server.ClientConnected += (sender, e) => {
  server.SendTo("Hello", e.Sender.address);
}
```

You can check the documentation in [the wiki](https://github.com/dan3143/android-bluetooth-plugin/wiki) for more information.
