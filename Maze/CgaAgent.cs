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