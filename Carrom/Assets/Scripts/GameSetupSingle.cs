using Assets.Scripts;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSetupSingle : MonoBehaviour
{
    public GameType GameType;

    public Camera CamObj;

    private static GameSetupSingle Instance;

    // public GameObject ScoringCoinsPrefab, ScoreColliderPrefab, StrigerPrefab;

    public GameObject ScoringCoins, ScoreCollider, Striger;

    public PlayerType Turn;

    public int ScoreOne = 0;
    public int ScoreTwo = 0;

    public bool CurrentPlayerScored;

    public bool QueenScored;

    public int QueenScoredTurnCount;

    public bool TimerStarted = false;

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
        CamObj = Camera.main;
        setupGame();
    }

    public static GameSetupSingle GetInstance()
    {
        return Instance;
    }

    private void setupGame()
    {
        if (GameType == GameType.SinglePlayer)
        {
            setupGameCamera();
            SetupSpawn();
            SetupTurn();
            SetupNames();
        }
        else if (GameType == GameType.MultiPlayer)
        {
            Destroy(this);
        }
    }

    private void SetupNames()
    {
        if (GameStatusScript.getInstance() != null)
        {
            GameStatusScript.getInstance().PlayerNameOne = GameStatusScript.getInstance().LocalUserName;
            GameStatusScript.getInstance().PlayerNameTwo = "Computer";
        }
    }

    void updateTurnRPC(PlayerType turn)
    {
        Turn = turn;
        GameStatusScript.getInstance().Turn = turn;
    }

    private void SetupTurn()
    {
        System.Random randomObj = new System.Random();
        int randomVal = randomObj.Next(0, 2);
        var t = (PlayerType)randomVal;

        Striger.GetComponent<StrigerControllerSingle>().setStriger(t);
        updateTurnRPC(t);
        notifyRPC(t == PlayerType.Player1 ? "Turn: Player 1" : "Turn: Player 2");
        if (t == PlayerType.Player2)
        {
            // Player Computer turn here
            StartCoroutine(Striger.GetComponent<StrigerControllerSingle>().ComputerMovePossibleShot());
        }
    }

    public void EndGameRPC(MatchResult matchResult)
    {
        GameStatusScript.getInstance().MatchStatus = Assets.Scripts.MatchStatus.END;
        GameStatusScript.getInstance().Score_Player_1 = matchResult.ScoreOne;
        GameStatusScript.getInstance().Score_Player_2 = matchResult.ScoreTwo;
        GameStatusScript.getInstance().MatchResult = matchResult;
        GameStatusScript.getInstance().ShowMatchResult = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
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

    public void ToggleTurn()
    {
        var turn = Turn;
        if (AllScored())
        {
            /// Match result throw here...
            //SendMatchResultMsg resultMsg = new SendMatchResultMsg();
            var Winner = ScoreOne > ScoreTwo ? WinCase.Player1 : WinCase.Player2;
            if (ScoreOne == ScoreTwo)
            {
                Winner = WinCase.Draw;
            }
            var matchResult = new MatchResult();
            matchResult.Winner = Winner;
            matchResult.ScoreOne = ScoreOne;
            matchResult.ScoreTwo = ScoreTwo;
            //Call endgme local here
            //PhotonView.RPC("EndGameRPC", RpcTarget.All, matchResult);
            EndGameRPC(matchResult);
            return;
        }
        if (QueenScored)
        {
            QueenScoredTurnCount += 1;
            if (QueenScoredTurnCount == 2)
            {
                QueenScored = false;
                QueenScoredTurnCount = 0;
                if (!CurrentPlayerScored)
                {
                    if (turn == PlayerType.Player1)
                    {
                        ScoreOne -= 50;
                        GameSetupScript.GetInstance().respawnQueen();
                        // Call update score here
                        //PhotonView.RPC("UpdateScoreRPC", RpcTarget.All, ScoreOne, ScoreTwo);
                        UpdateScoreRPC(ScoreOne, ScoreTwo);
                    }
                }
            }
        }
        switch (turn)
        {
            case PlayerType.Player1:
                turn = CurrentPlayerScored ? PlayerType.Player1 : PlayerType.Player2;

                break;
            case PlayerType.Player2:
                turn = CurrentPlayerScored ? PlayerType.Player2 : PlayerType.Player1;
                break;
            default:
                break;
        }
        if (CurrentPlayerScored)
        {
            CurrentPlayerScored = false;
        }
        if (Striger != null)
        {
            // Set Striger here
            Striger.GetComponent<StrigerControllerSingle>().setStriger(turn);
        }

        //Upodate turn here
        //PhotonView.RPC("updateTurnRPC", RpcTarget.All, turn);
        updateTurnRPC(turn);
        notifyRPC(turn == PlayerType.Player1 ? "Turn: Player 1" : "Turn: Player 2");
        if (Turn == PlayerType.Player2)
        {
            // Player Computer turn here
            StartCoroutine(Striger.GetComponent<StrigerControllerSingle>().ComputerMovePossibleShot());
        }
    }

    private void SetupSpawn()
    {
        ScoringCoins = Instantiate(ScoringCoins, new Vector3(0, 5.2f, 0), Quaternion.identity);
        ScoreCollider = Instantiate(ScoreCollider, new Vector3(0, 2f, 0), Quaternion.identity);
        Striger = Instantiate(Striger, new Vector3(0, 0, 0), Quaternion.Euler(90, 0, 0));
    }

    private void setupGameCamera()
    {
        // Setup Camera to be Player 1
        CamObj.transform.position = new Vector3(0, 70, -35);
        CamObj.transform.rotation = Quaternion.Euler(60, 0, 0);
    }

    public void updateScore(Dictionary<int, object> scoredData)
    {
        Debug.Log("Score Event raised");
        if ((ScoreCoinType)scoredData[2] == ScoreCoinType.Queen)
        {
            QueenScored = true;
        }
        CurrentPlayerScored = true;
        switch ((PlayerType)scoredData[0])
        {
            case PlayerType.Player1:
                ScoreOne += GetScoreByType((ScoreCoinType)scoredData[2]);
                break;
            case PlayerType.Player2:
                ScoreTwo += GetScoreByType((ScoreCoinType)scoredData[2]);
                break;
            default:
                break;
        }
        setScoringCoinInActive((string)scoredData[1], (ScoreCoinType)scoredData[2]);
        UpdateScoreRPC(ScoreOne, ScoreTwo);
    }

    void UpdateScoreRPC(int scoreOne, int scoreTwo)
    {
        GameStatusScript.getInstance().Score_Player_1 = scoreOne;
        GameStatusScript.getInstance().Score_Player_2 = scoreTwo;
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

    private int GetScoreByType(ScoreCoinType type)
    {
        switch (type)
        {
            case ScoreCoinType.White:
                return 5;
            case ScoreCoinType.Black:
                return 10;
            case ScoreCoinType.Queen:
                return 50;
            case ScoreCoinType.NOTSET:
                return 0;
            default:
                return 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!TimerStarted)
        {
            if (ClockScript.getInstance() != null)
            {
                ClockScript.getInstance().startTimer(false);
                TimerStarted = true;
            }
        }
    }

    void notifyRPC(string Msg)
    {
        HUDController.GetInstance().PushNotification(Msg);
    }
}
