using UnityEngine;

public class BluetoothService : MonoBehaviour
{
    static Bluetooth bluetooth = null;

    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("bluetooth");
        if (objs.Length > 1) {
            DestroyImmediate(this.gameObject);
        }else{
            bluetooth = new Bluetooth();
            if (bluetooth.IsEnabled) {
                bluetooth.ServerObject = "BluetoothService";
                bluetooth.PlayerObject = "car";
                bluetooth.start();
            }
            DontDestroyOnLoad(this.gameObject);
        }
    }
    void Start()
    {
        
    }

    void Message(string message){
        
    }

}
