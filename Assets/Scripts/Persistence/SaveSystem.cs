using System.Threading;
using System.Threading.Tasks;

public static class SaveSystem
{
    static ISaveProvider _provider;
    public static void Init(bool useCloud)
    {
        _provider = useCloud ? new CloudSaveProvider() : new PlayerPrefsSaveProvider();
    }
    public static Task<SaveData> LoadAsync(CancellationToken ct = default) => _provider.LoadAsync(ct);
    public static Task SaveAsync(SaveData d, CancellationToken ct = default) => _provider.SaveAsync(d, ct);
}
