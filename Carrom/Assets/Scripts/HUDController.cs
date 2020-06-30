using Assets.Scripts;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HUDController : MonoBehaviour
{
    private GameObject NameOneText, NameTwoText, ScoreOneText, ScoreTwoText, DisplayNotificationBox;
    public bool isNotifOpen = false;
    public bool keepNotifOpen = false;
    public float NotificationTime = 0;
    public float NotificationTimeLimit = 4;
    public List<string> Notifications = new List<string>();
    private static HUDController Instance;
    public GameType GameType;
    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        GameType = GameStatusScript.getInstance().GameType;
        NameOneText = gameObject.transform.Find("Player1").gameObject.transform.Find("Player 1 Name").gameObject;
        NameTwoText = gameObject.transform.Find("Player2").gameObject.transform.Find("Player 2 Name").gameObject;
        ScoreOneText = gameObject.transform.Find("Player1").gameObject.transform.Find("ScoreValue 1").gameObject;
        ScoreTwoText = gameObject.transform.Find("Player2").gameObject.transform.Find("ScoreValue 2").gameObject;
        DisplayNotificationBox = transform.Find("UserNotifDisplayBox").gameObject;
    }

    public static HUDController GetInstance()
    {
        return Instance;
    }

    // Update is called once per frame
    void Update()
    {
        NameOneText.GetComponent<TextMeshProUGUI>().text = GameStatusScript.getInstance().PlayerNameOne;
        NameTwoText.GetComponent<TextMeshProUGUI>().text = GameStatusScript.getInstance().PlayerNameTwo;
        ScoreOneText.GetComponent<TMPro.TextMeshProUGUI>().text = GameStatusScript.getInstance().Score_Player_1.ToString();
        ScoreTwoText.GetComponent<TMPro.TextMeshProUGUI>().text = GameStatusScript.getInstance().Score_Player_2.ToString();

        if (isNotifOpen)
        {
            if (!keepNotifOpen)
            {
                if (NotificationTime >= NotificationTimeLimit)
                {
                    NotificationTime = 0;
                    if (Notifications.Count != 0)
                    {
                        string notif = Notifications.First();
                        Notifications.RemoveAt(0);
                        DisplayNotificationBox.transform.GetChild(0).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = notif;
                        DisplayNotificationBox.SetActive(true);
                    }
                    else
                    {
                        DisplayNotificationBox.SetActive(false);
                        isNotifOpen = false;
                        keepNotifOpen = false;
                    }

                }
                else
                {
                    NotificationTime += Time.deltaTime;
                }
            }

        }
    }

    public void PushNotification(string Msg)
    {
        Notifications.Add(Msg);
        DisplayNotificationBox.SetActive(true);
        NotificationTime = 0;
        isNotifOpen = true;
    }

    public void ExitSession()
    {
        if (GameType == GameType.MultiPlayer)
        {
            if (PhotonNetwork.CurrentRoom != null)
            {
                var obj = new Dictionary<int, object>();
                obj[0] = PhotonNetwork.LocalPlayer;
                PhotonNetwork.RaiseEvent(RaiseEventCodes.ExitGame, obj, new RaiseEventOptions()
                {
                    Receivers = ReceiverGroup.MasterClient
                }, SendOptions.SendReliable);
            }
        }
        else if (GameType == GameType.SinglePlayer)
        {
            MatchResult result = new MatchResult()
            {
                ScoreOne = GameStatusScript.getInstance().Score_Player_1,
                ScoreTwo = GameStatusScript.getInstance().Score_Player_2,
                Winner = WinCase.Player2
            };

            GameSetupSingle.GetInstance().EndGameRPC(result);
        }

    }
}
