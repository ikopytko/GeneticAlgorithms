
const double crossoverRate = 0.7;
const double mutationRate = 0.001;
const int populationSize = 140;
const int chromosomeLength = 70;
const int geneLength = 2;

var agent = new CgaAgent(crossoverRate, mutationRate, populationSize, chromosomeLength, geneLength);

agent.Run();
Console.WriteLine(agent.Generation);

class Map
{
    private readonly int[,] _map;
    private readonly int _width;
    private readonly int _height;

    private readonly Cell _start;
    private readonly Cell _end;

    private readonly int[,] _memory;
    
    public Map()
    {
        _map = new[,]
        {
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            {1, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1},
            {3, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1},
            {1, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 1},
            {1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 1},
            {1, 1, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 1},
            {1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 1},
            {1, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 2},
            {1, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1},
            {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
        };

        _height = _map.GetLength(0);
        _width = _map.GetLength(1);
        _memory = new int[_height, _width];

        for (var i = 0; i < _map.Length; i++)
        {
            var x = i % _width;
            var y = i / _width;
            if (_map[y, x] == 2)
            {
                if (_start != null)
                    throw new ArgumentException("Double start node");
                _start = new Cell(x, y);
            }
            else if (_map[y, x] == 3)
            {
                if (_end != null)
                    throw new ArgumentException("Double end node");
                _end = new Cell(x, y);
            }
        }

        Console.WriteLine($"START: {_start}");
        Console.WriteLine($"END:   {_end}");
    }

    /// <summary>
    /// Takes a series of directions and evaluate travelled distance.
    /// </summary>
    /// <param name="path">Path to travel</param>
    /// <returns>Fitness score proportional to the distance reached from the exit</returns>
    public double TestRoute(List<int> path)
    {
        // given a vector of directions — representing North(0), South(1), East(2), West(3)
        // calculates the farthest position in the map Bob can reach
        // and then returns a fitness score proportional to Bob’s final distance from the exit.
        // The closer to the exit he gets, the higher the fitness score he is rewarded.
        // If the exit reached, the maximum fitness score of one is returned.
        
        Array.Clear(_memory);
        int posX = _start.X, posY = _start.Y;
        foreach (var direction in path)
        {
            int moveX = 0, moveY = 0;
            switch ((Direction)direction)
            {
                case Direction.North:
                    moveY = -1;
                    break;
                case Direction.South:
                    moveY = 1;
                    break;
                case Direction.East:
                    moveX = 1;
                    break;
                case Direction.West:
                    moveX = -1;
                    break;
            }

            if ((posX + moveX) >= 0 && (posX + moveX) < _width &&
                (posY + moveY) >= 0 && (posY + moveY) < _height)
            {
                var cell = _map[posY + moveY, posX + moveX];
                if (cell is 0 or 2 or 3)
                {
                    _memory[posY, posX] = 5 + direction;
                    posX += moveX;
                    posY += moveY;
                }
            }
        }

        var diffX = Math.Abs(posX - _end.X);
        var diffY = Math.Abs(posY - _end.Y);
        return 1.0 / (diffX + diffY + 1);
    }

    /// <summary>
    /// Print current map to the console
    /// </summary>
    public void PrintMap()
    {
        for (var row = 0; row < _height; row++)
        {
            for (var col = 0; col < _width; col++)
            {
                if (_memory[row, col] != 0)
                {
                    var c = GetPrintableChar(_memory[row, col]);
                    Console.Write(c);
                }
                else
                {
                    var c = GetPrintableChar(_map[row, col]);
                    Console.Write(c);
                }
            }

            Console.WriteLine();
        }
    }
    
    private char GetPrintableChar(int cellType) => cellType switch
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

class CgaAgent
{
    private readonly Random _random = new Random();
    private List<Genome> _genomes;
    private readonly int _populationSize;
    private readonly double _crossoverRate;
    private readonly double _mutationRate;
    private readonly int _chromosomeLength;
    private readonly int _geneLength;
    private int _fittestGenome;
    private double _bestFitnessScore;
    private double _totalFitnessScore;
    private int _generation;
    private Map _map;
    private Map _bestPass;
    private bool _isBusy;

    public int Generation => _generation;
    public Genome FittestGenome => _genomes[_fittestGenome];

    private void Mutate(ref int[] bits)
    {
        for (var i = 0; i < bits.Length; i++)
        {
            if (_random.NextDouble() < _mutationRate)
            {
                bits[i] = 1 - bits[i];
            }
        }
    }

    private void Crossover(int[] mum, int[] dad, out int[] baby1, out int[] baby2)
    {
        if (_random.NextDouble() > _crossoverRate || mum == dad)
        {
            baby1 = mum;
            baby2 = dad;
            return;
        }

        var cp = _random.Next(_chromosomeLength-1);
        baby1 = mum.Take(cp).Concat(dad.Skip(cp)).ToArray();
        baby2 = dad.Take(cp).Concat(mum.Skip(cp)).ToArray();
    }

    private Genome RouletteWheelSelection()
    {
        double slice = _random.NextDouble() * _totalFitnessScore;
        double cfTotal = 0;
        int selectedGenome = 0;

        for (var i = 0; i < _populationSize; i++)
        {
            cfTotal += _genomes[i].fitness;
            if (cfTotal > slice)
            {
                selectedGenome = i;
                break;
            }
        }

        return _genomes[selectedGenome];
    }

    /// <summary>
    /// updates the genomes fitness with the new fitness scores and calculates
    /// the highest fitness and the fittest member of the population.
    /// </summary>
    private void UpdateFitnessScore()
    {
        _totalFitnessScore = 0;
        _fittestGenome = 0;
        _bestFitnessScore = 0;
        for (var i = 0; i < _genomes.Count; i++)
        {
            var genome = _genomes[i];
            var path = Decode(genome.bits);

            var fitness = _map.TestRoute(path);
            genome.fitness = fitness;
            _totalFitnessScore += fitness;
            if (fitness > _bestFitnessScore)
            {
                _bestFitnessScore = fitness;
                _fittestGenome = i;

                if (fitness == 1)
                {
                    _isBusy = false;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// decodes a vector of bits into a vector of ints
    /// </summary>
    private List<int> Decode(IEnumerable<int> bits) 
        => bits.Chunk(_geneLength).Select(BitToInt).ToList();

    /// <summary>
    /// converts a vector of bits into decimal. Used by Decode.
    /// </summary>
    private int BitToInt(int[] bit)
    {
        int number = 0;
        for (int i = bit.Length - 1; i >= 0; i--)
        {
            number += bit[i] * (int)Math.Pow(2, i);
        }
        return number;
    }

    /// <summary>
    /// creates a start population of random bit strings
    /// </summary>
    private void CreateStartPopulation()
    {
        _genomes = new List<Genome>();
        for (int i = 0; i < _populationSize; i++)
        {
            _genomes.Add(Genome.Random(_chromosomeLength));
        }
    }

    public CgaAgent(double crossoverRate, double mutationRate, int populationSize, int chromosomeLength, int geneLength)
    {
        _crossoverRate = crossoverRate;
        _mutationRate = mutationRate;
        _chromosomeLength = chromosomeLength;
        _geneLength = geneLength;
        _populationSize = populationSize;
        _map = new Map();
        CreateStartPopulation();
    }

    public void Run()
    {
        _isBusy = true;
        while (_isBusy)
        {
            Epoch();
            Render();
        }
    }

    public void Epoch()
    {
        UpdateFitnessScore();
        var newBabies = 0;
        List<Genome> babyGenomes = new List<Genome>();

        while (newBabies < _populationSize)
        {
            Genome mum = RouletteWheelSelection();
            Genome dad = RouletteWheelSelection();

            Genome baby1 = new Genome();
            Genome baby2 = new Genome();
            Crossover(mum.bits, dad.bits, out baby1.bits, out baby2.bits);
            
            Mutate(ref baby1.bits);
            Mutate(ref baby2.bits);
            
            babyGenomes.Add(baby1);
            babyGenomes.Add(baby2);

            newBabies += 2;
        }

        _genomes = babyGenomes;
        _generation++;
    }

    public void Render()
    {
        Console.WriteLine(_bestFitnessScore);
        _map.PrintMap();
    }
    public bool Started() => _isBusy;
    public void Stop() => _isBusy = false;
}

class Genome
{
    public int[] bits;
    public double fitness;

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
            bits = bits
        };
    }
}

enum Direction
{
    North, South, East, West
}
record Cell(int X, int Y);