namespace Maze;

class Genome
{
    public int[] Bits;
    public double Fitness = 0;

    public static Genome Random(int length)
    {
        var rnd = new Random();
        var bits = new int[length];
        for (var i = 0; i < length; i++)
        {
            bits[i] = rnd.Next(0, 2);
        }

        return new Genome
        {
            Bits = bits
        };
    }
}