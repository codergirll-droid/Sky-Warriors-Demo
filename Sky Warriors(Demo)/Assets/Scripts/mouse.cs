using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouse : MonoBehaviour
{

    bool isVisible = false;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        isVisible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isVisible == false)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            isVisible = true;

        }else if(Input.GetKeyDown(KeyCode.Escape) && isVisible == true)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            isVisible = false;
        }
    }
}
