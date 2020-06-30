using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class FileHandlerScript
{
    private static string SavePath = Application.persistentDataPath + "/userInfo.dat";
    public static bool saveData(UserInfo info)
    {
        try
        {
            BinaryFormatter BinaryFormat = new BinaryFormatter();
            FileStream stream = new FileStream(SavePath, FileMode.OpenOrCreate);
            BinaryFormat.Serialize(stream, info);
            stream.Close();
            return true;
        }
        catch (Exception ex)
        {

            Debug.LogError(ex.Message);
            return false;
        }
    }

    public static bool hasSavedData()
    {
        return File.Exists(SavePath);
    }

    public static UserInfo getSavedData()
    {
        if (hasSavedData())
        {
            try
            {
                BinaryFormatter BinaryFormat = new BinaryFormatter();
                FileStream stream = new FileStream(SavePath, FileMode.Open);
                UserInfo userInfo = BinaryFormat.Deserialize(stream) as UserInfo;
                stream.Close();
                return userInfo;
            }
            catch (Exception ex)
            {

                Debug.LogError(ex.Message);
                return null;
            }
        }
        Debug.LogError("Save file not found");
        return null;
    }
}
