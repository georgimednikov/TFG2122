using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Threading;
using EvolutionSimulation;
using EvolutionSimulation.Entities;
using EvolutionSimulation.Genetics;

namespace VisualizadorConsola
{
    /// <summary>
    /// Implementation of a simulation with console output
    /// </summary>
    public class ConsoleSimulation : ISimulation
    {
        public void Init()
        {
            CreatureChromosome.SetChromosome();
            UniverseParametersManager.ReadJSON();
            world = new World();
            world.Init(UserInfo.Size);

            //A minimum distance to leave in between species spawn points to give them some room.
            //Calculated based on the world size and amount of species to spawn, and then reduced by
            //a value to give room in the world and not fill it in a homogenous manner.
            int minSpawnDist = UserInfo.Size / UserInfo.Species / 5;

            //List with previous spawn positions, to know if a new spot is too close to another one used.
            List<Tuple<int, int>> spawnPositions = new List<Tuple<int, int>>();
            int x, y;

            for (int i = 0; i < UserInfo.Species; i++)
            {
                bool validPosition = true;
                do
                {
                    x = RandomGenerator.Next(0, UserInfo.Size);
                    y = RandomGenerator.Next(0, UserInfo.Size);

                    foreach (Tuple<int, int> p in spawnPositions)
                    {
                        Vector2Int dist = new Vector2Int(x - p.Item1, y - p.Item2);
                        if (world.map[x, y].isWater || dist.Magnitude() < minSpawnDist)
                        {
                            validPosition = false;
                            break;
                        }
                    }
                }
                while (!validPosition);

                //The specified amount of individuals of each species is created.
                Animal a = world.CreateCreature<Animal>(x, y);
                for (int j = 1; j < UserInfo.Individuals; j++)
                {
                    world.CreateCreature<Animal>(x, y, a.chromosome, a.speciesName);
                }

                //The new position is added to the list of used.
                spawnPositions.Add(new Tuple<int, int>(x, y));
            }

            Console.WriteLine("Simulation Init done");
        }

        public void Run()
        {
            //WorldToBmp();
            int ticks = world.YearToTick(UserInfo.Years);
            for (int i = 0; i < ticks; i++)
            {
                world.Tick();
                //Render();
                Thread.Sleep(1000);
            }
            world.ExportContent();
        }

        /// <summary>
        /// Renders the map and creatures
        /// </summary>
        //public void Render()
        //{
        //    Console.Clear();
        //    for (int i = 0; i < world.map.GetLength(0); i++)
        //    {
        //        for (int j = 0; j < world.map.GetLength(1); j++)
        //        {
        //            double val = world.map[j, i].height;
        //            if (val < 0.3) Console.BackgroundColor = ConsoleColor.Black;
        //            else if (val < 0.5) Console.BackgroundColor = ConsoleColor.DarkBlue;
        //            else if (val == 0.5) Console.BackgroundColor = ConsoleColor.DarkGreen;
        //            else if (val < 0.6) Console.BackgroundColor = ConsoleColor.DarkCyan;
        //            else if (val < 0.7) Console.BackgroundColor = ConsoleColor.Blue;
        //            else if (val < 0.8) Console.BackgroundColor = ConsoleColor.Cyan;
        //            else Console.BackgroundColor = ConsoleColor.White;

        //            Console.Write((Math.Truncate(world.map[j, i].height * 10) / 1));
        //            Console.BackgroundColor = ConsoleColor.Black;
        //        }
        //        Console.Write(" ");
        //        for (int j = 0; j < world.map.GetLength(1); j++)
        //        {
        //            double val = world.map[j, i].humidity;

        //            if (val < 0.3) Console.BackgroundColor = ConsoleColor.Red;
        //            else if (val < 0.4) Console.BackgroundColor = ConsoleColor.DarkRed;
        //            else if (val < 0.5) Console.BackgroundColor = ConsoleColor.Green;
        //            else if (val < 0.6) Console.BackgroundColor = ConsoleColor.Cyan;
        //            else if (val < 0.8) Console.BackgroundColor = ConsoleColor.DarkCyan;
        //            else if (val < 1) Console.BackgroundColor = ConsoleColor.DarkBlue;
        //            else Console.BackgroundColor = ConsoleColor.White;

        //            if (world.map[j, i].height < 0.5f) Console.BackgroundColor = ConsoleColor.Magenta;

        //            val = (Math.Truncate(val * 10) / 1);
        //            if (val >= 10) Console.Write("X");
        //            else Console.Write(val);
        //            Console.BackgroundColor = ConsoleColor.Black;
        //        }
        //        Console.Write(" ");
        //        for (int j = 0; j < world.map.GetLength(1); j++)
        //        {
        //            double val = world.map[j, i].temperature;
        //            if (val < 0.2) Console.BackgroundColor = ConsoleColor.DarkBlue;
        //            else if (val < 0.3) Console.BackgroundColor = ConsoleColor.Blue;
        //            else if (val < 0.5) Console.BackgroundColor = ConsoleColor.DarkYellow;
        //            else if (val < 0.6) Console.BackgroundColor = ConsoleColor.Yellow;
        //            else if (val < 0.8) Console.BackgroundColor = ConsoleColor.DarkRed;
        //            else Console.BackgroundColor = ConsoleColor.Red;

        //            Console.Write((Math.Truncate(val * 10) / 1));
        //            Console.BackgroundColor = ConsoleColor.Black;
        //        }
        //        Console.Write(" ");

        //        //Random r = new Random();
        //        //for (int j = 0; j < world.map.GetLength(1); j++)
        //        //{
        //        //    double val = world.map[j, i].flora;
        //        //    if (val == 0 && world.map[j, i].height < 0.5) Console.BackgroundColor = ConsoleColor.DarkBlue;
        //        //    //else if (val < 0.3) Console.BackgroundColor = ConsoleColor.DarkRed;
        //        //    //else if (val < 0.4) Console.BackgroundColor = ConsoleColor.DarkYellow;
        //        //    //else if (val < 0.5) Console.BackgroundColor = ConsoleColor.Yellow;
        //        //    //else if (val < 0.7) Console.BackgroundColor = ConsoleColor.DarkGreen;
        //        //    //else Console.BackgroundColor = ConsoleColor.Green;

        //        //    if (val >= 0 && RandomGenerator.NextDouble() <= val)
        //        //        if (val <= 0.3)
        //        //            Console.BackgroundColor = ConsoleColor.Red;
        //        //        else if (val <= 0.35)
        //        //            Console.BackgroundColor = ConsoleColor.Yellow;
        //        //        else
        //        //            Console.BackgroundColor = ConsoleColor.Green;
        //        //    val = (Math.Truncate(val * 10) / 1);
        //        //    if (val == 10) Console.Write("X");
        //        //    else
        //        //        Console.Write(" ");
        //        //    Console.BackgroundColor = ConsoleColor.Black;
        //        //}

        //        Console.WriteLine();
        //    }

        //    // Let he who is without sin cast the first stone
        //    // Matthew 7:1
        //    Console.Write("Height");
        //    Console.Write("                           Humidity");
        //    Console.Write("                         Temperature");
        //    Console.Write("                      Flora");

        //    foreach (var e in world.Creatures)
        //    {
        //        Console.SetCursorPosition(e.x, e.y);
        //        Console.Write("e");
        //    }

        //    foreach (var e in world.StableEntities)
        //    {
        //        if (e.GetType() == typeof(Corpse))
        //        {
        //            Console.SetCursorPosition(e.x, e.y);
        //            Console.Write("x");
        //        }
        //    }
        //    //Console.SetCursorPosition(world.map.GetLength(0), world.map.GetLength(0));
        //}



        World world;
    }
}
