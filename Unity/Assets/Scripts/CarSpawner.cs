using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    
    public GameObject[] enemyCars;
    public float minPos = -2f;
    public float maxPos = 2f;
    public float delay = 1f;
    float timer;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0){
            Vector2 position = transform.position;
            position.x = Random.Range(minPos, maxPos);
            Instantiate(enemyCars[Random.Range(0, enemyCars.Length)], position, transform.rotation);
            timer = delay;
        }        
    }
}
