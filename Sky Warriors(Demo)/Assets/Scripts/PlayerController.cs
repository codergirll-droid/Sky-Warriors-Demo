using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public ProfileData playerProfile;

    //FOR MOVING
    float speed = 30f;
    float moveX, moveZ;

    //JUMP
    public bool isGrounded = true;
    [SerializeField] float jumpForce = 5f;
    Rigidbody rb;

    //FLY
    [Header("FLY")]
    [SerializeField] float flyTime;
    [SerializeField] float startFlyTime = 4f;
    [SerializeField] float flyForce = 20f;


    //FOR LOOKING AROUND
    float verticalLookRotation;
    [SerializeField] float mouseSensitivity = 20f;
    [SerializeField] GameObject cameraHolder;

    PhotonView PV;

    [SerializeField] Text playerName;
    [SerializeField] Text fpsTxt;
    float frames = 0;
    float fps = 0;

    //GUN SWITCHING
    [SerializeField] GameObject Rifle;
    [SerializeField] GameObject Pistol;

    //DAMAGE
    [SerializeField] float health;
    gameManager manager;

    //SHOOTING
    [SerializeField]
    Camera fpsCamera;


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


    //ANIMATION

    Animator Animator;


    public float maxNum = 350;
    public float minNum = 322;
    public float AngleNum;

    //UI
    [SerializeField] Slider healthbar;
    [SerializeField] Slider gassbar;
    [SerializeField] Gradient healthGradient;
    [SerializeField] Gradient gassGradient;
    [SerializeField] Image healthbarFill;
    [SerializeField] Image gassbarFill;
    [SerializeField] Text ammoCountTxt;


    //SFX
    AudioSource AudioSource;
    [SerializeField] AudioClip pistolSound;
    [SerializeField] AudioClip rifleSound;
    [SerializeField] AudioClip shotSound;
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip rocketSound;
    [SerializeField] AudioClip flightSound;
    [SerializeField] AudioClip rifleReloadingSound;
    [SerializeField] AudioClip pistolReloadingSound;

    [SerializeField] GameObject chestBone;


    [SerializeField] GameObject hitParticle;
    [SerializeField] GameObject deadParticle;


    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();

        manager = GameObject.Find("GameManager").GetComponent<gameManager>();
        fpsCamera = gameObject.GetComponentInChildren<Camera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!PV.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
        }

        Animator = gameObject.GetComponentInChildren<Animator>();

        SetPlayerUI();

        
        rb.useGravity = true;
        flyTime = startFlyTime;
        

        if (PV.IsMine)
        {
            photonView.RPC("SynchProfile", RpcTarget.AllBuffered, NetworkManager.myProfile.username, false);
        }
        health = 100f;
        SetMaxSliders();


        if (!PV.IsMine)
        {
            Destroy(rb);
        }
        ammoCountTxt.text = "Ammo : " + pistolAmmo.ToString();

        AudioSource = gameObject.GetComponent<AudioSource>();

    }

    [PunRPC]
    void SynchProfile(string s, bool ss) 
    {

        playerProfile = new ProfileData(s);

    }

    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine) return;

        Jump();



        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GunSwitcher(1);

        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GunSwitcher(2);
        }

        showFPS();
        SetSliders();

    }

    private void FixedUpdate()
    {
        if (!PV.IsMine)
        {
            return;
        }



        Move();
        Fly();
        
        Look();
        IncreaseFlyTime();


        Shooting();
        CheckIfFallen();



    }



    private void Move()
    {
        if (!PV.IsMine)
        {
            return;
        }

        moveX = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
        moveZ = Input.GetAxisRaw("Vertical") * speed * Time.deltaTime;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        transform.position += move;

        if(moveX > 0 || moveX < 0 || moveZ < 0 || moveZ > 0)
        {
            Animator.SetBool("isWalking", true);

        }
        else
        {
            Animator.SetBool("isWalking", false);

        }

    }

    void Jump()
    {
        if (!PV.IsMine)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded==true)
        {
            rb.useGravity = true;
            gameObject.transform.parent = null;
            AudioSource.clip = jumpSound;
            AudioSource.PlayOneShot(jumpSound);
            rb.AddForce(new Vector3(0f, jumpForce, 0f), ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void Look()
    {
        if (!PV.IsMine)
        {
            return;
        }

        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;

        AngleNum = cameraHolder.transform.localEulerAngles.x;

        
        if ((AngleNum < minNum || AngleNum > maxNum))
        {
            Animator.enabled = false;

        }
        else
        {
            Animator.enabled = true;
        }
        
        chestBone.transform.localEulerAngles = Vector3.back * verticalLookRotation;
    }

    void Fly()
    {
        if (!PV.IsMine)
        {
            return;
        }
        else
        {
            if ((Input.GetButtonDown("E") || Input.GetButton("E")) && flyTime > 0)
            {
                
                rb.useGravity = false;
                ReduceFlytime();
                //fly up
                Animator.SetBool("isFlying", true);
                transform.position = new Vector3(transform.position.x, transform.position.y + Time.deltaTime * flyForce, transform.position.z);

                

                gameObject.transform.parent = null;
                if (!AudioSource.isPlaying && (Input.GetButtonDown("E")))
                {
                    AudioSource.clip = rocketSound;
                    AudioSource.PlayOneShot(rocketSound);
                }else if(!AudioSource.isPlaying && (Input.GetButton("E")) && rb.useGravity==false)
                {
                    AudioSource.clip = flightSound;
                    AudioSource.Play();
                }
              

            }
            else if ((Input.GetButtonDown("E") || Input.GetButton("E")) && flyTime <= 0)
            {
                gameObject.transform.parent = null;
                if (!AudioSource.isPlaying)
                {
                    AudioSource.clip = rocketSound;
                    AudioSource.PlayOneShot(rocketSound);
                }
                else if (!AudioSource.isPlaying && rb.useGravity == false)
                {
                    AudioSource.clip = flightSound;
                    AudioSource.Play();
                }

                rb.useGravity = true;
            }
            //else if (Input.GetButtonUp("E"))
            else
            {
                rb.useGravity = true;
                Animator.SetBool("isFlying", false);
                if(AudioSource.clip == flightSound)
                {
                    AudioSource.Stop();
                }
            }

            if ((Input.GetKeyDown(KeyCode.Q) || Input.GetKey(KeyCode.Q)) && isGrounded == false)
            {
                //fly down
                //transform.position += new Vector3(0f, -0.07f, 0f);
                rb.useGravity = true;
                transform.position = new Vector3(transform.position.x, transform.position.y - Time.deltaTime * flyForce, transform.position.z);

                Animator.SetBool("isFlying", false);

            }
        }

        
        
    }

    void ReduceFlytime()
    {
        if (!PV.IsMine)
        {
            return;
        }

        if (flyTime > 0)
        {
            flyTime -= Time.deltaTime;

        }
    }

    void IncreaseFlyTime()
    {
        if (!PV.IsMine)
        {
            return;
        }

        if (flyTime < startFlyTime && !Input.GetKey(KeyCode.E) && !Input.GetKeyDown(KeyCode.E))
        {
            flyTime += Time.deltaTime / 2;

        }
    }

    void SetPlayerUI()
    {
        if(playerName != null)
        {
            playerName.text = photonView.Owner.NickName;
        }
    }

    void GunSwitcher(int idx)
    {

      
        if (idx == 1)
        {
            Pistol.SetActive(true);
            Rifle.SetActive(false);
            if (!ammoCountTxt.IsDestroyed())
            {
                ammoCountTxt.text = "Ammo : " + pistolAmmo.ToString();

            }
        }

        if (idx == 2)
        {
            Pistol.SetActive(false);
            Rifle.SetActive(true);
            if (!ammoCountTxt.IsDestroyed())
            {
                ammoCountTxt.text = "Ammo : " + rifleAmmo.ToString();

            }


        }


        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("gunIndex", idx);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
        

    }

    
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {

        if(!PV.IsMine && targetPlayer == PV.Owner)
        {
            GunSwitcher((int)changedProps["gunIndex"]);
        }
        
    }
    

    //DAMAGE METHODS
    public void TakeDamage(float _damage, int p_actor)
    {
        if (photonView.IsMine)
        {
            //ADD HEALTHBAR UI HERE
            health -= _damage;
            Debug.Log(health);

            if (health <= 0)
            {
                playDieSound(this.transform);

                health = 0;

                manager.Spawn();
                manager.ChangeStat_S(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);
                if (p_actor >= 0)
                    manager.ChangeStat_S(p_actor, 0, 1);
                Debug.Log("Player died");

                instantiateParticle(deadParticle.name, this.transform);

                PhotonNetwork.Destroy(gameObject);


            }
            else
            {
                AudioSource.clip = shotSound;
                AudioSource.PlayOneShot(shotSound);
            }
        }       

    }


    void Shooting()
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
            AudioSource.clip = rifleSound;

            if (!AudioSource.isPlaying)
            {
                AudioSource.Play();

            }


            rifleFireTimer = 0.0f;
            RaycastHit _hit;
            if (fpsCamera != null)
            {
                rifleAmmo -= 1;
                ammoCountTxt.text = "Ammo : " + rifleAmmo.ToString();
                Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
                rayCount += 1;
                Debug.Log(rayCount);



                if (Physics.Raycast(ray, out _hit, 100) && Rifle.activeSelf)
                {
                    Debug.Log(_hit.collider.gameObject.name);

                    if (_hit.collider.gameObject.CompareTag("Player") && _hit.collider.gameObject.TryGetComponent(out PhotonView PV) && !PV.IsMine)
                    {
                       
                        _hit.collider.transform.gameObject.GetPhotonView().RPC("takeDamage", RpcTarget.All, RifleDamage, PhotonNetwork.LocalPlayer.ActorNumber);
                        instantiateParticle(hitParticle.name, _hit.transform);

                    }

                }


            }


        }

        else if (Input.GetButtonDown("Fire1") && pistolFireTimer > PistolFireRate && Pistol.activeSelf && isReloading != true && isReloadingPistol == false)
        {
            AudioSource.clip = pistolSound;
            AudioSource.PlayOneShot(pistolSound);           

            pistolFireTimer = 0.0f;
            RaycastHit _hit;
            if (fpsCamera != null)
            {
                Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
                rayCount += 1;
                Debug.Log(rayCount);
                pistolAmmo -= 1;
                ammoCountTxt.text = "Ammo : " + pistolAmmo.ToString();


                if (Physics.Raycast(ray, out _hit, 100) && Pistol.activeSelf)
                {
                    Debug.Log(_hit.collider.gameObject.name);

                    if (_hit.collider.gameObject.CompareTag("Player") && _hit.collider.gameObject.TryGetComponent(out PhotonView PV) && !PV.IsMine)
                    {
                       
                        _hit.collider.transform.gameObject.GetPhotonView().RPC("takeDamage", RpcTarget.All, RifleDamage, PhotonNetwork.LocalPlayer.ActorNumber);
                        instantiateParticle(hitParticle.name, _hit.transform);


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
        AudioSource.clip = rifleReloadingSound;
        AudioSource.PlayOneShot(rifleReloadingSound);

        isReloadingRifle = true;
        Debug.Log("Reloading rifle ammo...");
        ammoCountTxt.text = "Reloading...";

        yield return new WaitForSeconds(3f);
        rifleAmmo = 50f;
        isReloadingRifle = false;
        ammoCountTxt.text = "Ammo : " + rifleAmmo.ToString();


    }

    IEnumerator waitForPistol()
    {
        AudioSource.clip = pistolReloadingSound;
        AudioSource.PlayOneShot(pistolReloadingSound);

        isReloadingPistol = true;
        Debug.Log("Reloading pistol ammo...");
        ammoCountTxt.text = "Reloading...";

        yield return new WaitForSeconds(1f);
        pistolAmmo = 15f;
        isReloadingPistol = false;
        ammoCountTxt.text = "Ammo : " + pistolAmmo.ToString();

    }

    IEnumerator rifleReloading()
    {
        AudioSource.clip = rifleReloadingSound;
        AudioSource.PlayOneShot(rifleReloadingSound);

        ammoCountTxt.text = "Reloading...";
        yield return new WaitForSeconds(rifleReloadTime);       
        isReloading = false;
        ammoCountTxt.text = "Ammo : " + rifleAmmo.ToString();

    }

    IEnumerator pistolReloading()
    {
        AudioSource.clip = pistolReloadingSound;
        AudioSource.PlayOneShot(pistolReloadingSound);

        ammoCountTxt.text = "Reloading...";
        yield return new WaitForSeconds(pistolReloadTime);
        Debug.Log("Pistol reloading...");
        isReloading = false;
        ammoCountTxt.text = "Ammo : " + pistolAmmo.ToString();

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

        //ADD RPC HERE----------------------------------------------------------------------------------------------------------------------------------


    }

    [PunRPC]
    void takeDamage (float p_damage, int p_actor)
    {
        GetComponent<PlayerController>().TakeDamage(p_damage, p_actor);
    }


    void CheckIfFallen()
    {
        if(gameObject.transform.position.y <= -23f)
        {
            playDieSound(this.gameObject.transform);
                            
            //gameObject.GetPhotonView().RPC("dieSoundRPC", RpcTarget.AllBuffered, this.transform);
            

            manager.Spawn();
            manager.ChangeStat_S(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);
            Debug.Log("Player died");
            PhotonNetwork.Destroy(gameObject);

        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "stone")
        {
            manager.SetHasStone(playerProfile);
            manager.isWinner = true;
            manager.ChangeStat_S(PhotonNetwork.LocalPlayer.ActorNumber, 2, 0);
            Destroy(other.gameObject);
        }
    }



    void SetMaxSliders()
    {
        healthbar.maxValue = health;
        healthbar.value = health;
        Color h = healthGradient.Evaluate(1f);
        h.a = 0.4f;
        healthbarFill.color = h;

        gassbar.maxValue = startFlyTime;
        gassbar.value = flyTime;
        Color g = gassGradient.Evaluate(1f);
        g.a = 0.4f;
        gassbarFill.color = g;

    }

    void SetSliders()
    {
        healthbar.value = health;
        Color h  = healthGradient.Evaluate(healthbar.normalizedValue);
        h.a = 0.4f;
        healthbarFill.color = h;

        gassbar.value = flyTime;
        Color g = gassGradient.Evaluate(gassbar.normalizedValue);
        g.a = 0.4f;
        gassbarFill.color = g;
        
    }

    public void playDieSound(Transform transform)
    {
        manager.transform.position = transform.position;
        manager.playDieSound();
    }

    [PunRPC]
    void soundRPC(AudioClip audioClip)
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.clip = audioClip;
        audioSource.PlayOneShot(audioClip);

    }

    [PunRPC]
    void dieSoundRPC(Transform transform)
    {
        
        playDieSound(transform);
    }

    
    private void OnCollisionEnter(Collision other)
    {


        if(other.gameObject.tag.Equals("miniisland"))
        {
            Debug.Log("Miniisland");

            gameObject.transform.parent = other.transform.parent;
            
        }

    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag.Equals("miniisland"))
        {
            Debug.Log("Miniisland");

            gameObject.transform.parent = other.transform.parent;

        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag.Equals("miniisland"))
        {
            Debug.Log("Null");

            gameObject.transform.parent = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag.Equals("miniisland"))
        {
            Debug.Log("Miniisland");

            gameObject.transform.parent = other.transform.parent;

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("miniisland"))
        {
            Debug.Log("Null");

            gameObject.transform.parent = null;
        }

        

    }
    

    void showFPS()
    {
        frames++;
        fps = Mathf.Round(frames / Time.realtimeSinceStartup);
        if(fpsTxt != null)
        {
            fpsTxt.text = "fps: " + fps.ToString();

        }

    }

    [PunRPC]
    void resurrection(int spawnPoint)
    {
        manager.Resurrection(spawnPoint);

    }

    void instantiateParticle(string particleName, Transform transform)
    {
        GameObject x = PhotonNetwork.Instantiate(particleName, new Vector3(transform.position.x, transform.position.y + 4, transform.position.z), transform.rotation); 
        x.SetActive(true);
    }


}
