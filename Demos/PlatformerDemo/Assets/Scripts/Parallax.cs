using UnityEngine;
using System.Collections;

// For usage apply the script directly to the element you wish to apply parallaxing
// Based on Brackeys 2D parallaxing script http://brackeys.com/
public class Parallax : MonoBehaviour {
	Transform cam; // Camera reference (of its transform)
	Vector3 previousCamPos;

	public float distanceX = 0f; // Distance of the item (z-index based) 
	public float distanceY = 0f;

	public float smoothingX = 1f; // Smoothing factor of parrallax effect
	public float smoothingY = 1f;

	void Awake () {
		cam = Camera.main.transform;
	}
	
	void Update () {
		if (distanceX != 0f) {
			float parallaxX = (previousCamPos.x - cam.position.x) * distanceX;
			Vector3 backgroundTargetPosX = new Vector3(transform.position.x + parallaxX, 
			                                          transform.position.y, 
			                                          transform.position.z);
			
			// Lerp to fade between positions
			transform.position = Vector3.Lerp(transform.position, backgroundTargetPosX, smoothingX * Time.deltaTime);
		}

		if (distanceY != 0f) {
			float parallaxY = (previousCamPos.y - cam.position.y) * distanceY;
			Vector3 backgroundTargetPosY = new Vector3(transform.position.x, 
			                                           transform.position.y + parallaxY, 
			                                           transform.position.z);
			
			transform.position = Vector3.Lerp(transform.position, backgroundTargetPosY, smoothingY * Time.deltaTime);
		}

		previousCamPos = cam.position;	
	}
}
