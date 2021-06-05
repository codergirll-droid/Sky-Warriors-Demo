using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlsMenu : MonoBehaviour
{
    public void controllsScene()
    {
        SceneManager.LoadScene("Controls");
    }

    public void creditsScene()
    {
        SceneManager.LoadScene("Credits");
    }

}
