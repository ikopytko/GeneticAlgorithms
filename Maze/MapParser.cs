namespace Maze;

public interface IMapParser
{
    int[,] GetMap();
}

public class StringMapParser : IMapParser
{
    private readonly string _source;

    public StringMapParser(string source)
    {
        _source = source;
    }
    public int[,] GetMap()
    {
        var lines = _source.Split(Environment.NewLine);
        var map = new int[lines.Length, lines[0].Length];
        for (var row = 0; row < lines.Length; row++)
        {
            var line = lines[row];
            if (line.Length != map.GetLength(1))
                throw new ArgumentException(line);
            for (var col = 0; col < line.Length; col++)
            {
                map[row, col] = int.Parse(line.AsSpan().Slice(col, 1));
            }
        }

        return map;
    }
}
