using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public static class SaveData
{
    public static void Save(Player player, string filename)
    {
        string path = Path.Combine(Application.persistentDataPath, filename);
        PlayerData_0_3_2 data = new PlayerData_0_3_2(player);

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    public static PlayerData_0_3_2 Load(string filename)
    {
        string path = Application.persistentDataPath + "/" + filename;
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Filename {filename} not found. Creating a new file.");
            Save(Master.newData, filename);
        }
        string json;
        try
        {
            json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<PlayerData_0_3_2>(json);
        }
        catch
        {
            Debug.LogWarning($"Filename {filename} is in an outdated version. Deleting and creating a new file.");
            Save(Master.newData, filename);
            json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<PlayerData_0_3_2>(json);
        }
    }
}