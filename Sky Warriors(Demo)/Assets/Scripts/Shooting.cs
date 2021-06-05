using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shooting : MonoBehaviour
{

    [SerializeField]
    Camera fpsCamera;

    [SerializeField] GameObject Rifle;
    [SerializeField] GameObject Pistol;

    [SerializeField] float RifleDamage = 20f;
    [SerializeField] float PistolDamage = 10f;

    float RifleFireRate = 0.1f;
    float PistolFireRate = 1f;

    [SerializeField] float rifleFireTimer = 0;
    [SerializeField] float pistolFireTimer = 0;

    [SerializeField] float pistolAmmo = 15f;
    [SerializeField] float rifleAmmo = 50f;

    [SerializeField] float rifleReloadTime = 3f;
    [SerializeField] float pistolReloadTime = 1f;

    bool isReloadingRifle = false;
    bool isReloadingPistol = false;

    bool isReloading = false;

    int rayCount = 0;

    [SerializeField] int pistolAimSpeed;
    [SerializeField] int rifleAimSpeed;

    private void Awake()
    {
        fpsCamera = gameObject.GetComponentInChildren<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (rifleFireTimer < RifleFireRate)
        {
            rifleFireTimer += Time.deltaTime;
        }
        if (pistolFireTimer < PistolFireRate)
        {
            pistolFireTimer += Time.deltaTime;
        }

        if (rifleAmmo <= 0 && isReloadingRifle == false)
        {
            StartCoroutine(waitForRifle());
            return;
        }

        if (pistolAmmo <= 0 && isReloadingPistol == false)
        {
            StartCoroutine(waitForPistol());
            return;
        }

        else if (Input.GetButton("Fire1") && rifleFireTimer > RifleFireRate && Rifle.activeSelf && isReloading != true && isReloadingRifle == false)
        {
            rifleFireTimer = 0.0f;
            RaycastHit _hit;
            if (fpsCamera != null)
            {
                rifleAmmo -= 1;
                Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
                rayCount += 1;
                Debug.Log(rayCount);



                if (Physics.Raycast(ray, out _hit, 100) && Rifle.activeSelf)
                {
                    Debug.Log(_hit.collider.gameObject.name);

                    if (_hit.collider.gameObject.CompareTag("Player") && _hit.collider.gameObject.TryGetComponent(out PhotonView PV) && !PV.IsMine)
                    {
                        _hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, RifleDamage);
                    }

                }


            }


        }

        else if (Input.GetButtonDown("Fire1") && pistolFireTimer > PistolFireRate && Pistol.activeSelf && isReloading != true && isReloadingPistol == false)
        {

            pistolFireTimer = 0.0f;
            RaycastHit _hit;
            if (fpsCamera != null)
            {
                Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
                rayCount += 1;
                Debug.Log(rayCount);
                pistolAmmo -= 1;


                if (Physics.Raycast(ray, out _hit, 100) && Pistol.activeSelf)
                {
                    Debug.Log(_hit.collider.gameObject.name);

                    if (_hit.collider.gameObject.CompareTag("Player") && _hit.collider.gameObject.TryGetComponent(out PhotonView PV) && !PV.IsMine)
                    {
                        _hit.collider.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.All, PistolDamage);
                    }

                }


            }


        }



        if (Input.GetKeyDown(KeyCode.R) && Rifle.activeSelf && rifleAmmo != 50)
        {
            rifleAmmo = 50;
            isReloading = true;
            StartCoroutine(rifleReloading());
        }
        if (Input.GetKeyDown(KeyCode.R) && Pistol.activeSelf && pistolAmmo != 15)
        {
            pistolAmmo = 15;
            isReloading = true;
            StartCoroutine(pistolReloading());
        }


        Aim();

    }


    IEnumerator waitForRifle()
    {
        isReloadingRifle = true;
        Debug.Log("Reloading rifle ammo...");
        yield return new WaitForSeconds(3f);
        rifleAmmo = 50f;
        isReloadingRifle = false;

    }

    IEnumerator waitForPistol()
    {
        isReloadingPistol = true;
        Debug.Log("Reloading pistol ammo...");
        yield return new WaitForSeconds(1f);
        pistolAmmo = 15f;
        isReloadingPistol = false;
    }

    IEnumerator rifleReloading()
    {
        yield return new WaitForSeconds(rifleReloadTime);
        Debug.Log("Rifle reloading...");
        isReloading = false;
    }

    IEnumerator pistolReloading()
    {
        yield return new WaitForSeconds(pistolReloadTime);
        Debug.Log("Pistol reloading...");
        isReloading = false;
    }


    void Aim()
    {
        Transform currentPosPistol = Pistol.transform.Find("States/NormalState");
        Transform aimPosPistol = Pistol.transform.Find("States/AimingState");
        Transform anchorPistol = Pistol.transform.Find("Anchor");

        Transform currentPosRifle = Rifle.transform.Find("States/NormalState");
        Transform aimPosRifle = Rifle.transform.Find("States/AimingState");
        Transform anchorRifle = Rifle.transform.Find("Anchor");

        if (Input.GetMouseButton(1) && Pistol.activeSelf)
        {
            anchorPistol.position = Vector3.Lerp(anchorPistol.position, aimPosPistol.position, Time.deltaTime * pistolAimSpeed);
        }
        else
        {
            anchorPistol.position = Vector3.Lerp(anchorPistol.position, currentPosPistol.position, Time.deltaTime * pistolAimSpeed);

        }

        if (Input.GetMouseButton(1) && Rifle.activeSelf)
        {
            anchorRifle.position = Vector3.Lerp(anchorRifle.position, aimPosRifle.position, Time.deltaTime * rifleAimSpeed);
        }
        else
        {
            anchorRifle.position = Vector3.Lerp(anchorRifle.position, currentPosRifle.position, Time.deltaTime * rifleAimSpeed);

        }


    }


}

