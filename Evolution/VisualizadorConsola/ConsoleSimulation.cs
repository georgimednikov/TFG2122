using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using EvolutionSimulation;

namespace VisualizadorConsola
{
    /// <summary>
    /// Implementation of a simulation with console output
    /// </summary>
    public class ConsoleSimulation : ISimulation
    {
        public void Init()
        {
            world = new World();
            world.Init(32);
            Creature c = world.AddEntity<Creature>();
            c.Init(world, 4, 4);
        }

        public void Run()
        {
            while (true)
            {
                world.Tick();
                Render();
                Thread.Sleep(200000);
                world.Init(477);
            }
        }

        /// <summary>
        /// Renders the map and creatures
        /// TODO: Modify this MF
        /// </summary>
        public void Render()
        {
            Console.Clear();
            for (int i = 0; i < world.map.GetLength(0); i++)
            {
                //for (int j = 0; j < world.map.GetLength(1); j++)
                //{
                //    double val = world.map[j, i].height;
                //    if (val < 0.3) Console.BackgroundColor = ConsoleColor.Black;
                //    else if (val < 0.5) Console.BackgroundColor = ConsoleColor.DarkBlue;
                //    else if (val == 0.5) Console.BackgroundColor = ConsoleColor.DarkGreen;
                //    else if (val < 0.6) Console.BackgroundColor = ConsoleColor.DarkCyan;
                //    else if (val < 0.7) Console.BackgroundColor = ConsoleColor.Blue;
                //    else if (val < 0.8) Console.BackgroundColor = ConsoleColor.Cyan;
                //    else Console.BackgroundColor = ConsoleColor.White;

                //    Console.Write((Math.Truncate(world.map[j, i].height * 10) / 1));
                //    Console.BackgroundColor = ConsoleColor.Black;
                //}
                //Console.Write(" ");
                /*
                for (int j = 0; j < world.map.GetLength(1); j++)
                {
                    double val = world.map[j, i].humidity;

                    if (val < 0.3) Console.BackgroundColor = ConsoleColor.Red;
                    else if (val < 0.4) Console.BackgroundColor = ConsoleColor.DarkRed;
                    else if (val < 0.5) Console.BackgroundColor = ConsoleColor.Green;
                    else if (val < 0.6) Console.BackgroundColor = ConsoleColor.Cyan;
                    else if (val < 0.8) Console.BackgroundColor = ConsoleColor.DarkCyan;
                    else if (val < 1) Console.BackgroundColor = ConsoleColor.DarkBlue;
                    else Console.BackgroundColor = ConsoleColor.White;

                    if (world.map[j, i].height < 0.5f) Console.BackgroundColor = ConsoleColor.Magenta ;

                    val = (Math.Truncate(val * 10) / 1);
                    if (val >= 10) Console.Write("X");
                    else Console.Write(val);
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                */
                //Console.Write(" ");
                /*
                for (int j = 0; j < world.map.GetLength(1); j++)
                {
                    double val = world.map[j, i].temperature;
                    if (val < 0.2) Console.BackgroundColor = ConsoleColor.DarkBlue;
                    else if (val < 0.3) Console.BackgroundColor = ConsoleColor.Blue;
                    else if (val < 0.5) Console.BackgroundColor = ConsoleColor.DarkYellow;
                    else if (val < 0.6) Console.BackgroundColor = ConsoleColor.Yellow;
                    else if (val < 0.8) Console.BackgroundColor = ConsoleColor.DarkRed;
                    else Console.BackgroundColor = ConsoleColor.Red;

                    Console.Write((Math.Truncate(val * 10) / 1));
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.Write(" ");
                */
                
                Random r = new Random();
                for (int j = 0; j < world.map.GetLength(1); j++)
                {
                    double val = world.map[j, i].flora;
                    if (val == 0) Console.BackgroundColor = ConsoleColor.DarkBlue;
                    //else if (val < 0.3) Console.BackgroundColor = ConsoleColor.DarkRed;
                    //else if (val < 0.4) Console.BackgroundColor = ConsoleColor.DarkYellow;
                    //else if (val < 0.5) Console.BackgroundColor = ConsoleColor.Yellow;
                    //else if (val < 0.7) Console.BackgroundColor = ConsoleColor.DarkGreen;
                    //else Console.BackgroundColor = ConsoleColor.Green;
                    if (val >= 0 && r.NextDouble() <= val)
                        if (val <= 0.5)
                            Console.BackgroundColor = ConsoleColor.Red;
                        else if (val <= 0.7)
                            Console.BackgroundColor = ConsoleColor.Yellow;
                        else
                            Console.BackgroundColor = ConsoleColor.Green;
                    val = (Math.Truncate(val * 10) / 1);
                    if (val == 10) Console.Write("X");
                    else 
                        Console.Write(" ");
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                
                Console.WriteLine();
            }

            //TODO: Let he who is without sin cast the first stone
            //TODO: Matthew 7:1
            //Console.Write("Height");
            //Console.Write("                           Humidity");
            //Console.Write("                         Temperature");
            //Console.Write("                      Flora");

            //foreach (var e in entities)
            //{
            //    Console.SetCursorPosition((e as Creature).x * 2, (e as Creature).y);
            //    Console.Write("e ");
            //}
            //Console.SetCursorPosition(mapSize, mapSize);
        }

        World world;
        string cr_state = "";
    }
}
