using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerPrefsSaveProvider : ISaveProvider
{
    const string Key = "save_v1";
    public Task<SaveData> LoadAsync(CancellationToken ct = default)
    {
        if (!PlayerPrefs.HasKey(Key)) return Task.FromResult(new SaveData()); //No player data found, creates new SaveData object
        var json = PlayerPrefs.GetString(Key); 
        return Task.FromResult(JsonUtility.FromJson<SaveData>(json)); //Returns the JSON as a SaveData object with fake asyncronus signiture. (because it's called same as Cloud function)
    }
    public Task SaveAsync(SaveData data, CancellationToken ct = default)
    {
        data.lastSaveUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); //Tags time onto the save file
        var json = JsonUtility.ToJson(data); //Converts save object to JSON object
        Debug.Log(json);
        PlayerPrefs.SetString(Key, json); //?
        PlayerPrefs.Save(); //Saves it
        return Task.CompletedTask; //Returns a successful message to caller.
    }
}
