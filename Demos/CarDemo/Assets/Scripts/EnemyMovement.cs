using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    
    public float speed = 0.1f;

    void Start()
    {
        
    }

    void FixedUpdate(){
        transform.Translate(new Vector3(0, -1, 0) * speed);
    }
}
