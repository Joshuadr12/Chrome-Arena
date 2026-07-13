using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public static class SaveData
{
    public static void Save(Player player, string filename)
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        PlayerData data = new PlayerData(player);

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    public static PlayerData Load(string filename)
    {
        string path = Application.persistentDataPath + "/" + filename;
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Filename {filename} not found. Creating a new file.");
            Save(Master.newData, filename);
        }

        string json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<PlayerData>(json);
    }
}