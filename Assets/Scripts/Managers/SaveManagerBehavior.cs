// SaveManagerBehaviour.cs
using System;
using UnityEngine;

[DefaultExecutionOrder(-10000)] // initialize very early
public class SaveManagerBehaviour : MonoBehaviour
{
    public static SaveManagerBehaviour Instance { get; private set; }

    public SaveData Current { get; private set; }
    public bool IsLoaded { get; private set; }

    // Let listeners refresh UI when values change or first load completes
    public event Action<SaveData> OnChanged;
    public event Action<SaveData> OnLoaded;

    // (Optional) simple autosave debounce
    bool _dirty;
    float _timer, _autosaveEvery = 30f;

    async void Awake()
    {
        Debug.Log("Save Manager Awake");
        if (Instance != null && Instance != this) { Destroy(gameObject); return; } 
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SaveSystem.Init(useCloud: false); // flip when you enable Cloud Save
        Current = await SaveSystem.LoadAsync(); //Pulls player SaveData object from either cloud or local storage
        Debug.Log("Current Save File");
        Debug.Log(Current.lastSaveUnix);
        IsLoaded = true;
        OnLoaded?.Invoke(Current); //??
        OnChanged?.Invoke(Current); // ?? fire once so UI can render initial state


    }
    //public void MarkDirty() => _dirty = true;

    public void Mutate(Action<SaveData> action)
    {
        action?.Invoke(Current);
        _dirty = true;
        OnChanged?.Invoke(Current);       // notify UI to refresh

    }

    private void Update() //Auto save functionality
    {
        if (!_dirty) return;
        _timer += Time.deltaTime;
        if (_timer >= _autosaveEvery)
        {
            _timer = 30f; 
            _dirty = false;
            _ = SaveSystem.SaveAsync(Current); // background save
            Debug.Log("AUTOSAVE");
        }        
    }


    void OnApplicationQuit()
    {
        Debug.Log("Quit");
        if (Current != null) _ = SaveSystem.SaveAsync(Current);
    }
}
