using Assets.Scripts;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreController : MonoBehaviourPun
{
    public GameType GameType;
    // Start is called before the first frame update
    void Start()
    {
        GameType = GameStatusScript.getInstance().GameType;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerExit(Collider other)
    {
        CoinScored coinScored = new CoinScored();
        coinScored.Player = GameStatusScript.getInstance().Turn;
        if (other.gameObject.transform.parent.name == "WhiteCoins")
        {
            coinScored.CoinType = ScoreCoinType.White;
            coinScored.CoinName = other.gameObject.name;
        }
        if (other.gameObject.transform.parent.name == "BlackCoins")
        {
            coinScored.CoinType = ScoreCoinType.Black;
            coinScored.CoinName = other.gameObject.name;
            // GameStatusScript.getInstance().setScore(score, turn);
        }
        if (other.gameObject.name == "Queen")
        {
            coinScored.CoinType = ScoreCoinType.Queen;
            coinScored.CoinName = other.gameObject.name;
        }
        var obj = new Dictionary<int, object>();
        obj[0] = coinScored.Player;
        obj[1] = coinScored.CoinName;
        obj[2] = coinScored.CoinType;
        if (GameType == GameType.MultiPlayer)
        {
            PhotonNetwork.RaiseEvent(RaiseEventCodes.CoinScoredEvent, obj, new RaiseEventOptions()
            {
                Receivers = ReceiverGroup.MasterClient
            }, SendOptions.SendReliable);
        }
        else if (GameType == GameType.SinglePlayer)
        {
            GameSetupSingle.GetInstance().updateScore(obj);
        }

    }


}
