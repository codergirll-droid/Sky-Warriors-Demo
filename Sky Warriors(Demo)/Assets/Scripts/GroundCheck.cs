using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    PlayerController PlayerController;

    private void Awake()
    {
        PlayerController = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            return;
        }


        PlayerController.isGrounded = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            return;
        }

        PlayerController.isGrounded = false;

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag.Equals("Player"))
        {
            return;
        }


        PlayerController.isGrounded = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == PlayerController.gameObject)
        {
            return;
        }


        PlayerController.isGrounded = true;
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject == PlayerController.gameObject)
        {
            return;
        }

        PlayerController.isGrounded = false;
    }
    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject == PlayerController.gameObject)
        {
            return;
        }

        PlayerController.isGrounded = true;
    }




}
