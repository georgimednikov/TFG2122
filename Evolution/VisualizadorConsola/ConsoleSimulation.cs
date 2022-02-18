using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using EvolutionSimulation;
using EvolutionSimulation.Entities;
using EvolutionSimulation.Entities.Status;

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
            Console.WriteLine("\n");
            
            Animal c = world.CreateCreature<Animal>();
            c.Init(world, 5, 5);
            //c = world.CreateCreature<Animal>();
            //c.Init(world, 4, 4);

            c.AddStatus(new Poison(20, 5));
            
            //Corpse corpse = world.CreateStableEntity<Corpse>();
            //corpse.Init(world, 5, 3, 3, 40, 70, 80);
        }


        public void Run()
        {
            //WorldToBmp();
            while (true)
            {
                world.Tick();
                //Render();
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

                    if (world.map[j, i].height < 0.5f) Console.BackgroundColor = ConsoleColor.Magenta;

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

            foreach (var e in world.StableEntities)
            {
                if (e.GetType() == typeof(Corpse))
                {
                    Console.SetCursorPosition(e.x, e.y);
                    Console.Write("x");
                }
            }
            //Console.SetCursorPosition(world.map.GetLength(0), world.map.GetLength(0));
        }

        public void WorldToBmp()
        {
            int scale = 2;
            Bitmap treeMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap floraMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap heightMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap tempMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap hMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            for (int i = 0; i < world.map.GetLength(0) * scale; i += scale)
            {
                for (int j = 0; j < world.map.GetLength(1) * scale; j += scale)
                {
                    double val = world.map[j/ scale, i/ scale].height;
                    if (val < 0.3) SetPixel(j, i, Color.DarkBlue, heightMap, scale);
                    else if (val < 0.5) SetPixel(j, i, Color.Blue, heightMap, scale);
                    else if (val == 0.5) SetPixel(j, i, Color.DarkGreen, heightMap, scale);
                    else if (val < 0.6) SetPixel(j, i, Color.Green, heightMap, scale);
                    else if (val < 0.7) SetPixel(j, i, Color.Yellow, heightMap, scale);
                    else if (val < 0.8) SetPixel(j, i, Color.LightYellow, heightMap, scale);
                    else SetPixel(j, i, Color.White, heightMap, scale);

                     val = world.map[j / scale, i / scale].humidity;

                    if (val < 0.3) SetPixel(j, i, Color.DarkRed, hMap, scale);
                    else if (val < 0.4) SetPixel(j, i, Color.Red, hMap, scale);
                    else if (val < 0.5) SetPixel(j, i, Color.IndianRed, hMap, scale);
                    else if (val < 0.6) SetPixel(j, i, Color.MediumVioletRed, hMap, scale);
                    else if (val < 0.8) SetPixel(j, i, Color.Blue, hMap, scale);
                    else if (val < 1) SetPixel(j, i, Color.DarkBlue, hMap, scale);

                     val = world.map[j / scale, i / scale].temperature;
                    if (val < 0.2) SetPixel(j, i, Color.DarkBlue, tempMap, scale);
                    else if (val < 0.3) SetPixel(j, i, Color.Blue, tempMap, scale);
                    else if (val < 0.5) SetPixel(j, i, Color.Yellow, tempMap, scale);
                    else if (val < 0.6) SetPixel(j, i, Color.Orange, tempMap, scale);
                    else if (val < 0.8) SetPixel(j, i, Color.OrangeRed, tempMap, scale);
                    else SetPixel(j, i, Color.Red, tempMap, scale);

                    val = world.map[j / scale, i / scale].flora;
                    if (val == 0)
                        if (world.map[j / scale, i / scale].height < 0.5) { SetPixel(j, i, Color.DarkBlue, treeMap, scale); SetPixel(j, i, Color.DarkBlue, floraMap, scale); }
                        else SetPixel(j, i, Color.Black, floraMap, scale);
                    else if (val < 0.1) SetPixel(j, i, Color.DarkRed, floraMap, scale);
                    else if (val < 0.2) SetPixel(j, i, Color.Red, floraMap, scale);
                    else if (val < 0.3) SetPixel(j, i, Color.OrangeRed, floraMap, scale);
                    else if (val < 0.4) SetPixel(j, i, Color.Orange, floraMap, scale);
                    else if (val < 0.5) SetPixel(j, i, Color.Yellow, floraMap, scale);
                    else if (val < 0.7) SetPixel(j, i, Color.YellowGreen, floraMap, scale);
                    else if (val < 1) SetPixel(j, i, Color.Green, floraMap, scale);
                    else SetPixel(j, i, Color.White, floraMap, scale);

                    Plant plant = world.map[j / scale, i / scale].plant;
                    if (plant as EvolutionSimulation.Grass != null)
                        SetPixel(j, i, Color.DarkOliveGreen, treeMap, scale);
                    else if (plant as EvolutionSimulation.Bush != null)
                        SetPixel(j, i, Color.ForestGreen, treeMap, scale);
                    else if (plant as EvolutionSimulation.Tree != null)
                        SetPixel(j, i, Color.LawnGreen, treeMap, scale);
                    else if (plant as EvolutionSimulation.EdibleTree != null)
                        SetPixel(j, i, Color.Red, treeMap, scale);
                }
            }

            treeMap.Save("treeTest.bmp");
            floraMap.Save("flora.bmp");
            heightMap.Save("height.bmp");
            tempMap.Save("temp.bmp");
            hMap.Save("humidity.bmp");
        }

        void SetPixel(int x, int y, Color color, Bitmap bitmap, int scale = 2)
        {
            for (int i = 0; i < scale; i++)
            {
                for (int j = 0; j < scale; j++)
                {
                    bitmap.SetPixel(x + i, y + j, color);
                }
            }
        }

        World world;
    }
}
