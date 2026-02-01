public interface IE2IndexMappingProvider
{
    IReadOnlyList<(int Index, string PointName)> GetPointsForCellType(int cellType);
}
