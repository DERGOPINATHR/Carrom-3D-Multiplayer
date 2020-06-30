using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MMUserInfoScript : MonoBehaviour
{
    // Start is called before the first frame update
    public bool UserInfoSet = false;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!UserInfoSet)
        {
            if (FileHandlerScript.hasSavedData())
            {
                var savedData = FileHandlerScript.getSavedData();
                if (savedData != null)
                {
                    string lvl = "";
                    if (savedData.exp <= 100)
                    {
                        lvl = "Beginner";
                    }
                    else if (savedData.exp > 100 && savedData.exp <= 250)
                    {
                        lvl = "Intermediate";
                    }
                    else if (savedData.exp > 250)
                    {
                        lvl = "Expert";
                    }
                    transform.GetComponent<TMPro.TextMeshProUGUI>().text = string.Format("Name: {0} \nExperience: {1}\nLevel: {2}", savedData.username, savedData.exp.ToString(),lvl);
                    UserInfoSet = true;
                }
            }
        }
    }
}
