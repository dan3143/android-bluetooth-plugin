using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityAndroidBluetooth {
    public class BluetoothControl : MonoBehaviour
    {
        [System.Serializable]
        public class BluetoothButton {
            public enum ButtonType {
                BUTTON,
                JOYSTICK
            }
            public string name;
            public string key_name;
            public string pressed_name;
            public string released_name;
            public ButtonType type;
            private bool isPressed = false;
            private bool isReleased = false;
            private bool isClicked = false;
            public bool IsPressed {
                get { return isPressed; }
                set { isPressed = value; }
            }
            public bool IsReleased {
                get { return isReleased; }
                set { isReleased = value; }
            }
            public bool IsClicked {
                get { return isClicked; }
                set { isClicked = value; }
            }
        }

        private static BluetoothControl _instance;
        private const int STATUS_RELEASED = 0;
        private const int STATUS_PRESSED = 1;
        private const int STATUS_CLICKED = 2;
        private BluetoothServer server;
        [SerializeField] private BluetoothButton[] buttons;

        public static BluetoothControl Instance {
            get { return _instance; }
        }

        public static BluetoothServer Server {
            get { 
                return _instance.server;
            }
        }

        void Awake()
        {
            if (Application.platform != RuntimePlatform.Android) return;
            
            if (_instance != null && _instance != this) {
                DestroyImmediate(this.gameObject);
            } else {
                _instance = this;
                _instance.server = new BluetoothServer();
                if (Bluetooth.IsEnabled) {
                    _instance.server.Start();
                }
                DontDestroyOnLoad(this.gameObject);
            }
        }

        void Start() 
        {
            Server.MessageReceived += MessageReceivedHandler;
        }

        void MessageReceivedHandler(object sender, MessageReceivedEventArgs e) {
            string message = e.Message;
            BluetoothDevice from = e.Sender;
            foreach (BluetoothButton btn in buttons) {
                if (message == btn.pressed_name) {
                    btn.IsPressed = true;
                    break;
                } else {
                    btn.IsPressed = false;
                }
                if (message == btn.released_name) {
                    btn.IsReleased = true;
                    break;
                } else {
                    btn.IsReleased = false;
                }

                if (message == btn.key_name) {
                    btn.IsClicked = true;
                    StartCoroutine(ToggleClick(btn));
                    break;
                } else {
                    btn.IsClicked = false;
                }
            }
        }

        private bool GetButtonStatus(string name, int status) 
        {
            if (!Bluetooth.IsEnabled) {
                return false;
            }

            bool found = false;
            foreach (BluetoothButton btn in buttons) {
                if (btn.name == name) {
                    found = true;
                    switch (status) {
                        case STATUS_PRESSED:
                            return btn.IsPressed;
                        case STATUS_RELEASED:
                            return btn.IsReleased;
                        case STATUS_CLICKED:
                            return btn.IsClicked;
                    }
                }
            }
            if (!found) {
                Debug.LogError("Button " + name + " not found");
            }
            return false;
        }

        public bool IsButtonPressed(string btn) {
            return GetButtonStatus(btn, STATUS_PRESSED);
        }

        public bool IsButtonReleased(string btn) {
            return GetButtonStatus(btn, STATUS_RELEASED);
        }

        public bool IsButtonClicked(string btn) {
            return GetButtonStatus(btn, STATUS_CLICKED);
        }

        IEnumerator ToggleClick(BluetoothButton btn) {
            if (btn.IsClicked) {
                yield return null;
                btn.IsClicked = false;
            }
        }
    }
}