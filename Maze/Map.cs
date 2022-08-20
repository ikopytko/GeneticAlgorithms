﻿namespace Maze;

public interface IMap
{
    /// <summary>
    /// Takes a series of directions and evaluate travelled distance.
    /// </summary>
    /// <param name="path">Path to travel</param>
    /// <returns>Fitness score proportional to the distance reached from the exit</returns>
    double TestRoute(List<int> path);

    /// <summary>
    /// Print current map to the console
    /// </summary>
    void PrintMap();
}

class Map : IMap
{
    private readonly IMapPrinter _mapPrinter;
    private readonly int[,] _map;
    private readonly int _width;
    private readonly int _height;

    private readonly Cell _start;
    private readonly Cell _end;

    private readonly int[,] _memory;
    
    public Map(int[,] mapData, IMapPrinter mapPrinter)
    {
        _map = mapData;
        _mapPrinter = mapPrinter;

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
        var penalty = 0;
        foreach (var direction in path)
        {
            (int moveX, int moveY) = (Direction)direction switch
            {
                Direction.North => (0, -1),
                Direction.South => (0, 1),
                Direction.East =>  (1, 0),
                Direction.West =>  (-1, 0)
            };
            
            if ((posX + moveX) >= 0 && (posX + moveX) < _width &&
                (posY + moveY) >= 0 && (posY + moveY) < _height)
            {
                var cell = _map[posY + moveY, posX + moveX];
                if (cell is 0 or 2 or 3)
                {
                    if (_memory[posY, posX] != 0)
                        penalty++;
                    _memory[posY, posX] = 5 + direction;
                    posX += moveX;
                    posY += moveY;
                }
            }
        }

        var diffX = Math.Abs(posX - _end.X);
        var diffY = Math.Abs(posY - _end.Y);
        return 1.0 / (diffX + diffY + 1 + penalty);
    }

    /// <summary>
    /// Print current map to the console
    /// </summary>
    public void PrintMap()
    {
        _mapPrinter.Print(_map, _memory);
    }
}

enum Direction
{
    North, South, East, West
}

record Cell(int X, int Y);
