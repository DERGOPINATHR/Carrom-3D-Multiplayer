using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public enum GameType { SinglePlayer, MultiPlayer, NA };
    public enum MultiType { Host, Join };
    public enum ScoreCoinType { White, Black, Queen, NOTSET };
    public enum GameScene { MainMenu, Game };
    public enum PlayerType { Player1, Player2 };
    public enum MatchStatus { NOTREADY, READY, INGAME, END };
    public enum WinCase { Player1, Player2, Draw };

}
