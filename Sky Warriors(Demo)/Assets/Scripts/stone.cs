using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stone : MonoBehaviour
{
    public float amount;
    bool flag = false; //false for going up, true for going down
    float hight_minus_y;
    float hight_y;

    public float rotationSpeed = 5f;

    private void Start()
    {
        hight_y = transform.position.y + amount;
        hight_minus_y = transform.position.y - amount;
    }

    private void FixedUpdate()
    {

        gameObject.transform.Rotate(Vector3.forward, rotationSpeed);

        if (transform.position.y >= hight_y)
        {
            flag = true;

        }
        else if (transform.position.y <= hight_minus_y)
        {
            flag = false;
        }

        if (transform.position.y < hight_y && flag == false)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + Time.deltaTime * 3, transform.position.z);
        }
        else if (transform.position.y > hight_minus_y && flag == true)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - Time.deltaTime * 3, transform.position.z);
        }

    }
}
