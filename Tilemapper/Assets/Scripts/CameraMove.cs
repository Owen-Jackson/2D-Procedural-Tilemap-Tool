using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour {
    [SerializeField]
    float minZoom = 30;
    [SerializeField]
    float maxZoom = 3;
    [SerializeField]
    float moveSpeed = 5;
    [SerializeField]
    float currentZoom;
	// Use this for initialization
	void Start () {
        currentZoom = Camera.main.orthographicSize;
	}
	
	// Update is called once per frame
	void Update () {

        //Translation
		if(Input.GetKey(KeyCode.A))
        {
            transform.position += new Vector3(-1.0f, 0, 0) * Time.deltaTime * moveSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += new Vector3(1.0f, 0, 0) * Time.deltaTime * moveSpeed;
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += new Vector3(0, 1.0f, 0) * Time.deltaTime * moveSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += new Vector3(0, -1.0f, 0) * Time.deltaTime * moveSpeed;
        }

        //Zooming in and out
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (currentZoom < minZoom)
            {
                currentZoom++;
                Camera.main.orthographicSize++;
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (currentZoom > maxZoom)
            {
                currentZoom--;
                Camera.main.orthographicSize--;
            }
        }
    }
}
