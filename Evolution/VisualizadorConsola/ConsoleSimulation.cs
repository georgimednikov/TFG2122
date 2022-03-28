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
            world.Init(512);//UserInfo.Size);
            //world.ExportContent();
            //WorldToBmp();
            //A minimum distance to leave in between species spawn points to give them some room.
            //Calculated based on the world size and amount of species to spawn, and then reduced by
            //a value to give room in the world and not fill it in a homogenous manner.
            int minSpawnDist = UserInfo.Size / UserInfo.Species / 15;

            //List with previous spawn positions, to know if a new spot is too close to another one used.
            List<Tuple<int, int>> spawnPositions = new List<Tuple<int, int>>();
            int x, y;

            for (int i = 0; i < UserInfo.Species; i++)
            {
                bool validPosition;
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
            Bitmap holdRidgeMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Bitmap voronoiMap = new Bitmap(world.map.GetLength(0) * scale, world.map.GetLength(0) * scale, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            double val;
            for (int i = 0; i < world.map.GetLength(0) * scale; i += scale)
            {
                for (int j = 0; j < world.map.GetLength(1) * scale; j += scale)
                {
                    #region HeightMap
                    val = world.map[j / scale, i / scale].height;
                    if (val < 0.3) SetPixel(j, i, Color.DarkBlue, heightMap, scale);
                    else if (val < 0.5) SetPixel(j, i, Color.Blue, heightMap, scale);
                    else if (val == 0.5) SetPixel(j, i, Color.DarkGreen, heightMap, scale);
                    else if (val < 0.6) SetPixel(j, i, Color.Green, heightMap, scale);
                    else if (val < 0.7) SetPixel(j, i, Color.Yellow, heightMap, scale);
                    else if (val < 0.8) SetPixel(j, i, Color.LightYellow, heightMap, scale);
                    else SetPixel(j, i, Color.White, heightMap, scale);
                    val = world.map[j / scale, i / scale].humidity;
                    #endregion

                    #region HumidityMap
                    if (val < 0.3) SetPixel(j, i, Color.DarkRed, hMap, scale);
                    else if (val < 0.4) SetPixel(j, i, Color.Red, hMap, scale);
                    else if (val < 0.5) SetPixel(j, i, Color.IndianRed, hMap, scale);
                    else if (val < 0.6) SetPixel(j, i, Color.MediumVioletRed, hMap, scale);
                    else if (val < 0.8) SetPixel(j, i, Color.Blue, hMap, scale);
                    else if (val < 1) SetPixel(j, i, Color.DarkBlue, hMap, scale);
                    #endregion

                    #region TemperatureMap
                    val = world.map[j / scale, i / scale].temperature;
                    if (val < 0.2) SetPixel(j, i, Color.DarkBlue, tempMap, scale);
                    else if (val < 0.3) SetPixel(j, i, Color.Blue, tempMap, scale);
                    else if (val < 0.5) SetPixel(j, i, Color.Yellow, tempMap, scale);
                    else if (val < 0.6) SetPixel(j, i, Color.Orange, tempMap, scale);
                    else if (val < 0.8) SetPixel(j, i, Color.OrangeRed, tempMap, scale);
                    else SetPixel(j, i, Color.Red, tempMap, scale);
                    #endregion

                    #region FloraMap
                    val = world.map[j / scale, i / scale].flora;
                    if (val == 0)
                        if (world.map[j / scale, i / scale].isWater)
                        {
                            SetPixel(j, i, Color.DarkBlue, treeMap, scale);
                            SetPixel(j, i, Color.DarkBlue, floraMap, scale);
                        }
                        else SetPixel(j, i, Color.Black, floraMap, scale);
                    else if (val < 0.1) SetPixel(j, i, Color.DarkRed, floraMap, scale);
                    else if (val < 0.2) SetPixel(j, i, Color.Red, floraMap, scale);
                    else if (val < 0.3) SetPixel(j, i, Color.OrangeRed, floraMap, scale);
                    else if (val < 0.4) SetPixel(j, i, Color.Orange, floraMap, scale);
                    else if (val < 0.5) SetPixel(j, i, Color.Yellow, floraMap, scale);
                    else if (val < 0.7) SetPixel(j, i, Color.YellowGreen, floraMap, scale);
                    else if (val < 1) SetPixel(j, i, Color.Green, floraMap, scale);
                    else SetPixel(j, i, Color.White, floraMap, scale);
                    #endregion

                    #region TerrainTexture
                    float thres = 1.0f, thres2 = 0.7f;
                    if (val >= thres) SetPixel(j, i, Color.FromArgb(0, 255, 0), floraMapMask, scale);
                    else SetPixel(j, i, Color.FromArgb((int)(150 * (thres - val)), (int)(90 + (val * 165f / thres)), 0), floraMapMask, scale);

                    double h = world.map[j / scale, i / scale].height;

                    if (h >= thres2)
                    {
                        Color c = floraMapMask.GetPixel(j, i);
                        SetPixel(j, i, Color.FromArgb((int)(c.R + ((h - thres2) / (1 - thres2) * (1 - (c.R / 255f))) * 255), (int)(c.G + ((h - thres2) / (1 - thres2) * (1 - (c.G / 255f))) * 255), (int)(c.B + ((h - thres2) / (1 - thres2) * (1 - (c.B / 255f)))) * 255), floraMapMask, scale);
                    }
                    #endregion

                    #region PlantMap
                    Plant plant = world.map[j / scale, i / scale].plant;
                    if (plant as Grass != null)
                        SetPixel(j, i, Color.DarkOliveGreen, treeMap, scale);
                    else if (plant as Bush != null)
                        SetPixel(j, i, Color.ForestGreen, treeMap, scale);
                    else if (plant as Tree != null)
                        SetPixel(j, i, Color.LawnGreen, treeMap, scale);
                    else if (plant as EdibleTree != null)
                        SetPixel(j, i, Color.Red, treeMap, scale);

                    if (world.map[j / scale, i / scale].isWater)
                    {
                        SetPixel(j, i, Color.FromArgb(0, 0, 255), holdRidgeMap, scale);
                        
                    }
                    #endregion

                    #region HoldridgeMap
                    val = world.map[j / scale, i / scale].temperature;
                    double val2 = world.map[j / scale, i / scale].humidity;
                    //Mapa usando Holdridge de 39 Biomas
                    //Polar
                    if (val < 0.1)
                    {
                        if (val2 < 0.15) SetPixel(j, i, Color.FromArgb(191, 191, 191), holdRidgeMap, scale); //Polar Desert
                        else SetPixel(j, i, Color.FromArgb(255, 255, 255), holdRidgeMap, scale); //Polar Ice
                    }
                    //Subpolar
                    else if (val < 0.2)
                    {
                        if (val2 < 0.15) SetPixel(j, i, Color.FromArgb(128, 128, 128), holdRidgeMap, scale); //Subpolar Dry Tundra
                        else if (val2 < 0.25) SetPixel(j, i, Color.FromArgb(96, 128, 128), holdRidgeMap, scale); //Subpolar Moist Tundra
                        else if (val2 < 0.35) SetPixel(j, i, Color.FromArgb(64, 128, 128), holdRidgeMap, scale); //Subpolar Wet Tundra
                        else SetPixel(j, i, Color.FromArgb(32, 128, 128), holdRidgeMap, scale); //Subpolar Rain Tundra
                    }
                    //Boreal
                    else if (val < 0.3)
                    {
                        if (val2 < 0.15) SetPixel(j, i, Color.FromArgb(160, 160, 128), holdRidgeMap, scale); //Boreal Desert
                        else if (val2 < 0.25) SetPixel(j, i, Color.FromArgb(128, 160, 128), holdRidgeMap, scale); //Boreal Dry Scrub
                        else if (val2 < 0.35) SetPixel(j, i, Color.FromArgb(96, 160, 128), holdRidgeMap, scale); //Boreal Moist Forest
                        else if (val2 < 0.45) SetPixel(j, i, Color.FromArgb(64, 160, 144), holdRidgeMap, scale); //Boreal Wet Forest
                        else SetPixel(j, i, Color.FromArgb(32, 160, 192), holdRidgeMap, scale); //Boreal Rain Forest
                    }
                    //Cool Temperate
                    else if (val < 0.35)
                    {
                        if (val2 < 0.15) SetPixel(j, i, Color.FromArgb(192, 192, 128), holdRidgeMap, scale); //Cool Temperate Desert
                        else if (val2 < 0.25) SetPixel(j, i, Color.FromArgb(160, 192, 128), holdRidgeMap, scale); //Cool Temperate Desert Scrub
                        else if (val2 < 0.35) SetPixel(j, i, Color.FromArgb(128, 192, 128), holdRidgeMap, scale); //Cool Temperate Steppe
                        else if (val2 < 0.45) SetPixel(j, i, Color.FromArgb(96, 192, 128), holdRidgeMap, scale); //Cool Temperate Moist Forest
                        else if (val2 < 0.70) SetPixel(j, i, Color.FromArgb(64, 192, 144), holdRidgeMap, scale); //Cool Temperate Wet Forest
                        else SetPixel(j, i, Color.FromArgb(32, 192, 192), holdRidgeMap, scale); //Cool Temperate Rain Forest
                    }
                    //Warm Temperate
                    else if (val < 0.65)
                    {
                        if (val2 < 0.15) SetPixel(j, i, Color.FromArgb(224, 224, 128), holdRidgeMap, scale); //Warm Temperate Desert
                        else if (val2 < 0.25) SetPixel(j, i, Color.FromArgb(192, 224, 128), holdRidgeMap, scale); //Warm Temperate Desert Scrub
                        else if (val2 < 0.35) SetPixel(j, i, Color.FromArgb(160, 224, 128), holdRidgeMap, scale); //Warm Temperate Thorn Scrub
                        else if (val2 < 0.45) SetPixel(j, i, Color.FromArgb(128, 224, 128), holdRidgeMap, scale); //Warm Temperate Dry Forest
                        else if (val2 < 0.70) SetPixel(j, i, Color.FromArgb(96, 224, 128), holdRidgeMap, scale); //Warm Temperate Moist Forest
                        else if (val2 < 0.85) SetPixel(j, i, Color.FromArgb(64, 224, 144), holdRidgeMap, scale); //Warm Temperate Wet Forest
                        else SetPixel(j, i, Color.FromArgb(32, 224, 192), holdRidgeMap, scale); //Warm Temperate Rain Forest
                    }
                    //Subtropical
                    else if (val < 0.9)
                    {
                        if (val2 < 0.15) SetPixel(j, i, Color.FromArgb(240, 240, 128), holdRidgeMap, scale); //Subtropical Desert
                        else if (val2 < 0.25) SetPixel(j, i, Color.FromArgb(208, 240, 128), holdRidgeMap, scale); //Subtropical Desert Scrub
                        else if (val2 < 0.35) SetPixel(j, i, Color.FromArgb(176, 240, 128), holdRidgeMap, scale); //Subtropical Thorn Woodland
                        else if (val2 < 0.45) SetPixel(j, i, Color.FromArgb(128, 240, 128), holdRidgeMap, scale); //Subtropical Dry Forest
                        else if (val2 < 0.70) SetPixel(j, i, Color.FromArgb(96, 240, 128), holdRidgeMap, scale); //Subtropical Moist Forest
                        else if (val2 < 0.85) SetPixel(j, i, Color.FromArgb(64, 240, 144), holdRidgeMap, scale); //Subtropical Wet Forest
                        else SetPixel(j, i, Color.FromArgb(32, 240, 176), holdRidgeMap, scale); //Subtropical Rain Forest
                    }
                    //Tropical
                    else
                    {
                        if (val2 < 0.15) SetPixel(j, i, Color.FromArgb(255, 255, 128), holdRidgeMap, scale); //Tropical Desert
                        else if (val2 < 0.25) SetPixel(j, i, Color.FromArgb(224, 255, 128), holdRidgeMap, scale); //Tropical Desert Scrub
                        else if (val2 < 0.35) SetPixel(j, i, Color.FromArgb(192, 255, 128), holdRidgeMap, scale); //Tropical Thorn Woodland
                        else if (val2 < 0.45) SetPixel(j, i, Color.FromArgb(160, 255, 128), holdRidgeMap, scale); //Tropical Very Dry Forest
                        else if (val2 < 0.70) SetPixel(j, i, Color.FromArgb(128, 255, 128), holdRidgeMap, scale); //Tropical Dry Forest
                        else if (val2 < 0.85) SetPixel(j, i, Color.FromArgb(96, 255, 128), holdRidgeMap, scale); //Tropical Moist Forest
                        else if (val2 < 0.95) SetPixel(j, i, Color.FromArgb(60, 255, 144), holdRidgeMap, scale); //Tropical Wet Forest
                        else SetPixel(j, i, Color.FromArgb(32, 255, 160), holdRidgeMap, scale); //Tropical Rain Forest
                    }
                    #endregion
                    
                    #region VoronoiDiagram
                    val = world.map[j / scale, i / scale].regionId;
                    switch (val % 10)
                    {
                        case 0:
                            SetPixel(j, i, Color.Red, voronoiMap, scale);
                            break;
                        case 1:
                            SetPixel(j, i, Color.Blue, voronoiMap, scale);
                            break;
                        case 2:
                            SetPixel(j, i, Color.Green, voronoiMap, scale);
                            break;
                        case 3:
                            SetPixel(j, i, Color.Violet, voronoiMap, scale);
                            break;
                        case 4:
                            SetPixel(j, i, Color.Cyan, voronoiMap, scale);
                            break;
                        case 5:
                            SetPixel(j, i, Color.Lime, voronoiMap, scale);
                            break;
                        case 6:
                            SetPixel(j, i, Color.Brown, voronoiMap, scale);
                            break;
                        case 7:
                            SetPixel(j, i, Color.Yellow, voronoiMap, scale);
                            break;
                        case 8:
                            SetPixel(j, i, Color.Orange, voronoiMap, scale);
                            break;
                        case 9:
                            SetPixel(j, i, Color.LightGoldenrodYellow, voronoiMap, scale);
                            break;
                    }
                    #endregion
                    if ((j / scale) % 32 == 0 || (i / scale) % 32 == 0)
                    {
                        SetPixel(j, i, Color.White, voronoiMap, scale);
                    }
                }
            }

            for (int t = 0; t < world.highMap.Count; t++)
            {
                Vector2 vector = world.highMap[t].spawnPoint;
                SetPixel((int)vector.X * scale, (int)vector.Y * scale, Color.Black, voronoiMap, scale);
            }

            treeMap.Save("treeTest.png");
            floraMap.Save("flora.bmp");
            floraMapMask.Save("floraMask.bmp");
            heightMap.Save("height.bmp");
            tempMap.Save("temp.bmp");
            hMap.Save("humidity.bmp");
            holdRidgeMap.Save("biome.bmp");
            voronoiMap.Save("VoronoiDiagram.bmp");
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
