using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TakingDamage : MonoBehaviourPunCallbacks
{
    [SerializeField] float health;
    gameManager manager;

    // Start is called before the first frame update
    void Start()
    {
        health = 100f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    //[PunRPC]
    public void TakeDamage(float _damage, PhotonMessageInfo info)
    {
        //ADD HEALTHBAR UI HERE
        health -= _damage;
        Debug.Log(health);

        if(health <= 0)
        {
            Debug.Log(info.Sender.NickName + " killed " + info.photonView.Owner);
            Die();
        }

    }

    void Die()
    {
        health = 0;

        //ADD DIE/RESPAWN ALGORITHM
        manager.ChangeStat_S(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);
        Debug.Log("Player died");
        //destroy gameobject
    }

}
