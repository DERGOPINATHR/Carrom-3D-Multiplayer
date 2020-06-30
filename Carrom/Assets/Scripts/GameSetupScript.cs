using Assets.Scripts;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSetupScript : MonoBehaviourPunCallbacks
{
    public GameType GameType;

    private PhotonView PhotonView;
    [SerializeField]
    public double MaxPlayerWaitTime = 30;
    [SerializeField]
    public double PlayerWaitTime = 0;
    [SerializeField]
    public int PlayerCount = 0;
    [SerializeField]
    public int MaxPlayers;
    [SerializeField]
    public bool ReadyToStart = false;
    [SerializeField]
    public bool Started = false;
    [SerializeField]
    public Player PlayerOne;
    [SerializeField]
    public Player PlayerTwo;

    public Vector3 StrigerPosPlayerOne = new Vector3(0, 10f, -12);
    public Vector3 StrigerPosPlayerTwo = new Vector3(0, 10f, 11);

    public Camera CamObj;
    [SerializeField]
    public GameObject ScoringCoins, ScoreCollider, Striger;

    public PlayerType Turn;
    public bool CurrentPlayerScored = false;
    public bool QueenScored = false;
    public int QueenScoredTurnCount = 0;
    public string RoomName = null;

    private static GameSetupScript Instance;

    public float DisconnectedTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        GameStatusScript.getInstance().MatchStatus = MatchStatus.NOTREADY;
        GameType = GameStatusScript.getInstance().GameType;
        PhotonView = GetComponent<PhotonView>();
        CamObj = Camera.main;
        setupGame();
    }

    /*  public override void OnEnable()
      {
          //PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
      }*/



    /*public override void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }*/



    public static GameSetupScript GetInstance()
    {
        return Instance;
    }

    void setupGame()
    {
        if (GameType == GameType.SinglePlayer)
        {
            // Destroy
            Destroy(this);
        }
        else if (GameType == GameType.MultiPlayer)
        {
            if (PhotonNetwork.IsConnectedAndReady)
            {
                updatePlayerCount();
                PhotonNetwork.Instantiate("PlayerPrefab", new Vector3(0, 0, 0), Quaternion.identity);
                setupGameCamera();
            }
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonView.RPC("notifyRPC", RpcTarget.All,"Waiting for player");
            }
        }
    }

    private void setupGameCamera()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Setup Camera to be Player 1
            CamObj.transform.position = new Vector3(0, 70, -35);
            CamObj.transform.rotation = Quaternion.Euler(60, 0, 0);
        }
        else
        {
            // Setup Camera to be Player 2
            CamObj.transform.position = new Vector3(0, 70, 35);
            CamObj.transform.rotation = Quaternion.Euler(60, 180, 0);
        }
    }

    private void updatePlayerCount()
    {
        PlayerCount = PhotonNetwork.PlayerList.Length;
        MaxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;

        if (PlayerCount == MaxPlayers)
        {
            ReadyToStart = true;
        }
        else if (PlayerCount < MaxPlayers)
        {
            ReadyToStart = false;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        updatePlayerCount();
    }

    // Update is called once per frame
    void Update()
    {
        if (!Started)
        {
            checkGameReady();
        }
    }

    private void SetupSpawn()
    {
        ScoringCoins = PhotonNetwork.Instantiate("ScoringCoins", new Vector3(0, 5.2f, 0), Quaternion.identity);
        ScoreCollider = PhotonNetwork.Instantiate("ScoreCollider", new Vector3(0, 2f, 0), Quaternion.identity);
        Striger = PhotonNetwork.Instantiate("Striger", new Vector3(0, 0, 0), Quaternion.Euler(90, 0, 0));
    }

    void SetupTurn()
    {
        System.Random randomObj = new System.Random();
        int randomVal = randomObj.Next(0, 2);
        var t = (PlayerType)randomVal;
        PhotonView.RPC("setupStrigerRPC", RpcTarget.All, t);
        PhotonView.RPC("updateTurnRPC", RpcTarget.All, t);
        PhotonView.RPC("notifyRPC", RpcTarget.All, t == PlayerType.Player1 ? "Turn: Player 1" : "Turn: Player 2");
    }

    [PunRPC]
    private void setupStrigerRPC(PlayerType turn)
    {
        if (Striger != null)
        {
            Striger.GetComponent<PhotonView>().RequestOwnership();
            Striger.GetComponent<StrigerController>().setStriger(turn);
        }
    }

    [PunRPC]
    void updateTurnRPC(PlayerType turn)
    {
        Turn = turn;
        GameStatusScript.getInstance().Turn = turn;
    }


    [PunRPC]
    void notifyRPC(string Msg)
    {
        StartCoroutine(NotifyCorotine(Msg));
    }

    private IEnumerator NotifyCorotine(String msg)
    {
        if (HUDController.GetInstance() != null)
        {
            HUDController.GetInstance().PushNotification(msg);
            yield break;
        }
        else
        {
            yield return null;
            StartCoroutine(NotifyCorotine(msg));
        }

    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (GameStatusScript.getInstance().MatchStatus == MatchStatus.INGAME)
        {
            PhotonNetwork.ReconnectAndRejoin();
        }
    }

    private void checkGameReady()
    {
        if (ReadyToStart && !Started)
        {
            ClockScript.getInstance().stopTimer();
            if (PlayerOne == null || PlayerTwo == null)
            {
                PlayerOne = PhotonNetwork.PlayerList[0];
                PlayerTwo = PhotonNetwork.PlayerList[1];
            }
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                ClockScript.getInstance().startTimer(false);
                SetupSpawn();
                SetupTurn();

            }
            RoomName = PhotonNetwork.CurrentRoom.Name;
            Started = true;
            GameStatusScript.getInstance().MatchStatus = MatchStatus.INGAME;
        }
        else if (!ReadyToStart && !Started)
        {
            PlayerWaitTime += Time.deltaTime;
            if (PlayerWaitTime >= MaxPlayerWaitTime)
            {
                ClockScript.getInstance().stopTimer();
                PlayerWaitTime = 0;
                PhotonNetwork.LeaveRoom();
                SceneManager.LoadScene(0);
            }
        }
    }

    public void respawnQueen()
    {
        ScoringCoins.transform.Find("Queen").gameObject.transform.position = new Vector3(0, 7, 0);
        ScoringCoins.transform.Find("Queen").gameObject.SetActive(true);
    }

    public bool AllScored()
    {
        for (int i = 0; i < ScoringCoins.transform.Find("WhiteCoins").childCount; i++)
        {
            if (ScoringCoins.transform.Find("WhiteCoins").GetChild(i).gameObject.activeSelf)
            {
                return false;
            }
        }
        for (int i = 0; i < ScoringCoins.transform.Find("BlackCoins").childCount; i++)
        {
            if (ScoringCoins.transform.Find("BlackCoins").GetChild(i).gameObject.activeSelf)
            {
                return false;
            }
        }
        if (ScoringCoins.transform.Find("Queen").gameObject.activeSelf)
        {
            return false;
        }
        return true;
    }

    public void setScoringCoinInActive(string Name, ScoreCoinType type)
    {
        switch (type)
        {
            case ScoreCoinType.White:
                ScoringCoins.transform.Find("WhiteCoins").Find(Name).gameObject.SetActive(false);
                break;
            case ScoreCoinType.Black:
                ScoringCoins.transform.Find("BlackCoins").Find(Name).gameObject.SetActive(false);
                break;
            case ScoreCoinType.Queen:
                ScoringCoins.transform.Find(Name).gameObject.SetActive(false);
                break;
            case ScoreCoinType.NOTSET:
                break;
            default:
                break;
        }
    }
}
