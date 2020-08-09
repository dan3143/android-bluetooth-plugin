using System;
using UnityEngine;
namespace UnityAndroidBluetooth {
    public class BluetoothServer : Bluetooth {

        /* ========== CONSTANTS ========== */
        public const string COULD_NOT_ACCEPT = "server.error.COULD_NOT_ACCEPT";
        public const string COULD_NOT_LISTEN = "server.error.COULD_NOT_LISTEN";
        public const string NOT_LISTENING = "server.not_listening";
        public const string LISTENING = "server.listening";
        public const string STOPPED = "server.stopped";

        /* ========== EVENT HANDLING ========== */
        public event EventHandler<ServerStateChangedEventArgs> ServerStateChanged;
        public event EventHandler<DeviceStatusChangedEventArgs> DeviceStatusChanged;

        protected virtual void OnServerStateChanged(ServerStateChangedEventArgs args) 
        {
            ServerStateChanged?.Invoke(this, args);
        }

        protected virtual void OnDeviceStatusChanged(DeviceStatusChangedEventArgs args) {
            DeviceStatusChanged?.Invoke(this, args);
        }

        // JNI Interface
        protected class OnAndroidServerStatus : Bluetooth.OnAndroidStatus {
            BluetoothServer server;
            public OnAndroidServerStatus(BluetoothServer s) : base(s) {
                server = s;
            }
            public override void OnStatus(string status){
                base.OnStatus(status);
                ServerStateChangedEventArgs args = new ServerStateChangedEventArgs();
                switch(status) {
                    case COULD_NOT_LISTEN:
                        throw new ServerException("Could not start listening");
                    case LISTENING:
                        args.State = ServerState.LISTENING;
                        server.OnServerStateChanged(args);
                        break;
                    case NOT_LISTENING:
                        args.State = ServerState.NOT_LISTENING;
                        server.OnServerStateChanged(args);
                        break;
                    case STOPPED:
                        args.State = ServerState.STOPPED;
                        server.OnServerStateChanged(args);
                        break;
                }
                string[] tokens = status.Split('.');
                DeviceStatusChangedEventArgs devArgs = new DeviceStatusChangedEventArgs();
                if (tokens[0] == "server" && tokens.Length >= 3) {
                    devArgs.address = tokens[2];
                    if (tokens[1] == "connected") {    
                        devArgs.IsConnected = true;
                        server.OnDeviceStatusChanged(devArgs);
                    } else if (tokens[1] == "disconnected") {
                        devArgs.IsConnected = false;
                        server.OnDeviceStatusChanged(devArgs);
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

        public void SendTo(string message, string address)
        {
            PluginInstance.Call("sendTo", message, address);
        }
    }
}