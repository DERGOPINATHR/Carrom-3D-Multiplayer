using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStatusScript : MonoBehaviour
{
    public string PlayerNameOne;
    public string PlayerNameTwo;
    public string JoinAddress;
    public int Score_Player_1;
    public int Score_Player_2;
    public DateTime Time;
    public GameType GameType;
    public PlayerType Turn;
    public MatchStatus MatchStatus;
    public MultiType MultiType;
    public bool CurrentPlayerScored;
    public bool ShowMatchResult = false;
    public MatchResult MatchResult;
    public bool StrigerMoving = false;
    public bool isUserPlayerOne = false;
    public string LocalUserName;

    private static GameStatusScript Instance;

    private void Start()
    {
        if (Instance != null)
        {
            GameObject.Destroy(this.gameObject);
            return;
        }
        Instance = this;
        GameObject.DontDestroyOnLoad(this.gameObject);
    }


    public static void loadMenu()
    {
        SceneManager.LoadScene(GameScene.MainMenu.ToString());
    }

    public static void loadGame()
    { 
        SceneManager.LoadScene(GameScene.Game.ToString());
    }

    public void resetInGameParams()
    {
        PlayerNameOne = string.Empty;
        PlayerNameTwo = string.Empty;
        Score_Player_1 = 0;
        Score_Player_2 = 0;
        Time = DateTime.Now;
        GameType = GameType.NA;
    }

    public static GameStatusScript getInstance()
    {
        return Instance;
    }
}
