using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public PlayerController controller;
    public Animator animator;
    public float speed = 10f;

    float xMovement = 0;
    bool jump = false;
    bool crouch = false;
    
    void Awake()
    {
        #if UNITY_ANDROID
        Bluetooth bt = GameObject.Find("BluetoothService").GetComponent<BluetoothService>().bluetooth;
        bt.PlayerObject = "Player";
        Debug.Log("PlayerObject: " + bt.PlayerObject);
        #endif
    }

    void FixedUpdate()
    {
        #if UNITY_EDITOR
        xMovement = Input.GetAxisRaw("Horizontal")*speed;
        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
            animator.SetBool("isJumping", true);
        }
        if (Input.GetButtonDown("Crouch"))
        {
            crouch = true;
        }
        #endif
        
        animator.SetFloat("speed", Mathf.Abs(xMovement));
        controller.Move(xMovement*Time.fixedDeltaTime, crouch, jump);
        jump = false;   
    }

    public void OnLanding()
    {
        animator.SetBool("isJumping", false);
    }

    public void OnCrouching(bool isCrouching)
    {
        animator.SetBool("isCrouching", isCrouching);
    }

    void Message(string message)
    {
        switch(message){
            case "left_pressed":
                xMovement = -speed;
                break;
            case "right_pressed":
                xMovement = speed;
                break;
            case "space_pressed":
                jump = true;
                animator.SetBool("isJumping", true);
                break;
            case "up_pressed":
                crouch = false;
                break;
            case "down_pressed":
                crouch = true;
                break;
            default:
                xMovement = 0;
                jump = false;
                break;
        }
        Debug.Log("Player recieved: " + message);
    }
}
