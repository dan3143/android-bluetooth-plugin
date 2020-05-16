using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public float speed;
    Vector2 position;
    Bluetooth bt;
    public UIManager ui;

    void Start(){
        position = transform.position;
        Time.timeScale = 0;
    }

    void Update()
    {

    }

    void Message(string message)
    {
        Debug.Log("Message: " + message);
        float movement = 0;
        switch(message) {
            case "left":
                movement = position.x - speed;
                break;
            case "right":
                movement = position.x + speed;
                break;
            case "a":
                if (Time.timeScale == 0) {
                    Time.timeScale = 1;
                }
                break;
            default:
                movement = position.x;
                break;
        }
        if (movement <= 2.5 && movement >= -2.5){
            position.x = movement;
            transform.position = position;
        }
        
    }

    void OnCollisionEnter2D(Collision2D collision){
        if (collision.gameObject.tag.Equals("enemy")){
            ui.GameOver();
            Destroy(gameObject);
        }
    }
}
