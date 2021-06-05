using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class floatingislands : MonoBehaviour
{
    public float amount;
    bool flag = false; //false for going up, true for going down
    float hight_minus_y;
    float hight_y;
    public float speed = 3f;

    private void Start()
    {
        hight_y = transform.position.y + amount;
        hight_minus_y = transform.position.y - amount;
    }

    private void FixedUpdate()
    {

        floating();

    }

    public void floating()
    {
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
            transform.position = new Vector3(transform.position.x, transform.position.y + Time.deltaTime * speed, transform.position.z);
        }
        else if (transform.position.y > hight_minus_y && flag == true)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - Time.deltaTime * speed, transform.position.z);
        }
    }

}



