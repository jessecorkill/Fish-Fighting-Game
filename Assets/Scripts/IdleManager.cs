using System;
using UnityEngine;

public class IdleManager : MonoBehaviour
{
    private const string LastUtcKey = "last_utc_iso";
    private const string SaveVersionKey = "save_ver";
    private const int SaveVersion = 1;

    // Example game vars
    public double softCurrency;                 // e.g., coins
    public double productionPerSecond = 12.0;   // base production rate
    public double storageCap = 10_000;          // optional cap for offline gains
    public double offlineMultiplier = 1.0;      // meta upgrades can raise this
    public double maxOfflineHours = 12;         // cap to avoid absurd jumps

    void Start()
    {
        ApplyOfflineProgress();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            ApplyOfflineProgress();
        else
            SaveTimestamp();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused) SaveTimestamp();
    }

    void OnApplicationQuit()
    {
        SaveTimestamp();
    }

    void SaveTimestamp()
    {
        PlayerPrefs.SetInt(SaveVersionKey, SaveVersion);
        PlayerPrefs.SetString(LastUtcKey, DateTime.UtcNow.ToString("o")); // ISO 8601
        PlayerPrefs.Save();
    }

    void ApplyOfflineProgress()
    {
        if (!PlayerPrefs.HasKey(LastUtcKey))
        {
            SaveTimestamp();
            return;
        }

        var now = DateTime.UtcNow;
        var last = DateTime.Parse(PlayerPrefs.GetString(LastUtcKey), null, System.Globalization.DateTimeStyles.RoundtripKind);

        double elapsedSeconds = Math.Max(0, (now - last).TotalSeconds);

        // Clamp to cap
        double clampedSeconds = Math.Min(elapsedSeconds, maxOfflineHours * 3600.0);

        // Example growth: linear production with multiplier and storage cap
        double gained = clampedSeconds * productionPerSecond * offlineMultiplier;
        double room = Math.Max(0, storageCap - softCurrency);
        double applied = Math.Min(gained, room);

        softCurrency += applied;

        // (Optional) show a summary UI
        Debug.Log($"Offline for {clampedSeconds:F0}s, gained {applied:F0} coins (of {gained:F0}).");

        SaveTimestamp(); // reset baseline
    }
}
