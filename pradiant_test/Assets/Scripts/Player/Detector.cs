using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    private bool inrange= false;
    private float speed = 60f;
    private float minY = 85;
    private float maxY = 275f;
    private void Update()
    {
        if (inrange == true)
        {
            transform.Rotate(Vector3.down * speed * Time.deltaTime);
        }
        else if (inrange == false)
        {
            transform.Rotate(Vector3.up * speed * Time.deltaTime);
        }
        Vector3 angle = transform.localEulerAngles;
        angle.y = Mathf.Clamp(transform.localRotation.eulerAngles.y, minY, maxY);
        transform.localEulerAngles = angle;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            inrange = true;
            Debug.Log("inrange");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            inrange = false;
            Debug.Log("outofrange");
        }
    }
}
