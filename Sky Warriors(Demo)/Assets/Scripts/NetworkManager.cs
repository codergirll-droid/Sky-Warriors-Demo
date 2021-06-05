using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;


[System.Serializable]
public class ProfileData
{
    public string username;

    public ProfileData()
    {
        this.username = "Default user name.";
    }

    public ProfileData(string u)
    {
        this.username = u;
    }

}

public class NetworkManager : MonoBehaviourPunCallbacks

{
    public static ProfileData myProfile = new ProfileData();


    [Header("Connection Status")]
    public Text connectionStatus;

    [Header("Login UI Panel")]
    public InputField playerNameInput;
    public GameObject login_uiPanel;


    [Header("Game Options UI Panel")]
    public GameObject gameoptionsUIPanel;

    [Header("Create Room UI Panel")]
    public GameObject CreateRoom_UI_Panel;
    public InputField roomNameInputField;
    public InputField maxPlayerInputField;

    [Header("Inside Room UI Panel")]
    public GameObject InsideRoom_UI_Panel;
    public Text roomInfoText;
    public GameObject playerListPrefab;
    public GameObject playerListContent;
    public GameObject startGameButton;

    [Header("Room List UI Panel")]
    public GameObject RoomList_UI_Panel;
    public GameObject roomListPrefab;
    public GameObject roomListParent;

    [Header("Join Random Room UI Panel")]
    public GameObject JoinRandomRoom_UI_Panel;


    Dictionary<string, RoomInfo> catchedRoomList;
    Dictionary<string, GameObject> roomListGameobjects;
    Dictionary<int, GameObject> playerListGameObjects;

    int currentOnlinePlayerCount = 0;
    [SerializeField] Text currentOnlinePlayerCountTxt;


    #region Unity Methods

    private void Awake()
    {
        myProfile = Data.LoadProfile();
    }

    // Start is called before the first frame update
    void Start()
    {
        PlayerPrefs.DeleteAll();

        ActivatePanel(login_uiPanel.name);

        catchedRoomList = new Dictionary<string, RoomInfo>();
        roomListGameobjects = new Dictionary<string, GameObject>();
        

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Update is called once per frame
    void Update()
    {
        connectionStatus.text = "Connection status: " + PhotonNetwork.NetworkClientState;
        currentOnlinePlayerCount = PhotonNetwork.CountOfPlayers;
        if(currentOnlinePlayerCount > 1)
        {
            currentOnlinePlayerCountTxt.text = currentOnlinePlayerCount.ToString() + " players are online...";

        }
        else
        {
            currentOnlinePlayerCountTxt.text = "1 player is online...";
        }


    }

    #endregion

    #region UI Callbacks

    public void OnLoginButtonClicked()
    {
        string playerName = playerNameInput.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
            myProfile.username = playerName;
            Data.SaveProfile(myProfile);
        }
        else
        {
            PhotonNetwork.LocalPlayer.NickName = "Player" + Random.Range(100, 10000);
            PhotonNetwork.ConnectUsingSettings();
            myProfile.username = PhotonNetwork.LocalPlayer.NickName;
            Data.SaveProfile(myProfile);
        }

        
    }

    public void OnCreateRommButtonClicked()
    {
        string roomName = roomNameInputField.text;
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room " + Random.Range(1000, 10000);
        }

        RoomOptions roomOptions = new RoomOptions();

        string maxPlayers = maxPlayerInputField.text;

        if (string.IsNullOrEmpty(maxPlayers))
        {
            roomOptions.MaxPlayers = (byte)10;

        }
        else
        {
            roomOptions.MaxPlayers = (byte)int.Parse(maxPlayerInputField.text);

        }


        PhotonNetwork.CreateRoom(roomName, roomOptions);

    }


    public void OnCancelbuttonClicked()
    {
        ActivatePanel(gameoptionsUIPanel.name);
    }

    public void OnShowRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }

        ActivatePanel(RoomList_UI_Panel.name);
    }

    public void OnJoinRandomRoomButtonClicked()
    {
        ActivatePanel(JoinRandomRoom_UI_Panel.name);
        PhotonNetwork.JoinRandomRoom();
    }


    public void OnStartGameButtonClicked()
    {       

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Game");
        }

    }


    #endregion


    #region Photon Callbacks

    public override void OnConnected()
    {
        Debug.Log("Connected to internet");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is connected to Photon");
        ActivatePanel(gameoptionsUIPanel.name);
    }


    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " is created.");
    }

    public override void OnJoinedRoom()
    {

        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name);
        ActivatePanel(InsideRoom_UI_Panel.name);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
        else
        {
            startGameButton.SetActive(false);
        }

        roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + "       Players/Max players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        if(playerListGameObjects == null)
        {
            playerListGameObjects = new Dictionary<int, GameObject>();

        }


        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerListGameObject = Instantiate(playerListPrefab);
            playerListGameObject.transform.SetParent(playerListContent.transform);
            playerListGameObject.transform.localScale = Vector3.one;
            playerListGameObject.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0f);

            playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;

            //if player is me say you
            if(player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
            }
            else
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);

            }


            playerListGameObjects.Add(player.ActorNumber, playerListGameObject);

        }



    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {

        ClearRoomListView();

        foreach (RoomInfo room in roomList)
        {
            Debug.Log(room.Name);

            if (!room.IsOpen || !room.IsVisible || room.RemovedFromList) 
            {
                if (catchedRoomList.ContainsKey(room.Name))
                {
                    catchedRoomList.Remove(room.Name);
                }
            }
            else
            {
                if (catchedRoomList.ContainsKey(room.Name))
                {
                    catchedRoomList[room.Name] = room;
                }
                else
                {
                    catchedRoomList.Add(room.Name, room);

                }
            }
        }

        foreach(RoomInfo room in catchedRoomList.Values)
        {
            GameObject roomListEntryGameobject = Instantiate(roomListPrefab);
            roomListEntryGameobject.transform.SetParent(roomListParent.transform);

            roomListEntryGameobject.transform.localScale = Vector3.one;
            roomListEntryGameobject.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0f);


            roomListEntryGameobject.transform.Find("RoomNameText").GetComponent<Text>().text = room.Name;
            roomListEntryGameobject.transform.Find("RoomPlayersText").GetComponent<Text>().text = room.PlayerCount + " /" + room.MaxPlayers;
            roomListEntryGameobject.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(() => OnJoinRoomButtonClicked(room.Name));

            roomListGameobjects.Add(room.Name, roomListEntryGameobject);

        }

    }


    public override void OnLeftLobby()
    {
        ClearRoomListView();
        catchedRoomList.Clear();

    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {

        roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + "       Players/Max players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;


        GameObject playerListGameObject = Instantiate(playerListPrefab);
        playerListGameObject.transform.SetParent(playerListContent.transform);
        playerListGameObject.transform.localScale = Vector3.one;
        playerListGameObject.transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0f);

        playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = newPlayer.NickName;

        //if player is me say "you"
        if (newPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
        }
        else
        {
            playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);

        }


        playerListGameObjects.Add(newPlayer.ActorNumber, playerListGameObject);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {

        roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + "       Players/Max players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;


        Destroy(playerListGameObjects[otherPlayer.ActorNumber].gameObject);
        playerListGameObjects.Remove(otherPlayer.ActorNumber);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
        


    }

    public override void OnLeftRoom()
    {
        ActivatePanel(gameoptionsUIPanel.name);

        foreach (GameObject playerListGameobject in playerListGameObjects.Values)
        {
            Destroy(playerListGameobject);
        }

        playerListGameObjects.Clear();
        playerListGameObjects = null;

    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);

        string roomName = "Room" + Random.Range(1000, 10000);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 20;

        PhotonNetwork.CreateRoom(roomName, roomOptions);

    }


    #endregion


    #region Public Methods


    public void ActivatePanel(string panelToBeActivated)
    {
        login_uiPanel.SetActive(panelToBeActivated.Equals(login_uiPanel.name));
        gameoptionsUIPanel.SetActive(panelToBeActivated.Equals(gameoptionsUIPanel.name));
        CreateRoom_UI_Panel.SetActive(panelToBeActivated.Equals(CreateRoom_UI_Panel.name));
        InsideRoom_UI_Panel.SetActive(panelToBeActivated.Equals(InsideRoom_UI_Panel.name));
        RoomList_UI_Panel.SetActive(panelToBeActivated.Equals(RoomList_UI_Panel.name));
        JoinRandomRoom_UI_Panel.SetActive(panelToBeActivated.Equals(JoinRandomRoom_UI_Panel.name));

    }


    #endregion

    #region Private Methods

    private void OnJoinRoomButtonClicked(string _roomName)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        PhotonNetwork.JoinRoom(_roomName);
    }

    void ClearRoomListView()
    {
        foreach (var roomListGameObject in roomListGameobjects.Values)
        {
            Destroy(roomListGameObject);
        }

        roomListGameobjects.Clear();
    }


    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        ActivatePanel(gameoptionsUIPanel.name);
    }


    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }


    #endregion



}
