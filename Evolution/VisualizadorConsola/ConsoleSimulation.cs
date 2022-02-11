using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using EvolutionSimulation;
using EvolutionSimulation.Entities;

namespace VisualizadorConsola
{
    /// <summary>
    /// Implementation of a simulation with console output
    /// </summary>
    public class ConsoleSimulation : ISimulation
    {
        public void Init()
        {
            //*/
            world = new World();
            world.Init(16);
            Animal c = world.CreateCreature();
            c.Init(world, 5, 5);
            c = world.CreateCreature();
            c.Init(world, 4, 4);
            /*/
            Animal c = CreateEntity<Animal>();
            c.chromosome.PrintChromosome();
            c.Init(world, 5, 5);
            //c = CreateEntity<Animal>();
            //c.Init(world, 4, 4);
            */
        }


        public void Run()
        {
            while (true)
            {
                world.Tick();
                Render();
                Thread.Sleep(1000);
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
                for (int j = 0; j < world.map.GetLength(1); j++)
                {
                    double val = world.map[j, i].height;
                    if (val < 0.3) Console.BackgroundColor = ConsoleColor.Black;
                    else if (val < 0.5) Console.BackgroundColor = ConsoleColor.DarkBlue;
                    else if (val == 0.5) Console.BackgroundColor = ConsoleColor.DarkGreen;
                    else if (val < 0.6) Console.BackgroundColor = ConsoleColor.DarkCyan;
                    else if (val < 0.7) Console.BackgroundColor = ConsoleColor.Blue;
                    else if (val < 0.8) Console.BackgroundColor = ConsoleColor.Cyan;
                    else Console.BackgroundColor = ConsoleColor.White;

                    Console.Write((Math.Truncate(world.map[j, i].height * 10) / 1));
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.Write(" ");
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
                Console.Write(" ");
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

                //Random r = new Random();
                //for (int j = 0; j < world.map.GetLength(1); j++)
                //{
                //    double val = world.map[j, i].flora;
                //    if (val == 0 && world.map[j, i].height < 0.5) Console.BackgroundColor = ConsoleColor.DarkBlue;
                //    //else if (val < 0.3) Console.BackgroundColor = ConsoleColor.DarkRed;
                //    //else if (val < 0.4) Console.BackgroundColor = ConsoleColor.DarkYellow;
                //    //else if (val < 0.5) Console.BackgroundColor = ConsoleColor.Yellow;
                //    //else if (val < 0.7) Console.BackgroundColor = ConsoleColor.DarkGreen;
                //    //else Console.BackgroundColor = ConsoleColor.Green;

                //    if (val >= 0 && RandomGenerator.NextDouble() <= val)
                //        if (val <= 0.3)
                //            Console.BackgroundColor = ConsoleColor.Red;
                //        else if (val <= 0.35)
                //            Console.BackgroundColor = ConsoleColor.Yellow;
                //        else
                //            Console.BackgroundColor = ConsoleColor.Green;
                //    val = (Math.Truncate(val * 10) / 1);
                //    if (val == 10) Console.Write("X");
                //    else
                //        Console.Write(" ");
                //    Console.BackgroundColor = ConsoleColor.Black;
                //}

                Console.WriteLine();
            }

            //TODO: Let he who is without sin cast the first stone
            //TODO: Matthew 7:1
            Console.Write("Height");
            Console.Write("                           Humidity");
            Console.Write("                         Temperature");
            Console.Write("                      Flora");

            foreach (var e in world.Creatures)
            {
                Console.SetCursorPosition(e.x, e.y);
                Console.Write("e");
            }
            //Console.SetCursorPosition(world.map.GetLength(0), world.map.GetLength(0));
        }

        public void WorldToBmp()
        {

            Bitmap treeMap = new Bitmap(world.map.GetLength(0), world.map.GetLength(0), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap floraMap = new Bitmap(world.map.GetLength(0), world.map.GetLength(0), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap heightMap = new Bitmap(world.map.GetLength(0), world.map.GetLength(0), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap tempMap = new Bitmap(world.map.GetLength(0), world.map.GetLength(0), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap hMap = new Bitmap(world.map.GetLength(0), world.map.GetLength(0), System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Console.Clear();
            for (int i = 0; i < world.map.GetLength(0); i++)
            {
                for (int j = 0; j < world.map.GetLength(1); j++)
                {
                    double val = world.map[j, i].height;
                    if (val < 0.3) heightMap.SetPixel(j, i, Color.DarkBlue);
                    else if (val < 0.5) heightMap.SetPixel(j, i, Color.Blue);
                    else if (val == 0.5) heightMap.SetPixel(j, i, Color.DarkGreen);
                    else if (val < 0.6) heightMap.SetPixel(j, i, Color.Green);
                    else if (val < 0.7) heightMap.SetPixel(j, i, Color.Yellow);
                    else if (val < 0.8) heightMap.SetPixel(j, i, Color.LightYellow);
                    else heightMap.SetPixel(j, i, Color.White);
                }

                for (int j = 0; j < world.map.GetLength(1); j++)
                {
                    double val = world.map[j, i].humidity;

                    if (val < 0.3) hMap.SetPixel(j, i, Color.DarkRed);
                    else if (val < 0.4) hMap.SetPixel(j, i, Color.Red);
                    else if (val < 0.5) hMap.SetPixel(j, i, Color.IndianRed);
                    else if (val < 0.6) hMap.SetPixel(j, i, Color.MediumVioletRed);
                    else if (val < 0.8) hMap.SetPixel(j, i, Color.Blue);
                    else if (val < 1) hMap.SetPixel(j, i, Color.DarkBlue);
                }

                for (int j = 0; j < world.map.GetLength(1); j++)
                {
                    double val = world.map[j, i].temperature;
                    if (val < 0.2) tempMap.SetPixel(j, i, Color.DarkBlue);
                    else if (val < 0.3) tempMap.SetPixel(j, i, Color.Blue);
                    else if (val < 0.5) tempMap.SetPixel(j, i, Color.Yellow);
                    else if (val < 0.6) tempMap.SetPixel(j, i, Color.Orange);
                    else if (val < 0.8) tempMap.SetPixel(j, i, Color.OrangeRed);
                    else tempMap.SetPixel(j, i, Color.Red);
                }

                for (int j = 0; j < world.map.GetLength(1); j++)
                {
                    double val = world.map[j, i].flora;
                    if (val == 0)
                        if (world.map[j, i].height < 0.5) { treeMap.SetPixel(j, i, Color.DarkBlue); floraMap.SetPixel(j, i, Color.DarkBlue); }
                        else floraMap.SetPixel(j, i, Color.Black);
                    else if (val < 0.1) floraMap.SetPixel(j, i, Color.DarkRed);
                    else if (val < 0.2) floraMap.SetPixel(j, i, Color.Red);
                    else if (val < 0.3) floraMap.SetPixel(j, i, Color.OrangeRed);
                    else if (val < 0.4) floraMap.SetPixel(j, i, Color.Orange);
                    else if (val < 0.5) floraMap.SetPixel(j, i, Color.Yellow);
                    else if (val < 0.7) floraMap.SetPixel(j, i, Color.YellowGreen);
                    else floraMap.SetPixel(j, i, Color.Green);

                    if (val >= 0 && RandomGenerator.NextDouble() <= val)
                        if (val <= 0.3)
                            treeMap.SetPixel(j, i, Color.DarkOliveGreen);
                        else if (val <= 0.35)
                            treeMap.SetPixel(j, i, Color.ForestGreen);
                        else if (val < 0.7)
                            treeMap.SetPixel(j, i, Color.LawnGreen);
                        else
                            treeMap.SetPixel(j, i, Color.LimeGreen);
                }
            }

            treeMap.Save("treeTest.bmp");
            floraMap.Save("flora.bmp");
            heightMap.Save("height.bmp");
            tempMap.Save("temp.bmp");
            hMap.Save("humidity.bmp");
        }

        World world;
    }
}
