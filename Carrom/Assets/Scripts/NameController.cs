using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameController : MonoBehaviourPun
{
    public string PlayerOneName = "Nil";
    public string PlayerTwoName = "Nil";

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.LocalPlayer.NickName = GameStatusScript.getInstance().LocalUserName;
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.PlayerList.Length == 2)
        {
            if (PlayerOneName != PhotonNetwork.PlayerList[0].NickName)
            {
                PlayerOneName = PhotonNetwork.PlayerList[0].NickName;
                GameStatusScript.getInstance().PlayerNameOne = PlayerOneName;
            }
            if (PlayerTwoName != PhotonNetwork.PlayerList[1].NickName)
            {
                PlayerTwoName = PhotonNetwork.PlayerList[1].NickName;
                GameStatusScript.getInstance().PlayerNameTwo = PlayerTwoName;
            }
        }
        
    }
}
