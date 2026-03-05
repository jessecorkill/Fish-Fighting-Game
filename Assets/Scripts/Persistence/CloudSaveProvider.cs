using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;

public class CloudSaveProvider : ISaveProvider
{
    const string Key = "save_v1";

    static async Task EnsureUgsAsync() 
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized) //Makes sure connection is alive? 
        {
            await UnityServices.InitializeAsync();
        }
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            // later you can link: AuthenticationService.Instance.LinkWithAppleAsync(), etc.
        }
    }

    public async Task<SaveData> LoadAsync(CancellationToken ct = default)
    {
        await EnsureUgsAsync();
        try
        {
            var result = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { Key });
            if (result.TryGetValue(Key, out var item))
            {
                var json = item.Value.GetAsString();
                return JsonUtility.FromJson<SaveData>(json);
            }
        }
        catch { /* handle 404/offline */ }
        return new SaveData();
    }

    public async Task SaveAsync(SaveData data, CancellationToken ct = default)
    {
        await EnsureUgsAsync();
        data.lastSaveUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var json = JsonUtility.ToJson(data);
        var dict = new Dictionary<string, object> { { Key, json } };

        // ForceSaveAsync overwrites; you can also use SaveAsync with etags to handle conflicts
        await CloudSaveService.Instance.Data.Player.SaveAsync(dict);
    }
}

