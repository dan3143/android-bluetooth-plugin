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
    bool movingLeft;
    bool movingRight;

    void Start(){
        position = transform.position;
        Time.timeScale = 0;
    }

    void FixedUpdate()
    {
        if (movingLeft) {
            position.x -= speed;
        }
        if (movingRight) {
            position.x += speed;
        }
        if (position.x <= 2.5 && position.x >= -2.5){
            transform.position = position;
        }
    }

    void Message(string message)
    {
        Debug.Log("Message: " + message);
        if (message.Equals("a") || message.Equals("a_pressed")) {
            Time.timeScale = 1;
        }
        movingLeft = message.Equals("left_pressed");
        movingRight = message.Equals("right_pressed");
    }

    void OnCollisionEnter2D(Collision2D collision){
        if (collision.gameObject.tag.Equals("enemy")){
            ui.GameOver();
            Destroy(gameObject);
        }
    }
}
