using Maze;

const double crossoverRate = 0.7;
const double mutationRate = 0.001;
const int populationSize = 140;
const int chromosomeLength = 70;
const int geneLength = 2;

string mapData =
    @"111111111111111
101000001110001
300000001110001
100011100100001
100011100000101
110011100000101
100001000011101
101100010000002
101100010000001
111111111111111";

IMapParser parser = new StringMapParser(mapData);
IMapPrinter printer = new ConsoleMapPrinter();
var map = new Map(parser.GetMap(), printer);

var agent = new CgaAgent(map, crossoverRate, mutationRate, populationSize, chromosomeLength, geneLength);

foreach (var generation in agent.Run())
{
    Console.Clear();
    Console.WriteLine($"Generation {generation} fittest score: {agent.FittestGenome}");
    agent.Render();
}

