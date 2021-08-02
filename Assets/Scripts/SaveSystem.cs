using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SaveMissionsInfo(MissionsInfo missionsInfo)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/missions.info";
        FileStream stream = new FileStream(path, FileMode.Create);

        SaveMissionsInfo data = new SaveMissionsInfo(missionsInfo);

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static SaveMissionsInfo LoadMissionsInfo()
    {
        string path = Application.persistentDataPath + "/missions.info";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
        
            SaveMissionsInfo data = formatter.Deserialize(stream) as SaveMissionsInfo;
            stream.Close();
            
            return data;
        }
        else
        {
            Debug.Log("No existeix el fitxer " + path);
            return null;
        }
    }

    public static void DeleteMissionsInfo()
    {
        string path = Application.persistentDataPath + "/missions.info";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
