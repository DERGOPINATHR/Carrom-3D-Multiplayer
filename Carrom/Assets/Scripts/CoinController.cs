using Assets.Scripts;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinController : MonoBehaviourPun
{
    private List<GameObject> WhiteDiscs, BlackDiscs;
    private GameObject Queen;
    private static CoinController Instance;

    public GameType GameType;

    public static CoinController GetInstance()
    {
        return Instance;
    }
    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        GameType = GameStatusScript.getInstance().GameType;
        WhiteDiscs = new List<GameObject>();
        BlackDiscs = new List<GameObject>();

        var whitecoins = transform.Find("WhiteCoins");
        for (int i = 0; i < whitecoins.childCount; i++)
        {
            WhiteDiscs.Add(whitecoins.GetChild(i).gameObject);
        }
        var blackcoins = transform.Find("BlackCoins");
        for (int i = 0; i < blackcoins.childCount; i++)
        {
            BlackDiscs.Add(blackcoins.GetChild(i).gameObject);
        }
        Queen = transform.Find("Queen").gameObject;
    }

    private void requestPermission()
    {
        if (GetComponent<PhotonView>().Owner != PhotonNetwork.NetworkingClient.LocalPlayer)
        {
            GetComponent<PhotonView>().RequestOwnership();
        }
    }

    public void toggleAllY(bool value)
    {
        foreach (var item in WhiteDiscs)
        {
            item.GetComponent<Rigidbody>().constraints = value ? RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.FreezeRotation;
        }
        foreach (var item in BlackDiscs)
        {
            item.GetComponent<Rigidbody>().constraints = value ? RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.FreezeRotation;
        }
        Queen.GetComponent<Rigidbody>().constraints = value ? RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.FreezeRotation;
    }


    public bool AllSleeping()
    {
        if (GameType == GameType.MultiPlayer)
        {
            requestPermission();
        }
        
        foreach (var item in WhiteDiscs)
        {
            if (!item.GetComponent<Rigidbody>().IsSleeping())
            {
                return false;
            }
        }
        foreach (var item in BlackDiscs)
        {
            if (!item.GetComponent<Rigidbody>().IsSleeping())
            {
                return false;
            }
        }
        if (!Queen.GetComponent<Rigidbody>().IsSleeping())
        {
            return false;
        }
        return true;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
