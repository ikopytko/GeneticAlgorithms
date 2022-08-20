
const double crossoverRate = 0.7;
const double mutationRate = 0.001;
const int populationSize = 140;
const int chromosomeLength = 70;
const int geneLength = 2;

var map = new Map();

var agent = new CgaAgent(map, crossoverRate, mutationRate, populationSize, chromosomeLength, geneLength);

agent.Run();
Console.WriteLine(agent.Generation);
