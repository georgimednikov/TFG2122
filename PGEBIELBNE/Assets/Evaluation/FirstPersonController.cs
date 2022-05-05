using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public float yawSpeed;
    public float pitchSpeed;
    public float camSpeed;

    Camera thisCam;

    float yaw = 0;
    float pitch = 0;

    public void Awake()
    {
        thisCam = this.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            yaw += yawSpeed * Input.GetAxis("Mouse X");
            pitch -= pitchSpeed * Input.GetAxis("Mouse Y");

            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W))
        {
            thisCam.transform.position += thisCam.transform.forward * camSpeed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            thisCam.transform.position -= thisCam.transform.forward * camSpeed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            thisCam.transform.position -= thisCam.transform.right * camSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            thisCam.transform.position += thisCam.transform.right * camSpeed;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            thisCam.transform.position -= thisCam.transform.up * camSpeed;
        }
        if (Input.GetKey(KeyCode.E))
        {
            thisCam.transform.position += thisCam.transform.up * camSpeed;
        }
    }
}
