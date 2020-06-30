using Assets.Scripts;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockScript : MonoBehaviour
{
    public PhotonView PhotonView;
    public float clock;

    public string TimeString = string.Empty;

    public bool GameTimerStart = false;
    public bool WaitTimerStart = false;

    public GameType GameType;

    private static ClockScript Instance;

    public void stopTimer()
    {
        clock = 0;
        GameTimerStart = false;
        WaitTimerStart = false;
    }

    public void startTimer(bool playerWait)
    {
        if (playerWait)
        {
            clock = 30;
            WaitTimerStart = true;
            GameTimerStart = false;
        }
        else
        {
            clock = 0;
            WaitTimerStart = false;
            GameTimerStart = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        GameType = GameStatusScript.getInstance().GameType;
        Instance = this;
        clock = 0;
        if (GameType == GameType.MultiPlayer)
        {
            PhotonView = GetComponent<PhotonView>();
            startTimer(true);
        }
    }

    public static ClockScript getInstance()
    {
        return Instance;
    }

    // Update is called once per frame
    void Update()
    {
        float clockVal = clock;
        if (GameTimerStart)
        {
            clockVal += Time.deltaTime;
        }
        if (WaitTimerStart)
        {
            clockVal -= Time.deltaTime;
        }
        if (GameType == GameType.MultiPlayer)
        {
            if (PhotonNetwork.IsConnectedAndReady)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonView.RPC("SetTimeStringRPC", RpcTarget.All, clockVal);
                }
            }
            
        }
        else if (GameType == GameType.SinglePlayer)
        {
            SetTimeStringRPC(clockVal);
        }
        
    }

    [PunRPC]
    void SetTimeStringRPC(float inValue)
    {
        clock = inValue;
        TimeString = getTimeString();
        GetComponent<TMPro.TextMeshProUGUI>().text = TimeString;
    }

    private string getTimeString()
    {
        int hours = 0;
        int minuites = 0;
        int seconds = (int)clock;
        minuites = seconds / 60;
        hours = minuites / 60;
        while (seconds >= 60)
        {
            seconds -= 60;
        }
        while (minuites >= 60)
        {
            minuites -= 60;
        }
        return string.Format("{0}:{1}:{2}", hours, minuites, seconds);
    }
}
