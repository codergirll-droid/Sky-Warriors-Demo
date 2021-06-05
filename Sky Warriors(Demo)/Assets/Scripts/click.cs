using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class click : MonoBehaviour
{
    AudioSource AudioSource;

    public void onClick()
    {
        AudioSource = GameObject.Find("button").GetComponent<AudioSource>();
        AudioSource.Play();
    }


}
