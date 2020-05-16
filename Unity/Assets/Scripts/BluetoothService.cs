using UnityEngine;

public class BluetoothService : MonoBehaviour
{
    static Bluetooth bluetooth = null;

    void Awake()
    {
        Debug.Log("Bluetooth: " + bluetooth);
        GameObject[] objs = GameObject.FindGameObjectsWithTag("bluetooth");
        if (objs.Length > 1) {
            DestroyImmediate(this.gameObject);
        }else{
            bluetooth = new Bluetooth();
            if (bluetooth.isEnabled()) {
                bluetooth.setServerObject("server");
                bluetooth.setGameObject("car");
                bluetooth.start();
            }
            DontDestroyOnLoad(this.gameObject);
        }
    }
    void Start()
    {
        
    }

    void Message(string message){
        Debug.Log("Server: " + message);
        if (message.Equals("socket_disconnected")){
            bluetooth.stop();
            bluetooth.start();
        }
    }

}
