using System.Reflection;
using System.Text.Json;

public sealed class EmbeddedE2IndexMappingProvider : IE2IndexMappingProvider
{
    private readonly Dictionary<int, List<(int Index, string PointName)>> _map;

    public EmbeddedE2IndexMappingProvider()
    {
        _map = LoadMappings();
    }

    private Dictionary<int, List<(int Index, string PointName)>> LoadMappings()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "BmsBridge.Resources.E2IndexMappings.json";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource: {resourceName}");

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        var items = JsonSerializer.Deserialize<List<E2PointIndexMapping>>(json)
            ?? new List<E2PointIndexMapping>();

        return items
            .GroupBy(x => x.CellType)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => (x.Index, x.Name)).ToList()
            );
    }

    public IReadOnlyList<(int Index, string PointName)> GetPointsForCellType(int cellType)
    {
        return _map.TryGetValue(cellType, out var list)
            ? list
            : Array.Empty<(int, string)>();
    }
}
