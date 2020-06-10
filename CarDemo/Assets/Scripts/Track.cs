using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track : MonoBehaviour
{
    public float speed;
    Vector2 offset;
    Renderer mRenderer;

    void Start()
    {
        mRenderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        offset = new Vector2(0, Time.time * speed);
        mRenderer.material.mainTextureOffset = offset;
        
    }
}
