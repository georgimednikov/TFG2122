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
            //world.ExportContent();
            //WorldToBmp();
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
                    validPosition = true;
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
                //Thread.Sleep(1000);
            }
            world.ExportContent();
        }


        public void WorldToBmp()
        {
            int scale = 4;
            Bitmap treeMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap floraMapMask = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap floraMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap heightMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap tempMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap hMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            double val;
            for (int i = 0; i < world.map.GetLength(0) * scale; i += scale)
            {
                for (int j = 0; j < world.map.GetLength(1) * scale; j += scale)
                {
                    val = world.map[j / scale, i / scale].height;
                    //*
                    if (val < 0.3) SetPixel(j, i, Color.DarkBlue, heightMap, scale);
                    else if (val < 0.5) SetPixel(j, i, Color.Blue, heightMap, scale);
                    else if (val == 0.5) SetPixel(j, i, Color.DarkGreen, heightMap, scale);
                    else if (val < 0.6) SetPixel(j, i, Color.Green, heightMap, scale);
                    else if (val < 0.7) SetPixel(j, i, Color.Yellow, heightMap, scale);
                    else if (val < 0.8) SetPixel(j, i, Color.LightYellow, heightMap, scale);
                    else SetPixel(j, i, Color.White, heightMap, scale);
                    /*/
                    val = (Math.Truncate(val * 10) / 10);
                    SetPixel(j, i, Color.FromArgb((int)(0 * 255), (int)(val * 255), (int)((1 - Math.Pow(val, 2)) * 255 / 2)), heightMap, scale);
                    //*/
                    val = world.map[j / scale, i / scale].humidity;

                    //*
                    if (val < 0.3) SetPixel(j, i, Color.DarkRed, hMap, scale);
                    else if (val < 0.4) SetPixel(j, i, Color.Red, hMap, scale);
                    else if (val < 0.5) SetPixel(j, i, Color.IndianRed, hMap, scale);
                    else if (val < 0.6) SetPixel(j, i, Color.MediumVioletRed, hMap, scale);
                    else if (val < 0.8) SetPixel(j, i, Color.Blue, hMap, scale);
                    else if (val < 1) SetPixel(j, i, Color.DarkBlue, hMap, scale);
                    /*/
                    val = (Math.Truncate(val * 10) / 10);
                    SetPixel(j, i, Color.FromArgb((int)((1 - val) * 255 / 2), (int)((0) * 255), (int)((val) * 255)), hMap, scale);
                    //*/

                    val = world.map[j / scale, i / scale].temperature;
                    //*
                    if (val < 0.2) SetPixel(j, i, Color.DarkBlue, tempMap, scale);
                    else if (val < 0.3) SetPixel(j, i, Color.Blue, tempMap, scale);
                    else if (val < 0.5) SetPixel(j, i, Color.Yellow, tempMap, scale);
                    else if (val < 0.6) SetPixel(j, i, Color.Orange, tempMap, scale);
                    else if (val < 0.8) SetPixel(j, i, Color.OrangeRed, tempMap, scale);
                    else SetPixel(j, i, Color.Red, tempMap, scale);
                    /*/
                    val = (Math.Truncate(val * 10) / 10);
                    SetPixel(j, i, Color.FromArgb((int)(val * 255), (int)((1 - val) * 255 / 2), (int)((1 - val) * 255)), tempMap, scale);
                    //*/

                    val = world.map[j / scale, i / scale].flora;
                    //int aux = 0;
                    if (val == 0)
                        if (world.map[j / scale, i / scale].isWater)
                        {
                            SetPixel(j, i, Color.DarkBlue, treeMap, scale);
                            SetPixel(j, i, Color.DarkBlue, floraMap, scale);
                            //aux = 1;
                        }
                        else SetPixel(j, i, Color.Black, floraMap, scale);
                    //*
                    else if (val < 0.1) SetPixel(j, i, Color.DarkRed, floraMap, scale);
                    else if (val < 0.2) SetPixel(j, i, Color.Red, floraMap, scale);
                    else if (val < 0.3) SetPixel(j, i, Color.OrangeRed, floraMap, scale);
                    else if (val < 0.4) SetPixel(j, i, Color.Orange, floraMap, scale);
                    else if (val < 0.5) SetPixel(j, i, Color.Yellow, floraMap, scale);
                    else if (val < 0.7) SetPixel(j, i, Color.YellowGreen, floraMap, scale);
                    else if (val < 1) SetPixel(j, i, Color.Green, floraMap, scale);
                    else SetPixel(j, i, Color.White, floraMap, scale);
                    /*/
                    val = (Math.Truncate(val * 10) / 10);
                    SetPixel(j, i, Color.FromArgb((int)(((1 - val) * (1 - aux)) * 255 / 2), (int)((val - (val * aux )) * 255), (int)((aux) * 255/2)), floraMap, scale);
                    //*/
                    float thres = 1.0f, thres2 = 0.7f;
                    if (val >= thres) SetPixel(j, i, Color.FromArgb(0, 255, 0), floraMapMask, scale);
                    else SetPixel(j, i, Color.FromArgb((int)(150 * (thres - val)), (int)(90 + (val * 165f / thres)), 0), floraMapMask, scale);

                    double h = world.map[j / scale, i / scale].height;

                    if (h >= thres2)
                    {
                        Color c = floraMapMask.GetPixel(j, i);
                        //SetPixel(j, i, Color.FromArgb(255, 255, 255), floraMapMask, scale);
                        SetPixel(j, i, Color.FromArgb((int)(c.R + ((h - thres2) / (1 - thres2) * (1 - (c.R / 255f))) * 255), (int)(c.G + ((h - thres2) / (1 - thres2) * (1 - (c.G / 255f))) * 255), (int)(c.B + ((h - thres2) / (1 - thres2) * (1 - (c.B / 255f)))) * 255), floraMapMask, scale);
                    }

                    Plant plant = world.map[j / scale, i / scale].plant;
                    if (plant as Grass != null)
                        SetPixel(j, i, Color.DarkOliveGreen, treeMap, scale);
                    else if (plant as Bush != null)
                        SetPixel(j, i, Color.ForestGreen, treeMap, scale);
                    else if (plant as Tree != null)
                        SetPixel(j, i, Color.LawnGreen, treeMap, scale);
                    else if (plant as EdibleTree != null)
                        SetPixel(j, i, Color.Red, treeMap, scale);
                    //else SetPixel(j, i, Color.Black, treeMap, scale);
                }
            }

            treeMap.Save("treeTest.png");
            floraMap.Save("flora.bmp");
            floraMapMask.Save("floraMask.bmp");
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
