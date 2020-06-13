# Bluetooth for Unity
This is a plugin for Unity that enables Bluetooth communication for Android devices.
## How to use
Just import `android-bluetooth-plugin.unitypackage` into your project, then create a GameObject and attach `BluetoothService.cs` onto it.

With `BluetoothService.Bluetooth.PlayerObject = "gameobject"`, *gameobject* will receive data through Bluetooth, and you can access this data by creating a method called Message, like this:

    void Message(string data) {
	    DoSomething(data);
    }

If you want to receive information about the state of the Bluetooth connection, you have to do the same thing, but this time setting `BluetoothService.Bluetooth.ServerObject = "gameobject"`. The messages that this *gameobject* can receive are:
| Message | Meaning |
|--|--|
| bluetooth.on | The Bluetooth adapter was enabled |
| bluetooth.off | The Bluetooth adapter was disabled |
| bluetooth.connected | A device connected |
| bluetooth.disconnected | A device disconnected |
| server.listening | The Bluetooth server is listening for incoming connections|
| server.stopped | The Bluetooth server stopped listening for incoming connections |
| server.connected.*device* | The device *device* connected |

Additional to this, you can check if Bluetooth is enabled in the device with the property `BluetoothService.Bluetooth.IsEnabled`.

For now, this plugin only allows receiving data, but I plan to make it able to send data in the future.
