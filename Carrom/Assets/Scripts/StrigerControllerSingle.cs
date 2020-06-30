using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrigerControllerSingle : MonoBehaviour
{
    private static Vector3 PlayerOneStrigerPoint = new Vector3(0, 5f, -12);
    private static Vector3 PlayerTwoStrigerPoint = new Vector3(0, 5f, 11);
    public GameType GameType;

    private float sleepThresh = 0.2f;

    public PlayerType Turn;


    public float StrigerMoveTime;
    public float DistanceCamToObject;
    public Vector3 ThrowDirection;

    public bool StrigerMoving = false;
    public bool Dragging = false;
    public bool StrigerThrow = false;

    public double DragAngle;
    public float DragLength;
    public double ThrowAngle;

    public float MaxForce = 200;
    public float MidForce = 150;
    public float MinForce = 100;

    // Start is called before the first frame update
    void Start()
    {
        GameType = GameStatusScript.getInstance().GameType;
        if (GameType == GameType.MultiPlayer)
        {
            Destroy(this);
        }
        GetComponent<Rigidbody>().sleepThreshold = sleepThresh;

    }

    // Update is called once per frame
    void Update()
    {
        if (GameStatusScript.getInstance() != null)
        {
            Turn = GameStatusScript.getInstance().Turn;
        }
        if (Turn == PlayerType.Player1)
        {
            if (Dragging)
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
        if (StrigerMoving)
        {
            StrigerMoveTime += Time.deltaTime;
            if (StrigerMoveTime > 7)
            {
                //Unfreeze all y here
                UnfreezeAllYRPC();
                //PhotonView.RPC("UnfreezeAllYRPC", RpcTarget.All);
                if (CoinController.GetInstance().AllSleeping() && GetComponent<Rigidbody>().IsSleeping())
                {
                    StrigerMoveTime = 0;
                    
                    if (GameSetupSingle.GetInstance() != null)
                    {
                        // Toggle turn here
                        GameSetupSingle.GetInstance().ToggleTurn();
                        //PhotonNetwork.RaiseEvent(RaiseEventCodes.ToggleTurnEvent, null, new RaiseEventOptions() { Receivers = ReceiverGroup.MasterClient }, SendOptions.SendReliable);
                        GameStatusScript.getInstance().StrigerMoving = false;
                        StrigerMoving = false;
                    }
                }
            }
        }
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
            ThrowStrigerRPC(ThrowDirection, DragLength);
            StrigerRayCast.toggleLine(false);
            StrigerMoving = true;
            GameStatusScript.getInstance().StrigerMoving = true;
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

    public IEnumerator ComputerMovePossibleShot()
    {
        List<PossibleShot> possibles = new List<PossibleShot>();
        double transformLocationStart = -7.5;
        double transformLocationEnd = 7.5;
        for (double transformLocationX = transformLocationStart; transformLocationX < transformLocationEnd; transformLocationX += 0.1)
        {
            for (int theta = -10; theta > -170; theta--)
            {
                Vector3 ThrowDirection = StrigerRayCast.GetLocation(theta, 10);

                RaycastHit hit;
                //Debug.DrawRay(new Vector3((float)transformLocationX, transform.position.y, transform.position.z), ThrowDirection* 1000, Color.yellow, 20);
                // Does the ray intersect any objects excluding the player layer
                if (Physics.Raycast(new Vector3((float)transformLocationX, transform.position.y, transform.position.z), ThrowDirection * 1000, out hit, Mathf.Infinity))
                {
                    //Debug.DrawRay(transform.position, ThrowDirection.normalized * hit.distance, Color.yellow, 5);

                    PossibleShot shot = new PossibleShot
                    {
                        ThrowDirection = ThrowDirection.normalized,
                        TransformLocation = new Vector3((float)transformLocationX, transform.position.y, transform.position.z),
                        HitCoinType = ScoreCoinType.NOTSET,
                        HitDistance = hit.distance
                    };
                    if (hit.collider.gameObject.transform.parent != null)
                    {
                        if (hit.collider.gameObject.transform.parent.name == "WhiteCoins")
                        {
                            shot.HitCoinType = ScoreCoinType.White;
                        }
                        if (hit.collider.gameObject.transform.parent.name == "BlackCoins")
                        {
                            shot.HitCoinType = ScoreCoinType.Black;
                        }
                    }
                    if (hit.collider.gameObject.name == "Queen")
                    {
                        shot.HitCoinType = ScoreCoinType.Queen;

                    }
                    if (shot.HitCoinType != ScoreCoinType.NOTSET)
                    {
                        possibles.Add(shot);
                    }
                }
                else
                {
                    //Debug.DrawRay(transform.position, ThrowDirection.normalized * 1000, Color.white, 15);
                }
            }
            yield return null;
        }

        if (possibles.Count != 0)
        {
            ThrowStrigerRPC(possibles[possibles.Count / 2].ThrowDirection * possibles[possibles.Count / 2].HitDistance * 5, possibles[possibles.Count / 2].HitDistance);
            StrigerMoving = true;
        }
        yield break;
        //GetComponent<Rigidbody>().AddForce(possibles[possibles.Count/2].ThrowDirection* possibles[possibles.Count / 2].HitDistance);
    }

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
}
