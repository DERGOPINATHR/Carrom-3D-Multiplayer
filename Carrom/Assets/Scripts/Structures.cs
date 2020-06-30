using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    [Serializable]
    public class UserInfo
    {
        public string username { get; set; }
        public int exp { get; set; }
    }

    public static class RaiseEventCodes
    {
        public static byte ToggleTurnEvent = 100;
        public static byte CoinScoredEvent = 101;
        public static byte ExitGame = 102;
    }

    public struct MatchResult
    {
        public WinCase Winner;
        public int ScoreOne;
        public int ScoreTwo;
    }

    [Serializable]
    public class CoinScored
    {
        public PlayerType Player;
        public ScoreCoinType CoinType;
        public string CoinName;
    }

    public class PossibleShot
    {
        public Vector3 TransformLocation { get; set; }
        public Vector3 ThrowDirection { get; set; }
        public ScoreCoinType HitCoinType { get; set; }
        public float HitDistance { get; set; }
    }

   /* public class ScoredCoins : MessageBase
    {
        public ScoreCoinType Type;
    }

    public class SpawnMsg : MessageBase
    {
        public bool SpawnReady;
    }

    public class ClientDcMsg : MessageBase
    {
        public string Client;
    }

    public class ToggleTurnMsg : MessageBase
    {
        public bool Toggle;
    }

    public class ToggleScoreMsg : MessageBase
    {
        public PlayerType Player;
        public ScoreCoinType Type;
        public string CoinName;
    }

    public class SetTurnMsg : MessageBase
    {
        public PlayerType Turn;
    }

    public class SetScoreMsg : MessageBase
    {
        public int Score_One;
        public int Score_Two;
    }

    public class DisplayNotifMsg : MessageBase
    {
        public string Msg;
        public bool keepOpen;
    }

    public class PlayerNameMsg : MessageBase
    {
        public string Name;
        public PlayerType Player;
    }


    public class SetHUDNamesMsg : MessageBase
    {
        public string PlayerOne;
        public string PlayerTwo;
    }

    public class SendMatchResultMsg : MessageBase
    {
        public WinCase Winner;
        public int ScoreOne;
        public int ScoreTwo;
    }*/
}
