using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SleepThreshScript : MonoBehaviour
{
    private float sleepThresh = 0.2f;
    public float LowVelocityTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().sleepThreshold = sleepThresh;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameStatusScript.getInstance().StrigerMoving)
        {
            LowVelocityTime = 0;
        }
        if (GetComponent<Rigidbody>().velocity.magnitude < 1)
        {
            LowVelocityTime += Time.deltaTime;
            if (LowVelocityTime > 5 && GameStatusScript.getInstance().StrigerMoving)
            {
                GetComponent<Rigidbody>().Sleep();
            }
        }
        else
        {
            LowVelocityTime = 0;
        }
    }
}
