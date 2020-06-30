using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkController : MonoBehaviourPunCallbacks
{
    public bool InternetAvailable = false;
    public bool ServerConnected = false;
    public double InternetCheckTime = 0;
    public double InternetCheckTimeLimit = 10;

    private static NetworkController Instance;
    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        Instance = this;
        PhotonNetwork.AutomaticallySyncScene = true;
        checkInternet();
    }

    public static NetworkController getInstance()
    {
        return Instance;
    }

    private void Awake()
    {
        checkInternet();
    }

    public void checkInternet()
    {
        if (isNetAvailable())
        {
            InternetAvailable = true;
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                //PhotonNetwork.OfflineMode = false;
                PhotonNetwork.ConnectUsingSettings();
            }
            else
            {
                if (!ServerConnected)
                {
                    ServerConnected = true;
                }
            }
        }
        else
        {
            InternetAvailable = false;
            ServerConnected = false;
            if(PhotonNetwork.IsConnectedAndReady)
            {
                PhotonNetwork.Disconnect();
            }
            if (!PhotonNetwork.OfflineMode && !PhotonNetwork.IsConnectedAndReady)
            {
                //PhotonNetwork.OfflineMode = true;
            }
        }
    }

    public override void OnConnectedToMaster()
    {
        if (PhotonNetwork.CloudRegion != null && PhotonNetwork.CloudRegion.Trim() != string.Empty && PhotonNetwork.IsConnectedAndReady)
        {
            //PhotonNetwork.OfflineMode = false;
            Debug.Log("Connected to: " + PhotonNetwork.CloudRegion + " Server");
            ServerConnected = true;
            return;
        }
        Debug.Log("Fake connection report");

    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        ServerConnected = false;
        switch (cause)
        {
            case DisconnectCause.None:
                break;
            case DisconnectCause.ExceptionOnConnect:

                break;
            case DisconnectCause.Exception:
                break;
            case DisconnectCause.ServerTimeout:
                break;
            case DisconnectCause.ClientTimeout:
                break;
            case DisconnectCause.DisconnectByServerLogic:
                break;
            case DisconnectCause.DisconnectByServerReasonUnknown:
                break;
            case DisconnectCause.InvalidAuthentication:
                break;
            case DisconnectCause.CustomAuthenticationFailed:
                break;
            case DisconnectCause.AuthenticationTicketExpired:
                break;
            case DisconnectCause.MaxCcuReached:
                break;
            case DisconnectCause.InvalidRegion:
                break;
            case DisconnectCause.OperationNotAllowedInCurrentState:
                break;
            case DisconnectCause.DisconnectByClientLogic:
                break;
            default:
                break;
        }
        base.OnDisconnected(cause);
    }

    public bool isNetAvailable()
    {
        return !(Application.internetReachability == NetworkReachability.NotReachable);
    }

    // Update is called once per frame
    void Update()
    {
        if (InternetCheckTime >= InternetCheckTimeLimit)
        {
            InternetCheckTime = 0;
            checkInternet();
        }
        else
        {
            InternetCheckTime += Time.deltaTime;
        }
    }
}
