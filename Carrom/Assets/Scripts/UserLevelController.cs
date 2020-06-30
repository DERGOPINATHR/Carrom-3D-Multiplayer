using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserLevelController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (FileHandlerScript.hasSavedData())
        {
            var savedData = FileHandlerScript.getSavedData();
            if (savedData != null)
            {
                if (savedData.exp <= 100)
                {
                    transform.gameObject.SetActive(true);
                }
                else
                {
                    transform.gameObject.SetActive(false);
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
