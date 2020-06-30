using Assets.Scripts;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

public class MenuController : MonoBehaviourPunCallbacks
{
    [SerializeField]
    public GameObject UserInputMenuObject, MainMenuObject, MatchResultObject, JoinAddressObject;

    [SerializeField]
    public UserInfo UserInfo { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        UserInputMenuObject = transform.Find("UsernameInputObj").gameObject;
        MainMenuObject = transform.Find("MainMenu").gameObject;
        MatchResultObject = transform.Find("MatchResult").gameObject;
        JoinAddressObject = transform.Find("MultiPlayerTypesMenu").gameObject.transform.Find("JoinAddressField").gameObject;
        if (GameStatusScript.getInstance() != null)
        {
            if (GameStatusScript.getInstance().ShowMatchResult)
            {
                MainMenuObject.SetActive(false);
                fetch_matchResult();
                MatchResultObject.SetActive(true);
                setGameModeBtnActive(true, false);
            }
        }
        if (FileHandlerScript.hasSavedData())
        {
            if (GameStatusScript.getInstance() != null)
            {
                if (!GameStatusScript.getInstance().ShowMatchResult)
                {
                    MainMenuObject.SetActive(true);
                }
            }
            UserInputMenuObject.SetActive(false);
            UserInfo = FileHandlerScript.getSavedData();
            setGameModeBtnActive(true, false);
        }
        else
        {
            MainMenuObject.SetActive(false);
            UserInputMenuObject.SetActive(true);
        }




        /*if (GameStatusScript.getInstance() != null)
        {
            if (GameStatusScript.getInstance().ShowMatchResult)
            {
                MainMenu.SetActive(false);
                fetch_matchResult();
                MatchResult.SetActive(true);
                setGameModeBtnActive();
            }
        }
        else
        {
            if (FileHandlerScript.hasSavedData())
            {
                MainMenu.SetActive(true);
                UserInputMenu.SetActive(false);
                UserInfo = FileHandlerScript.getSavedData();
                Username = UserInfo.username;
                setGameModeBtnActive();
            }
            else
            {
                MainMenu.SetActive(false);
                UserInputMenu.SetActive(true);
            }
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkController.getInstance() != null)
        {
            if (NetworkController.getInstance().isNetAvailable() && NetworkController.getInstance().ServerConnected)
            {
                setGameModeBtnActive(true, true);
            }
            else
            {
                setGameModeBtnActive(true, false);
            }
        }
    }

    public void StartSingleGame()
    {
        GameStatusScript.getInstance().GameType = GameType.SinglePlayer;
        GameStatusScript.getInstance().Score_Player_1 = 0;
        GameStatusScript.getInstance().LocalUserName = UserInfo.username;
        GameStatusScript.getInstance().Score_Player_2 = 0;
        SceneManager.LoadScene(1);
    }

    public void StartMultiGame()
    {
        GameStatusScript.getInstance().GameType = GameType.MultiPlayer;
        GameStatusScript.getInstance().Score_Player_1 = 0;
        GameStatusScript.getInstance().Score_Player_2 = 0;
        GameStatusScript.getInstance().LocalUserName = UserInfo.username;
        PhotonNetwork.JoinRandomRoom();
    }

    private void fetch_matchResult()
    {
        string winner = "";
        if (GameStatusScript.getInstance().GameType == GameType.MultiPlayer)
        {
            if (GameStatusScript.getInstance().MatchResult.Winner == WinCase.Player1 && GameStatusScript.getInstance().isUserPlayerOne)
            {
                winner = "You Won";
                increment_exp();
            }
            else if (GameStatusScript.getInstance().MatchResult.Winner == WinCase.Player2 && !GameStatusScript.getInstance().isUserPlayerOne)
            {
                winner = "You Won";
                increment_exp();
            }
            else
            {
                winner = "You Lost";
            }
        }
        else
        {

            if (GameStatusScript.getInstance().MatchResult.Winner == WinCase.Player1)
            {
                winner = "You Won";
                increment_exp();
            }
            else if (GameStatusScript.getInstance().MatchResult.Winner == WinCase.Player2)
            {
                winner = "You Lost";
            }
        }
        MatchResultObject.transform.Find("WinCaseDisplay").GetComponent<TextMeshProUGUI>().text = winner;
        MatchResultObject.transform.Find("PlayerOneScore").GetComponent<TextMeshProUGUI>().text = "Player One: " + GameStatusScript.getInstance().MatchResult.ScoreOne.ToString();
        MatchResultObject.transform.Find("PlayerTwoScore").GetComponent<TextMeshProUGUI>().text = "Player Two: " + GameStatusScript.getInstance().MatchResult.ScoreTwo.ToString();
    }

    private void increment_exp()
    {
        if (FileHandlerScript.hasSavedData())
        {
            var userData = FileHandlerScript.getSavedData();
            userData.exp += 25;
            FileHandlerScript.saveData(userData);
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //base.OnJoinRandomFailed(returnCode, message);
        CreateRoom();
    }

    private void CreateRoom()
    {
        int roomNumber = RandomNumber(0, 10000);
        RoomOptions roomOptions = new RoomOptions
        {
            IsOpen = true,
            IsVisible = true,
            MaxPlayers = 2,
            PlayerTtl = 6000,
            EmptyRoomTtl = 60000
        };
        PhotonNetwork.CreateRoom("Room " + roomNumber, roomOptions);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        //base.OnCreateRoomFailed(returnCode, message);
        CreateRoom();
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void setGameModeBtnActive(bool Single, bool Multi)
    {
        MainMenuObject.transform.Find("SingleBtn").gameObject.GetComponent<UnityEngine.UI.Button>().enabled = Single;
        MainMenuObject.transform.Find("MultiBtn").gameObject.GetComponent<UnityEngine.UI.Button>().enabled = Multi;
    }

    public override void OnJoinedRoom()
    {

        //base.OnJoinedRoom();
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(1);
        }
    }

    // Generate a random number between two numbers  
    public int RandomNumber(int min, int max)
    {
        Random random = new Random();
        return random.Next(min, max);
    }

    public void UpdateUserName()
    {
        UserInfo = FileHandlerScript.getSavedData();
    }
}
