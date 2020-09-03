using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityAndroidBluetooth {

    public class BluetoothControl : MonoBehaviour
    {
        private static BluetoothControl _instance;
        private BluetoothServer server;
        private List<ControlButton> buttons;

        public static BluetoothControl Instance {
            get { return _instance; }
        }

        public static BluetoothServer Server {
            get { 
                return _instance.server;
            }
        }
        
        public ControlButton GetButton(string name){
            foreach(ControlButton btn in buttons){
                if (btn.Name == name) return btn;
            }
            return null;
        }
        
        public void StartServer() {
            server.Start();
        }
        
        public void StopServer(){
            server.Stop();
        }

        void Awake()
        {
            if (Application.platform != RuntimePlatform.Android) return;
            
            if (_instance != null && _instance != this) {
                DestroyImmediate(this.gameObject);
            } else {
                _instance = this;
                _instance.server = new BluetoothServer();
                DontDestroyOnLoad(this.gameObject);
            }
        }

        void Start() 
        {
            Server.MessageReceived += MessageReceivedHandler;
        }

        void MessageReceivedHandler(object sender, MessageReceivedEventArgs e) {
            string[] message = e.Message.Split(':');
            BluetoothDevice from = e.Sender;
            foreach (ControlButton btn in buttons) {
                if (message[0] == btn.SymbolicName) {
                    if (message[1] == "1") {
                        btn.IsClicked = true;
                        btn.IsPressed = true;
                        StartCoroutine(ToggleClick(btn));
                        break;
                    } else {
                        btn.IsPressed = false;
                    }
                    if (btn is ControlJoystick) {
                        ControlJoystick joystick = btn as ControlJoystick;
                        joystick.DeltaX = double.Parse(message[2]);
                        joystick.DeltaY = double.Parse(message[3]);
                    } else if (btn is ControlTrigger) {
                        ControlTrigger trigger = btn as ControlTrigger;
                        trigger.Value = double.Parse(message[2]);
                    }
                }
            }
        }
        
        IEnumerator ToggleClick(ControlButton btn) {
            if (btn.IsClicked) {
                yield return null;
                btn.IsClicked = false;
            }
        }
    }

    public class ControlButton {
        public string Name { get; set; }
        public string SymbolicName { get; set; }
        public bool IsPressed { get; set; }
        public bool IsClicked { get; set; }

        public ControlButton(string name, string symbolicName) {
            Name = name;
            SymbolicName = symbolicName;
            IsPressed = false;
            IsClicked = false;
        }

        public ControlButton(string name) : this(name, name) { }

    }

    public class ControlJoystick : ControlButton
    {
        public ControlJoystick(string name) : this(name, name) { }

        public ControlJoystick(string name, string symbolicName) : base(name, symbolicName)
        {
            DeltaX = 0;
            DeltaY = 0;
        }

        public double DeltaX { get; set; }
        public double DeltaY { get; set; }
    }

    public class ControlTrigger : ControlButton
    {
        public ControlTrigger(string name) : this(name, name) {  }
        public ControlTrigger(string name, string symbolicName) : base(name, symbolicName)
        {
            Value = 0;
        }

        public double Value { get; set; }
    }
}
