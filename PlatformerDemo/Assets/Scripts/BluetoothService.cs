using UnityEngine;
using TMPro;

public class BluetoothService : MonoBehaviour
{
    public Bluetooth bluetooth;
    bool isConected = false;
    string device = "";
    string message = "";

    public void toggleBluetooth() 
    {
        if (bluetooth.IsEnabled) {
            bluetooth.Stop();
            bluetooth.Disable();
        } else {
            bluetooth.Enable();
        }
    }

    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Bluetooth");
        if (objs.Length > 1) {
            GameObject.Find("Canvas/SettingsMenu").SetActive(false);
            DestroyImmediate(this.gameObject);
        } else {
            bluetooth = new Bluetooth();
            bluetooth.ServerObject = "BluetoothService";
            DontDestroyOnLoad(this.gameObject);
        }
    }

    void Start()
    {
        GameObject gm = GameObject.Find("Canvas/SettingsMenu/StartServer/Text");
        #if UNITY_ANDROID
        Debug.Log("ServerObject: " + bluetooth.ServerObject);
        if (gm != null)
        {
            TextMeshProUGUI text = gm.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                if (bluetooth.IsEnabled) {
                    Message("bluetooth.on");
                } else if (isConected) {
                    text.text = "CONNECTED TO " + device;
                }
                
            }
        }
        #endif
        GameObject.Find("Canvas/SettingsMenu").SetActive(false);
    }

    void Message(string message)
    {
        GameObject gm = GameObject.Find("Canvas/SettingsMenu/StartServer/Text");
        if (gm != null)
        {
            TextMeshProUGUI text = gm.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                switch (message) {
                    case "bluetooth.on":
                        text.text = "CONNECT THE CONTROLLER...";
                        bluetooth.Start();
                        break;
                    case "bluetooth.off":
                        text.text = "START BLUETOOTH";
                        isConected = false;
                        device = "";
                        break;
                    case "bluetooth.disconnected":
                        text.text = "CONNECT THE CONTROLLER...";
                        isConected = false;
                        device = "";
                        bluetooth.Stop();
                        bluetooth.Start();
                        break;
                }
                string[] tokens = message.Split('.');
                if (tokens[1] == "connected") {
                    device = tokens[2];
                    text.text = "CONNECTED TO " + device;
                    isConected = true;
                }
            }
        }
        Debug.Log("Server said: " + message);
    }
}