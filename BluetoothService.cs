using System.Collections;
using System.Threading.Tasks;
using UnityEngine;


public class BluetoothService : MonoBehaviour
{
    [System.Serializable]
    public class BluetoothButton {
        public string name;
        public string key_name;
        public string pressed_name;
        public string released_name;
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

    private static BluetoothService _instance;
    private const int RELEASED = 0;
    private const int PRESSED = 1;
    private const int CLICKED = 2;
    private Bluetooth bluetooth;
    [SerializeField] private BluetoothButton[] buttons;
    private static Bluetooth tmpBt;

    public static BluetoothService Instance {
        get { return _instance; }
    }

    public static Bluetooth Bluetooth {
        get { 
            if (tmpBt == null) {
                tmpBt = _instance.GetComponent<BluetoothService>().bluetooth;
            }
            return tmpBt;
         }
    }

    void Awake()
    {
        if (Application.platform != RuntimePlatform.Android) return;
        
        if (_instance != null && _instance != this) {
            DestroyImmediate(this.gameObject);
        } else {
            _instance = this;
            _instance.bluetooth = new Bluetooth();
            if (_instance.bluetooth.IsEnabled) {
                _instance.bluetooth.Start();
                Debug.Log("[BluetoothService]Bluetooth server started");
            }
            _instance.bluetooth.PlayerObject = "BluetoothService";
            DontDestroyOnLoad(this.gameObject);
        }
    }

    void Start() 
    {
        
    }

    private bool GetButtonStatus(string name, int status) 
    {
        if (!bluetooth.IsEnabled) {
            Debug.LogError("Bluetooth is off");
            return false;
        }

        bool found = false;
        foreach (BluetoothButton btn in buttons) {
            if (btn.name == name) {
                found = true;
                switch (status) {
                    case PRESSED:
                        return btn.IsPressed;
                    case RELEASED:
                        return btn.IsReleased;
                    case CLICKED:
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
        return GetButtonStatus(btn, PRESSED);
    }

    public bool IsButtonReleased(string btn) {
        return GetButtonStatus(btn, RELEASED);
    }

    public bool IsButtonClicked(string btn) {
        return GetButtonStatus(btn, CLICKED);
    }

    IEnumerator ToggleClick(BluetoothButton btn) {
        if (btn.IsClicked) {
            yield return null;
            btn.IsClicked = false;
        }
    }

    void Message(string message) {
        Debug.Log("Message:" + message);
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
}
