using System;
using UnityEngine;
namespace UnityAndroidBluetooth {
    public class BluetoothServer : Bluetooth {

        /* ========== CONSTANTS ========== */
        private const string COULD_NOT_ACCEPT = "server.error.COULD_NOT_ACCEPT";
        private const string COULD_NOT_LISTEN = "server.error.COULD_NOT_LISTEN";
        private const string NOT_LISTENING = "server.not_listening";
        private const string LISTENING = "server.listening";
        private const string STOPPED = "server.stopped";

        /* ========== EVENT HANDLING ========== */
        public event EventHandler<ServerStateChangedEventArgs> ServerStateChanged;
        public event EventHandler<DeviceInfoEventArgs> ClientConnected;
        public event EventHandler<DeviceInfoEventArgs> ClientDisconnected;

        protected virtual void OnServerStateChanged(ServerStateChangedEventArgs e) 
        {
            ServerStateChanged?.Invoke(this, e);
        }

        protected virtual void OnClientConntected(DeviceInfoEventArgs e) {
            ClientConnected?.Invoke(this, e);
        }

        protected virtual void OnClientDisconnected(DeviceInfoEventArgs e) {
            ClientDisconnected?.Invoke(this, e);
        }

        // JNI Interface
        protected class OnAndroidServerStatus : Bluetooth.OnAndroidStatus {
            BluetoothServer server;
            public OnAndroidServerStatus(BluetoothServer s) : base(s) {
                server = s;
            }
            public override void OnStatus(string status){
                base.OnStatus(status);
                ServerStateChangedEventArgs e = new ServerStateChangedEventArgs();
                switch(status) {
                    case COULD_NOT_LISTEN:
                        throw new BluetoothException("Server could not start listening");
                    case LISTENING:
                        e.State = ServerState.LISTENING;
                        server.OnServerStateChanged(e);
                        break;
                    case NOT_LISTENING:
                        e.State = ServerState.NOT_LISTENING;
                        server.OnServerStateChanged(e);
                        break;
                    case STOPPED:
                        e.State = ServerState.STOPPED;
                        server.OnServerStateChanged(e);
                        break;
                }
                string[] tokens = status.Split('.');
                DeviceInfoEventArgs e2 = new DeviceInfoEventArgs();
                if (tokens[0] == "server" && tokens.Length >= 3) {
                    e2.Device = Bluetooth.GetDevice(tokens[2]);
                    if (tokens[1] == "connected") {    
                        server.OnClientConntected(e2);
                    } else if (tokens[1] == "disconnected") {
                        server.OnClientDisconnected(e2);
                    }
                }   
            }
        }

        public BluetoothServer() : base("com.guevara.bluetooth.BluetoothServer") {
            SetOnAndroidStatus(new OnAndroidServerStatus(this));
        }

        /* ========== JNI METHODS ========== */
        public void Start()
        {
            PluginInstance.Call("start");
        }

        public void Start(string name)
        {
            PluginInstance.Call("start", name);
        }

        public void Start(string name, string uuid)
        {
            PluginInstance.Call("start", name, uuid);
        }

        public void Stop()
        {
            PluginInstance.Call("stop");
        }

        public void SetMaxConnections(int value)
        {
            PluginInstance.Call("setMaxConnections", value);
        }

        public void SendToAll(string message)
        {
            PluginInstance.Call("sendAll", message);
        }

        public void SendTo(string address, string message)
        {
            PluginInstance.Call("sendTo", message, address);
        }
    }
}