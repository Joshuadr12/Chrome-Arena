using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveData
{
    public static void Save(Player player, string filename)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + filename;
        PlayerData data = new PlayerData(player);

        if (Master.testMode)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }
        else
        {
            FileStream stream = new FileStream(path, FileMode.Create);
            formatter.Serialize(stream, data);
            stream.Close();
        }
    }

    public static PlayerData Load(string filename)
    {
        string path = Application.persistentDataPath + "/" + filename;
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Filename {filename} not found. Creating a new file.");
            Save(Master.newData, filename);
        }

        if (Master.testMode)
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            PlayerData data = (PlayerData)formatter.Deserialize(stream);
            stream.Close();
            return data;
        }
    }
}