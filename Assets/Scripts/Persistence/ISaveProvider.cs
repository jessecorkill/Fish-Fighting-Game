using System.Threading;
using System.Threading.Tasks;

public interface ISaveProvider
{
    Task<SaveData> LoadAsync(CancellationToken ct = default);
    Task SaveAsync(SaveData data, CancellationToken ct = default);
}
