using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class UserNameInputScript : MonoBehaviour
{
    private GameObject UsernameInput, MainMenu;

    private void Start()
    {
        UsernameInput = transform.Find("UsernameInput").gameObject;
        MainMenu = transform.parent.gameObject.transform.Find("MainMenu").gameObject;
    }

    public void saveUsername()
    {
        string userInput = UsernameInput.transform.GetChild(0).gameObject.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text.Trim();
        if (userInput.Length == 1)
        {
            //AndroidNativeFunctions.ShowToast("Enter valid username");
            return;
        }
        /*if()
        {
        Unique username part here.
        }*/
        if (FileHandlerScript.hasSavedData())
        {
            var UserData = FileHandlerScript.getSavedData();
            UserData.username = userInput;
            FileHandlerScript.saveData(UserData);
        }
        else
        {
            var UserData = new UserInfo 
            {
                username = userInput,
                exp = 0
            };
            FileHandlerScript.saveData(UserData);
        }
        //AndroidNativeFunctions.ShowToast("Username updated");
        gameObject.SetActive(false);
        MainMenu.SetActive(true);
        SendMessageUpwards("UpdateUserName", userInput);
    }
}
