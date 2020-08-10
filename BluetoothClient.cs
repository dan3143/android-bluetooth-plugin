using System;
using UnityEngine;

namespace UnityAndroidBluetooth{
    public class BluetoothClient: Bluetooth {

        /* ========== CONSTANTS ========== */
        private const string COULD_NOT_CREATE_SOCKET = "client.error.COULD_NOT_CREATE_SOCKET";
        private const string COULD_NOT_CONNECT = "client.error.COULD_NOT_CONNECT";
        private const string CONNECTION_LOST = "client.connection_lost";
        private const string DISCONNECTED = "client.disconnected";

        /* ========== EVENT HANDLING ========== */

        public event EventHandler ConnectionLost;
        public event EventHandler Disconnected;
        public event EventHandler<DeviceInfoEventArgs> Connected;

        protected virtual void OnConnectionLost()
        {
            ConnectionLost?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnConnected(DeviceInfoEventArgs e)
        {
            Connected?.Invoke(this, e);
        }

        // JNI Interface
        protected class OnAndroidClientStatus : Bluetooth.OnAndroidStatus {
            BluetoothClient client;
            public OnAndroidClientStatus(BluetoothClient c) : base(c) { 
                client = c;
            }

            public override void OnStatus(string message){
                base.OnStatus(message);
                switch(message) {
                    case BluetoothClient.COULD_NOT_CREATE_SOCKET:
                        throw new BluetoothException("The client could not create a socket");
                    case BluetoothClient.COULD_NOT_CONNECT:
                        throw new BluetoothException("The client could not connect to the server");
                    case BluetoothClient.CONNECTION_LOST:
                        client.OnConnectionLost();
                        break;
                    case BluetoothClient.DISCONNECTED:
                        client.OnDisconnected();
                        break;
                }

                string[] tokens = message.Split('.');
                if (tokens[0] == "client" && tokens[1] == "connected")
                {
                    DeviceInfoEventArgs e = new DeviceInfoEventArgs();
                    e.Device = Bluetooth.GetDevice(tokens[2]);
                    client.OnConnected(e);
                }

            }
        }

        public BluetoothClient() : base("com.guevara.bluetooth.BluetoothClient")
        {
            SetOnAndroidStatus(new OnAndroidClientStatus(this));
        }

        /* ========== JNI METHODS ========== */

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

        public void Send(string message)
        {
            PluginInstance.Call("send", message);
        }
    }
}