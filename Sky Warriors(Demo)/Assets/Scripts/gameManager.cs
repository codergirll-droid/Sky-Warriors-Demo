using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerInfo 
{
    public ProfileData Profile;
    public int actor;
    public short kills;
    public short deaths;
    public bool hasStone;

    public PlayerInfo (ProfileData p, int a, short k, short d, bool s)
    {
        this.Profile = p;
        this.actor = a;
        this.kills = k;
        this.deaths = d;
        this.hasStone = s;
    }

}


public class gameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [SerializeField]
    GameObject playerPrefab;
    [SerializeField]
    Transform[] transforms;
    [SerializeField]
    int randomSpawnPoint;

    public List<PlayerInfo> playerInfos = new List<PlayerInfo>();
    public int myind;


    Transform ui_leaderboard;
    public List<PlayerInfo> playerInfo;

    public bool hasStone = false;
    public string whoHasStone = "";
    public GameObject mapcam;
    Transform ui_hasStone;
    GameState state = GameState.Waiting;
    public Text whoHasStoneTxt;

    int matchLength = 240; //match Length hereeeeeeeeeeeeeeeeeee
    Text ui_timer;
    int currentMatchTime;
    Coroutine timerCoroutine;
    Transform ui_endgametimer;
    bool hasTimeFinished = false;
    bool isLeaderboardOpen = false;

    public bool isWinner = false;

    Text kills;
    Text deaths;

    AudioSource audioSource;
    [SerializeField] AudioClip dieSound;
    [SerializeField] AudioClip winSound;
    [SerializeField] AudioClip lostSound;

    Image im;
    [SerializeField] GameObject deadParticle;



    public enum GameState
    {
        Waiting = 0,
        Starting = 1,
        Playing = 2,
        Ending = 3
    }

    public enum EventCodes : byte
    {
        NewPlayer, 
        UpdatePlayers,
        ChangeStat,
        RefreshTimer
    }


    // Start is called before the first frame update
    void Start()
    {


        mapcam.SetActive(false);
        ValidateConnection();
        InitializeUI();
        NewPlayer_S(NetworkManager.myProfile);
        Spawn();

        playerInfo = new List<PlayerInfo>();

        InitializeTimer();

        audioSource = gameObject.GetComponent<AudioSource>();

    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // Update is called once per frame
    void Update()
    {
        CheckConnection();

        if(state == GameState.Ending)
        {
            return;
        }

        
        if (Input.GetKeyDown(KeyCode.Tab) && isLeaderboardOpen == false)
        {
            isLeaderboardOpen = true;
            Leaderboard(ui_leaderboard);
        }
        else if (Input.GetKeyDown(KeyCode.Tab) && isLeaderboardOpen == true)
        {
            isLeaderboardOpen = false;
            ui_leaderboard.gameObject.SetActive(false);
        }

        if (hasStone || hasTimeFinished)
        {
            StateCheck();
        }
        

    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(0);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code >= 200) return;

        EventCodes e = (EventCodes)photonEvent.Code;
        object[] o = (object[])photonEvent.CustomData;

        switch (e)
        {
            case EventCodes.NewPlayer:
                NewPlayer_R(o);
                break;
            case EventCodes.UpdatePlayers:
                UpdatePlayers_R(o);
                break;
            case EventCodes.ChangeStat:
                ChangeStat_R(o);
                break;
            case EventCodes.RefreshTimer:
                RefreshTimer_R(o);
                break;
        }

    }

    public void Spawn()
    {
        randomSpawnPoint = Random.Range(0, transforms.Length);

        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (playerPrefab != null)
            {
                GameObject x = PhotonNetwork.Instantiate(playerPrefab.name, transforms[randomSpawnPoint].position, transforms[randomSpawnPoint].rotation);

                x.GetPhotonView().RPC("resurrection", RpcTarget.AllBuffered, randomSpawnPoint);



            }
            else
            {
                Debug.Log("Player prefab is missing!");
                GameObject newPlayer = Instantiate(playerPrefab, transforms[randomSpawnPoint].position, transforms[randomSpawnPoint].rotation) as GameObject; 
            }

        }
    }

    
    void ValidateConnection()
    {
        if (PhotonNetwork.IsConnected)
        {
            return;

        }
        else
        {
            SceneManager.LoadScene(0);
            Debug.Log("Lost connection.");
        }
    }

    public void NewPlayer_S(ProfileData p)
    {
        object[] package = new object[5];

        package[0] = p.username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = (short)0;
        package[3] = (short)0;
        package[4] = false;

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient},
            new SendOptions { Reliability = true}
            );

    }

    public void NewPlayer_R(object[] data)
    {
        PlayerInfo p = new PlayerInfo(
            new ProfileData(
                (string)data[0]
                ),
            (int)data[1],
            (short)data[2],
            (short)data[3],
            (bool)data[4]
            );

        playerInfos.Add(p);
        UpdatePlayers_S((int)state, playerInfos);
    }

    public void UpdatePlayers_S(int state, List<PlayerInfo> info)
    {
        object[] package = new object[info.Count + 1];

        package[0] = state;
        for(int i = 0; i < info.Count; i++)
        {
            object[] piece = new object[5];

            piece[0] = info[i].Profile.username;
            piece[1] = info[i].actor;
            piece[2] = info[i].kills;
            piece[3] = info[i].deaths;
            piece[4] = info[i].hasStone;

            package[i + 1] = piece;

        }

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.UpdatePlayers,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );

    }

    public void UpdatePlayers_R(object[] data)
    {
        state = (GameState)data[0];
        playerInfos = new List<PlayerInfo>();

        for (int i = 1; i < data.Length; i++)
        {
            object[] extract = (object[])data[i];

            PlayerInfo p = new PlayerInfo(
                new ProfileData(
                    (string)extract[0]
                    ),
                (int)extract[1],
                (short)extract[2],
                (short)extract[3],
                (bool)extract[4]
                );

            playerInfos.Add(p);
            if (PhotonNetwork.LocalPlayer.ActorNumber == p.actor) myind = i-1;

        }
        StateCheck();
    }

    public void ChangeStat_S(int actor, byte stat, byte amt)
    {
        object[] package = new object[] { actor, stat, amt };
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ChangeStat,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void ChangeStat_R(object[] data)
    {
        int actor = (int)data[0];
        byte stat = (byte)data[1];
        byte amt = (byte)data[2];

        for (int i = 0; i < playerInfos.Count; i++)
        {
            if(playerInfos[i].actor == actor)
            {
                switch (stat)
                {
                    case 0: //kills
                        playerInfos[i].kills += amt;
                        Debug.Log($"Player { playerInfos[i].Profile.username} : kills = {playerInfos[i].kills}");
                        
                        break;
                    case 1: //deaths
                        playerInfos[i].deaths += amt;
                        Debug.Log($"Player { playerInfos[i].Profile.username} : deaths = {playerInfos[i].deaths}");
                        break;
                    case 2: //hasStone
                        playerInfos[i].hasStone = true;
                        Debug.Log($"Player { playerInfos[i].Profile.username} : hasStone = {playerInfos[i].hasStone}");
                        break;

                }

                if (i == myind) RefreshMyStats();
                if (ui_leaderboard.gameObject.activeSelf) Leaderboard(ui_leaderboard);
                break;
            }
        }

        ScoreCheck();
    }

    void CheckConnection()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Disconnected");

            SceneManager.LoadScene(0);
        }
    }

    void InitializeUI()
    {
        ui_leaderboard = GameObject.Find("Canvas/HUD").transform.Find("LeaderBoard").transform;
        ui_hasStone = GameObject.Find("Canvas/HUD").transform.Find("hasStone").transform;
        ui_endgametimer = GameObject.Find("Canvas/HUD").transform.Find("EndGameTimer").transform;
        ui_timer = GameObject.Find("Canvas/HUD/Timer").GetComponent<Text>();
        kills = GameObject.Find("Canvas/HUD/Kills").GetComponent<Text>();
        deaths = GameObject.Find("Canvas/HUD/Deaths").GetComponent<Text>();
        im = GameObject.Find("Canvas/HUD/Image").GetComponent<Image>();

        RefreshMyStats();
    }

    public void RefreshMyStats()
    {
        Debug.Log("Called refreshmystats");

        
        if (playerInfos.Count > myind)
        {
            kills.text = $"Kills : {playerInfos[myind].kills}";
            deaths.text = $"Deaths : {playerInfos[myind].deaths}";
        }
        else
        {
            kills.text = "Kills : 0";
            deaths.text = "Deaths : 0";
        }
        

        if (ui_leaderboard.gameObject.activeSelf) Leaderboard(ui_leaderboard);

    }

    public void Leaderboard(Transform p_lb)
    {
        Debug.Log("Called Leaderboard");

        for (int i = 2; i < p_lb.childCount; i++)
        {
            Destroy(p_lb.GetChild(i).gameObject);
            Debug.Log("Destroyed children of leaderboard.");
        }

        GameObject playercard = p_lb.GetChild(1).gameObject;
        Debug.Log("Got the player card " + playercard.gameObject.name);
        playercard.SetActive(false);

        List<PlayerInfo> sorted = SortPlayers(playerInfos);


        foreach (PlayerInfo a in sorted)
        {
            GameObject newcard = Instantiate(playercard, p_lb) as GameObject;
            newcard.transform.Find("PlayerName").GetComponent<Text>().text = a.Profile.username;
            newcard.transform.Find("Kills").GetComponent<Text>().text = a.kills.ToString();
            newcard.transform.Find("Deaths").GetComponent<Text>().text = a.deaths.ToString();

            Debug.Log("Created card");
            newcard.SetActive(true);

        }
        

        p_lb.gameObject.SetActive(true);
        p_lb.parent.gameObject.SetActive(true);

    }

    List<PlayerInfo> SortPlayers(List<PlayerInfo> p_info)
    {
        Debug.Log("Called sort players");

        List<PlayerInfo> sorted = new List<PlayerInfo>();

        while (sorted.Count < p_info.Count)
        {
            short highest = -1;
            PlayerInfo selection = p_info[0];

            foreach (PlayerInfo a in p_info)
            {
                if (sorted.Contains(a)) continue;
                if (a.kills > highest)
                {
                    selection = a;
                    highest = a.kills;
                }
            }

            sorted.Add(selection);
        }
        return sorted;
    }

    void StateCheck()
    {


        if(state == GameState.Ending && hasStone==true && hasTimeFinished == false)
        {
            Debug.Log("Inside state check called");
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            currentMatchTime = 0;
            RefreshTimerUI();
            EndGamehasStone();
        }
        

        else if(state == GameState.Ending)
        {
            Debug.Log("EndGameTimer called");
            EndGameTimer();
            hasTimeFinished = false;
        }
        

    }

    void ScoreCheck()
    {
        Debug.Log("ScoreCheck called");

        foreach(PlayerInfo a in playerInfos)
        {
            if(a.hasStone == true)
            {
                whoHasStone = a.Profile.username;
                hasStone = true;
                break;
            }
        }

        if(hasStone == true)
        {
           
            if (PhotonNetwork.IsMasterClient && state != GameState.Ending)
            {
                UpdatePlayers_S((int)GameState.Ending, playerInfos);
            }
            
        }
    }

    void EndGamehasStone()
    {
        Debug.Log("EndgamehasStone called");

        state = GameState.Ending;

        if (isWinner)
        {
            gameObject.GetComponent<AudioListener>().enabled = true;

            audioSource.clip = winSound;
            audioSource.loop = true;
            audioSource.Play();
            StartCoroutine(wait(winSound));
        }
        else
        {
            gameObject.GetComponent<AudioListener>().enabled = true;

            audioSource.clip = lostSound;
            audioSource.loop = true;
            audioSource.Play();
            StartCoroutine(wait(lostSound));

        }


        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;

        }
        

        mapcam.SetActive(true);

        if (isWinner)
        {
            ui_hasStone.transform.Find("GameOverTxt").GetComponent<Text>().text = "Congragulations! \n You got the Stone of the Skies! \n You are now the Queen/King of \n the Skies!";
            Color g;
            ColorUtility.TryParseHtmlString("#0CFF00", out g);
            ui_hasStone.transform.Find("GameOverTxt").GetComponent<Text>().color = g;
            ui_hasStone.transform.Find("GameOverTxt").GetComponent<Text>().fontSize = 20;

            ui_hasStone.transform.Find("GameOverTxt").GetComponent<Text>().transform.localPosition = new Vector2(ui_hasStone.transform.Find("GameOverTxt").GetComponent<Text>().transform.localPosition.x, 29); ;

            ui_hasStone.transform.Find("WinnerTxt").gameObject.SetActive(false);

            


        }
        

        ui_hasStone.gameObject.SetActive(true);

       
        whoHasStoneTxt.text = "Player " + whoHasStone + " got the stone!";

        StartCoroutine(End(10f));

    }

    IEnumerator End(float p_wait)
    {
        yield return new WaitForSeconds(p_wait);

        PhotonNetwork.AutomaticallySyncScene = false;
        if (PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.LeaveRoom();

        }

    }

    public void SetHasStone(ProfileData data)
    {
        foreach (PlayerInfo info in playerInfos)
        {
            if(info.Profile.username == data.username)
            {
                Debug.Log("Set has stone to true for " + data.username);
                info.hasStone = true;
            }
        }
    }

    void InitializeTimer()
    {
        currentMatchTime = matchLength;
        RefreshTimerUI();

        if (PhotonNetwork.IsMasterClient)
        {
            timerCoroutine = StartCoroutine(Timer());
        }

    }

    void RefreshTimerUI()
    {
        string minutes = (currentMatchTime / 60).ToString("00");
        string seconds = (currentMatchTime % 60).ToString("00");
        ui_timer.text = $"{minutes}:{seconds}";
    }


    private void EndGameTimer()
    {
        // set game state to ending
        state = GameState.Ending;

        gameObject.GetComponent<AudioListener>().enabled = true;


        audioSource.clip = lostSound;
        audioSource.loop = true;
        audioSource.Play();

        StartCoroutine(wait(lostSound));

        // set timer to 0
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        currentMatchTime = 0;
        RefreshTimerUI();

        // disable room
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;

        }

        // activate map camera
        mapcam.SetActive(true);

        // show end game ui
        im.gameObject.SetActive(false);
        kills.gameObject.SetActive(false);
        deaths.gameObject.SetActive(false);
        ui_endgametimer.gameObject.SetActive(true);
        Leaderboard(ui_endgametimer.Find("LeaderBoard"));

        

        // wait X seconds and then return to main menu
        StartCoroutine(End(10f));
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(1f);

        currentMatchTime -= 1;

        if (currentMatchTime <= 0)
        {
            timerCoroutine = null;
            //hasTimeFinished = true;//----------------------------------
            UpdatePlayers_S((int)GameState.Ending, playerInfos);
        }
        else
        {
            RefreshTimer_S();
            timerCoroutine = StartCoroutine(Timer());
        }
    }

    public void RefreshTimer_S()
    {
        object[] package = new object[] { currentMatchTime };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.RefreshTimer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }
    public void RefreshTimer_R(object[] data)
    {
        currentMatchTime = (int)data[0];
        RefreshTimerUI();
    }

    public void playDieSound()
    {
        audioSource.clip = dieSound;
        audioSource.volume = Random.Range(0, 0.5f);
        audioSource.PlayOneShot(dieSound);
        audioSource.volume = 1f;

    }

    IEnumerator wait(AudioClip clip)
    {
        yield return new WaitForSeconds(clip.length);
    }

    IEnumerator setFalseActive(float time)
    {
        yield return new WaitForSeconds(time);
        transforms[randomSpawnPoint].gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }



    public void Resurrection(int randomSpawnPoint)
    {
        transforms[randomSpawnPoint].gameObject.transform.GetChild(0).gameObject.SetActive(false);
        transforms[randomSpawnPoint].gameObject.transform.GetChild(0).gameObject.SetActive(true);
        StartCoroutine(setFalseActive(5));
    }

    public void dead(Transform a)
    {
        Debug.LogWarning("manager dead called");
        Instantiate(deadParticle, a);
        //x.SetActive(true);
    }

}
