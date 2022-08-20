namespace Maze;

public interface IMapPrinter
{
    void Print(params int[][,] layers);
}
public class ConsoleMapPrinter : IMapPrinter
{
    public void Print(params int[][,] layers)
    {
        ArgumentNullException.ThrowIfNull(layers);
        var height = layers[0].GetLength(0);
        var width = layers[0].GetLength(1);
        for (var row = 0; row < height; row++)
        {
            for (var col = 0; col < width; col++)
            {
                var cellType = 0;
                foreach (var layer in layers)
                {
                    if (layer[row, col] != 0)
                        cellType = layer[row, col];
                }
                var c = GetPrintableChar(cellType);
                Console.Write(c);
            }

            Console.WriteLine();
        }
    }
    
    private static char GetPrintableChar(int cellType) => cellType switch
    {
        0 => '.',
        1 => '#',
        2 => '>',
        3 => '@',
        5 => '↑',
        6 => '↓',
        7 => '→',
        8 => '←',
        _ => '?'
    };
}