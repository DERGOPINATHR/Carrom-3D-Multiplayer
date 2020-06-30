using Assets.Scripts;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaisedEventsController : MonoBehaviourPun
{
    public PlayerType Turn;

    public bool CurrentPlayerScored = false;

    public GameObject Striger;

    public GameType GameType;

    private PhotonView PhotonView;

    public int ScoreOne = 0, ScoreTwo = 0;

    public bool QueenScored = false;

    public int QueenScoredTurnCount = 0;

    public Player PlayerOne;
    public Player PlayerTwo;

    // Start is called before the first frame update
    void Start()
    {
        GameType = GameStatusScript.getInstance().GameType;
        if (GameType == GameType.SinglePlayer)
        {
            Destroy(this);
        }
        PhotonView = GetComponent<PhotonView>();
        if (GameSetupScript.GetInstance() != null)
        {
            Striger = GameSetupScript.GetInstance().Striger;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerOne == null && PlayerTwo == null)
        {
            if (PhotonNetwork.PlayerList.Length == 2)
            {
                PlayerOne = PhotonNetwork.PlayerList[0];
                PlayerTwo = PhotonNetwork.PlayerList[1];
            }
        }
        if (GameSetupScript.GetInstance() != null && Striger == null)
        {
            Striger = GameSetupScript.GetInstance().Striger;
        }/**/
        if (GameStatusScript.getInstance() != null)
        {
            Turn = GameStatusScript.getInstance().Turn;
            //CurrentPlayerScored = ScoreController.CurrentPlayerScored;
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }

    [PunRPC]
    void updateTurnRPC(PlayerType turn)
    {
        Turn = turn;
        GameStatusScript.getInstance().Turn = turn;
    }

    private void NetworkingClient_EventReceived(EventData obj)
    {

        if (obj.Code == RaiseEventCodes.ToggleTurnEvent)
        {
            Debug.Log("ToggleTurn Event Raised");
            var turn = Turn;
            if (GameSetupScript.GetInstance().AllScored())
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
                PhotonView.RPC("EndGameRPC", RpcTarget.All, matchResult);
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
                            PhotonView.RPC("UpdateScoreRPC", RpcTarget.All, ScoreOne, ScoreTwo);
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
                    /*if (GameType == GameType.MultiPlayer)
                    {
                        AssignStriger(Turn);
                    }
                    turnMsg = new SetTurnMsg();
                    turnMsg.Turn = Turn;
                    NetworkServer.SendToAll<SetTurnMsg>(turnMsg, SetTurnMsgId);*/
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
                Striger.GetComponent<PhotonView>().RequestOwnership();
                StartCoroutine(SetStrigerRoutine(turn));

            }

            PhotonView.RPC("updateTurnRPC", RpcTarget.All, turn);
            PhotonView.RPC("notifyRPC", RpcTarget.All, turn == PlayerType.Player1 ? "Turn: Player 1" : "Turn: Player 2");
        }

        if (obj.Code == RaiseEventCodes.CoinScoredEvent)
        {
            Debug.Log("Score Event raised");
            var scoredData = obj.CustomData as Dictionary<int, object>;
            if ((ScoreCoinType)scoredData[2] == ScoreCoinType.Queen)
            {
                QueenScored = true;
            }
            switch ((PlayerType)scoredData[0])
            {
                case PlayerType.Player1:
                    ScoreOne += GetScoreByType((ScoreCoinType)scoredData[2]);
                    CurrentPlayerScored = true;
                    break;
                case PlayerType.Player2:
                    ScoreTwo += GetScoreByType((ScoreCoinType)scoredData[2]);
                    CurrentPlayerScored = true;
                    break;
                default:
                    break;
            }
            GameSetupScript.GetInstance().setScoringCoinInActive((string)scoredData[1], (ScoreCoinType)scoredData[2]);
            PhotonView.RPC("UpdateScoreRPC", RpcTarget.All, ScoreOne, ScoreTwo);
            /*if (isScoringCoinInActive(msg.CoinName, msg.Type))
            {
                return;
            }
            if (!isColorSet())
            {
                setPlayerColor(msg.Player, msg.Type);
            }
            switch (msg.Player)
            {
                case PlayerType.Player1:
                    if (msg.Type == ScoreCoinType.Queen)
                    {
                        Score_One += GetScoreByType(msg.Type);
                    }
                    else
                    {
                        if (isColorSet() && PlayerOneColor == msg.Type)
                        {
                            Score_One += GetScoreByType(msg.Type);
                            CurrentPlayerScored = true;
                        }
                        else if (isColorSet() && PlayerTwoColor == msg.Type)
                        {
                            Score_Two += GetScoreByType(msg.Type);
                        }
                    }
                    break;
                case PlayerType.Player2:
                    if (msg.Type == ScoreCoinType.Queen)
                    {
                        Score_Two += GetScoreByType(msg.Type);
                    }
                    else
                    {
                        if (isColorSet() && PlayerTwoColor == msg.Type)
                        {
                            Score_Two += GetScoreByType(msg.Type);
                            CurrentPlayerScored = true;
                        }
                        else if (isColorSet() && PlayerOneColor == msg.Type)
                        {
                            Score_One += GetScoreByType(msg.Type);
                        }
                    }
                    break;
                default:
                    break;
            }

            setScoringCoinInActive(msg.CoinName, msg.Type);

            SetScoreMsg scoreMsg = new SetScoreMsg();
            scoreMsg.Score_One = Score_One;
            scoreMsg.Score_Two = Score_Two;
            NetworkServer.SendToAll<SetScoreMsg>(scoreMsg, SetScoreMsgId);*/
        }

        if (obj.Code == RaiseEventCodes.ExitGame)
        {
            var player = (obj.CustomData as Dictionary<int, object>)[0] as Player;
            WinCase Winner;
            Winner = WinCase.Draw;
            if (PlayerOne == player)
            {
                Winner = WinCase.Player2;

            }
            if (PlayerTwo == player)
            {
                Winner = WinCase.Player1;

            }
            PhotonView.RPC("EndGameRPC", RpcTarget.All, Winner == WinCase.Player1 ? "PlayerOne" : "PlayerTwo", ScoreOne, ScoreTwo);
        }
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

    [PunRPC]
    void UpdateScoreRPC(int scoreOne, int scoreTwo)
    {
        GameStatusScript.getInstance().Score_Player_1 = scoreOne;
        GameStatusScript.getInstance().Score_Player_2 = scoreTwo;
    }


    [PunRPC]
    void EndGameRPC(string winner, int scoreOne, int scoreTwo)
    {
        var matchResult = new MatchResult();
        matchResult.Winner = winner == "PlayerOne" ? WinCase.Player1 : WinCase.Player2;
        matchResult.ScoreOne = scoreOne;
        matchResult.ScoreTwo = scoreTwo;
        GameStatusScript.getInstance().MatchStatus = Assets.Scripts.MatchStatus.END;
        GameStatusScript.getInstance().Score_Player_1 = matchResult.ScoreOne;
        GameStatusScript.getInstance().Score_Player_2 = matchResult.ScoreTwo;
        if (PhotonNetwork.IsMasterClient)
        {
            GameStatusScript.getInstance().isUserPlayerOne = true;
        }
        else
        {
            GameStatusScript.getInstance().isUserPlayerOne = false;
        }
        GameStatusScript.getInstance().MatchResult = matchResult;
        GameStatusScript.getInstance().ShowMatchResult = true;
        PhotonNetwork.LeaveRoom();
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private IEnumerator SetStrigerRoutine(PlayerType turn)
    {
        if (Striger.GetComponent<PhotonView>().IsMine)
        {
            Striger.GetComponent<StrigerController>().setStriger(turn);
            yield break;
        }
        else
        {
            yield return null;
            StartCoroutine(SetStrigerRoutine(turn));
        }
    }


}

