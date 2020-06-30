using Assets.Scripts;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrigerController : MonoBehaviourPun, IPunObservable
{
    private static Vector3 PlayerOneStrigerPoint = new Vector3(0, 5f, -12);
    private static Vector3 PlayerTwoStrigerPoint = new Vector3(0, 5f, 11);
    public bool Dragging = false;
    public bool StrigerThrow = false;

    public double DragAngle;

    public float DragLength;

    public double ThrowAngle;

    public bool StrigerMoving = false;

    public bool PlayersSet = false;

    [SerializeField]
    public Player PlayerOne;
    [SerializeField]
    public Player PlayerTwo;

    public PlayerType Turn;

    public float DistanceCamToObject;

    public Vector3 ThrowDirection;
    public float MaxForce = 200;
    public float MidForce = 150;
    public float MinForce = 100;

    public GameType GameType;

    private PhotonView PhotonView;



    public float StrigerMoveTime;
    public Vector3 LagLatestPos;
    public Quaternion LagLatestRot;

    public float LagCurrentTime;
    public double LagLastPacketTime;
    public double LagCurrentPacketTime;
    public Vector3 PositionAtLastPacket;
    public Quaternion RotationAtLastPacket;
    public Vector3 NetworkPosition;
    public Quaternion NetworkRotation;
    private float sleepThresh = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        GameType = GameStatusScript.getInstance().GameType;
        if (GameType == GameType.SinglePlayer)
        {
            Destroy(this);
        }
        PhotonView = GetComponent<PhotonView>();
        GetComponent<Rigidbody>().sleepThreshold = sleepThresh;
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            GetComponent<Rigidbody>().position = Vector3.MoveTowards(GetComponent<Rigidbody>().position, NetworkPosition, Time.fixedDeltaTime);
            GetComponent<Rigidbody>().rotation = Quaternion.RotateTowards(GetComponent<Rigidbody>().rotation, NetworkRotation, Time.fixedDeltaTime * 100.0f);
            /* //Lag compensation
             double timeToReachGoal = LagCurrentPacketTime - LagLastPacketTime;
             LagCurrentTime += Time.deltaTime;

             //Update remote player
             transform.position = Vector3.Lerp(PositionAtLastPacket, LagLatestPos, (float)(LagCurrentTime / timeToReachGoal));
             transform.rotation = Quaternion.Lerp(RotationAtLastPacket, LagLatestRot, (float)(LagCurrentTime / timeToReachGoal));*/
        }
        if (GameStatusScript.getInstance() != null)
        {
            Turn = GameStatusScript.getInstance().Turn;
            if (!PlayersSet && GameStatusScript.getInstance().MatchStatus == MatchStatus.INGAME)
            {
                PlayerOne = PhotonNetwork.PlayerList[0];
                PlayerTwo = PhotonNetwork.PlayerList[1];
                PlayersSet = true;
            }
        }
        /*  if (!Input.GetMouseButtonDown(0))
          {
              return;
          }*/
        if (Turn == PlayerType.Player1)
        {
            if (PlayerOne.IsLocal)
            {
                if (GetComponent<PhotonView>().Owner != PlayerOne)
                {
                    GetComponent<PhotonView>().RequestOwnership();
                }
                if (StrigerMoving)
                {
                    StrigerMoveTime += Time.deltaTime;
                    if (StrigerMoveTime > 7)
                    {
                        PhotonView.RPC("UnfreezeAllYRPC", RpcTarget.All);
                        if (CoinController.GetInstance().AllSleeping() && GetComponent<Rigidbody>().IsSleeping())
                        {
                            StrigerMoveTime = 0;
                            StrigerMoving = false;
                            // Toggle turn here.
                            if (GameSetupScript.GetInstance() != null)
                            {
                                //GameSetupScript.GetInstance().ToggleTurn();
                                PhotonNetwork.RaiseEvent(RaiseEventCodes.ToggleTurnEvent,null, new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient }, SendOptions.SendReliable);
                                GameStatusScript.getInstance().StrigerMoving = false;
                            }
                        }
                    }
                }
            }
            if (Dragging && PlayerOne.IsLocal)
            {
                Vector3 rayPoint = getRayPoint();
                if ((rayPoint.x >= -7.5 && rayPoint.x <= 7.5) && (rayPoint.z <= -10 && rayPoint.z >= -14))
                {
                    var point = rayPoint;
                    point.y = transform.position.y;
                    point.z = transform.position.z;
                    transform.position = point;
                    StrigerThrow = false;

                    // Toggle Ray cast off here
                    StrigerRayCast.toggleLine(false);
                }
                if (rayPoint.z < -13)
                {
                    StrigerThrow = true;
                    // Toggle Ray cast on here
                    StrigerRayCast.toggleLine(false);
                    DragAngle = AngleBetween(new Vector2(transform.position.x, transform.position.z), new Vector2(rayPoint.x, rayPoint.z));
                    DragLength = Vector3.Distance(transform.position, rayPoint);
                    ThrowAngle = 90 + DragAngle;
                    var x = 100 * Math.Cos(ThrowAngle * Mathf.Deg2Rad);
                    var z = 100 * Math.Sin(ThrowAngle * Mathf.Deg2Rad);
                    ThrowDirection = new Vector3((float)x, 0, (float)z);
                    Debug.DrawRay(transform.position, ThrowDirection, Color.green, 0.2f, false);
                    //  cast line
                    StrigerRayCast.castLine(transform.position, ThrowDirection);
                }
            }
        }
        else if (Turn == PlayerType.Player2)
        {
            if (PlayerTwo.IsLocal)
            {
                if (GetComponent<PhotonView>().Owner != PlayerTwo)
                {
                    GetComponent<PhotonView>().RequestOwnership();
                }
                if (StrigerMoving)
                {
                    StrigerMoveTime += Time.deltaTime;
                    if (StrigerMoveTime > 7)
                    {
                        PhotonView.RPC("UnfreezeAllYRPC", RpcTarget.All);
                        if (CoinController.GetInstance().AllSleeping() && GetComponent<Rigidbody>().IsSleeping())
                        {
                            StrigerMoveTime = 0;
                            StrigerMoving = false;
                            GameStatusScript.getInstance().StrigerMoving = false;
                            // Toggle turn here.
                            if (GameSetupScript.GetInstance() != null)
                            {
                                PhotonNetwork.RaiseEvent(RaiseEventCodes.ToggleTurnEvent, null, new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient }, SendOptions.SendReliable);
                            }
                        }
                    }
                }
            }
            if (Dragging && PlayerTwo.IsLocal)
            {
                Vector3 rayPoint = getRayPoint();
                if ((rayPoint.x >= -7.5 && rayPoint.x <= 7.5) && (rayPoint.z >= 10 && rayPoint.z <= 12))
                {
                    var point = rayPoint;
                    point.y = transform.position.y;
                    point.z = transform.position.z;
                    transform.position = point;
                    StrigerThrow = false;

                    // Toggle Ray cast off here
                    StrigerRayCast.toggleLine(false);
                }
                if (rayPoint.z > 12)
                {
                    StrigerThrow = true;
                    // Toggle Ray cast onn here
                    StrigerRayCast.toggleLine(true);
                    DragAngle = AngleBetween(new Vector2(transform.position.x, transform.position.z), new Vector2(rayPoint.x, rayPoint.z));
                    DragLength = Vector3.Distance(transform.position, rayPoint);
                    ThrowAngle = DragAngle - 90;
                    var x = 100 * Math.Cos(ThrowAngle * Mathf.Deg2Rad);
                    var z = 100 * Math.Sin(ThrowAngle * Mathf.Deg2Rad);
                    ThrowDirection = new Vector3((float)x, 0, (float)z);
                    Debug.DrawRay(transform.position, ThrowDirection, Color.green, 0.2f, false);
                    //  cast line
                    StrigerRayCast.castLine(transform.position, ThrowDirection);
                }
            }
        }
        
    }

    [PunRPC]
    private void UnfreezeAllYRPC()
    {
        CoinController.GetInstance().toggleAllY(false);
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
    }

    public Vector3 getRayPoint()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        DistanceCamToObject = Vector3.Distance(transform.position, Camera.main.transform.position);
        return ray.GetPoint(DistanceCamToObject);
    }

    private void OnMouseDown()
    {
        Dragging = true;
    }


    void OnMouseUp()
    {
        Dragging = false;
        if (StrigerThrow)
        {
            StrigerThrow = false;
            PhotonView.RPC("ThrowStrigerRPC", RpcTarget.All, ThrowDirection, DragLength);
            StrigerRayCast.toggleLine(false);
            StrigerMoving = true;
            GameStatusScript.getInstance().StrigerMoving = true;
        }
    }

    [PunRPC]
    private void ThrowStrigerRPC(Vector3 throwDirection, float dragLength)
    {
        CoinController.GetInstance().toggleAllY(true);
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        if (dragLength > 11)
        {
            GetComponent<Rigidbody>().AddForce(throwDirection * MaxForce);
        }
        else if (dragLength > 6)
        {
            GetComponent<Rigidbody>().AddForce(throwDirection * MidForce);
        }
        else if (dragLength < 6)
        {
            GetComponent<Rigidbody>().AddForce(throwDirection * MinForce);
        }

    }

    public void setStriger(PlayerType type)
    {
        switch (type)
        {
            case PlayerType.Player1:
                transform.position = PlayerOneStrigerPoint;
                break;
            case PlayerType.Player2:
                transform.position = PlayerTwoStrigerPoint;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// AngleBetween - the angle between 2 vectors
    /// </summary>
    /// <returns>
    /// Returns the the angle in degrees between vector1 and vector2
    /// </returns>
    /// <param name="vector1"> The first Vector </param>
    /// <param name="vector2"> The second Vector </param>
    public double AngleBetween(Vector2 vector1, Vector2 vector2)
    {
        double sin = vector1.x * vector2.y - vector2.x * vector1.y;
        double cos = vector1.x * vector2.x + vector1.y * vector2.y;

        return Math.Atan2(sin, cos) * (180 / Math.PI);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(GetComponent<Rigidbody>().position);
            stream.SendNext(GetComponent<Rigidbody>().rotation);
            stream.SendNext(GetComponent<Rigidbody>().velocity);
        }
        else
        {
            NetworkPosition = (Vector3)stream.ReceiveNext();
            NetworkRotation = (Quaternion)stream.ReceiveNext();
            GetComponent<Rigidbody>().velocity = (Vector3)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            NetworkPosition += GetComponent<Rigidbody>().velocity * lag;
        }
    }
}
