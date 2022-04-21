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
    public class ConsoleSimulation : Simulation
    {
        override public void Init(int years, int species, int individuals, string dataDir, string exportDir)
        {
            base.Init(years, species, individuals, dataDir, exportDir);
            //WorldToBmp();
            Console.WriteLine("Simulation Init done");
        }

        override public void Init(int years, int species, int individuals, string uniParamsFile = null, string chromosomeFile = null, string abilitiesFile = null, string sGeneWeightFile = null, string minSimilarityFile = null, string worldFile = null, string highMap = null, string exportDir = null)
        {
            base.Init(years, species, individuals, uniParamsFile, chromosomeFile, abilitiesFile, sGeneWeightFile, minSimilarityFile, worldFile, exportDir);
            Console.WriteLine("Simulation Init done");
        }

        override public void Run()
        {
            int YearTicks = world.YearToTick(1.0f);
            DateTime time = DateTime.Now;
            int ticks = world.YearToTick(UserInfo.Years);
            int i = 1;
            for (; i <= ticks; i++)
            {
                if (!world.Tick())
                {
                    CreateCreatures();
                    Console.WriteLine("APOCALYPSIS: Generating new set of creatures");
                    Export();
                };

                Console.WriteLine("Num Creatures: {1} Ticks: {0}/{2} ", i, world.Creatures.Count, ticks);
                //Render();
                //Thread.Sleep(1000);
                if (i % YearTicks == 0)
                    Console.WriteLine("A Year has passed");
            }
            DateTime time2 = DateTime.Now;
            Console.Write("Estimated Time for "+ UserInfo.Years + " years will be: " + ((time2-time).TotalMilliseconds/(i-1) * world.YearToTick(UserInfo.Years))/360000/24 + " days.\n");
            Console.Write("Simulation ended, ticks elapsed: " + i + "\n");
        }

        override public void Export()
        {
            base.Export();
            Console.Write("Simulation data has been exported");
        }
        /// <summary>
        /// Asks the user where to look for the files containing the different values for the calculation of the chromosme, genes and stats,
        /// as well as the folder in which to save the resulting species. This method uses the program's console to do so.
        /// </summary>
        /// <returns></returns>
        static public bool AskInfoUsingConsole()
        {
            do
            {
                Console.WriteLine("Input a valid directory containing the necessary data files for the program (chromosome.json, etc.):\n");
                UserInfo.DataDirectory = Console.ReadLine() + "\\";
                Console.Clear();
            } while (!Directory.Exists(UserInfo.DataDirectory));

            Console.WriteLine("Input a valid directory in which the resulting data will be saved:\n");
            UserInfo.ExportDirectory = Console.ReadLine() + "\\";
            Console.Clear();
            if (!Directory.Exists(UserInfo.ExportDirectory))
                UserInfo.ExportDirectory = Directory.CreateDirectory(UserInfo.ExportDirectory).FullName;

            do
            {
                Console.WriteLine("Input how many years of evolution are going to be simulated:");
                string input = Console.ReadLine();
                UserInfo.Years = -1;
                if (input != "") UserInfo.Years = Int32.Parse(input);
                Console.Clear();
            } while (UserInfo.Years < 0);

            int minSize = UserInfo.MinWorldSize();
            do
            {
                Console.WriteLine("Input how big in squares the world is going to be. Must be a number larger than: " + minSize + "\n");
                string input = Console.ReadLine();
                UserInfo.Size = -1;
                if (input != "") UserInfo.Size = Int32.Parse(input);
                Console.Clear();
            } while (UserInfo.Size < minSize);

            int minSpecies = UserInfo.MinSpeciesAmount();
            do
            {
                Console.WriteLine("Input how many species are going to be created initially. Must be a number larger than: " + minSpecies + "\n");
                string input = Console.ReadLine();
                UserInfo.Species = -1;
                if (input != "") UserInfo.Species = Int32.Parse(input);
                Console.Clear();
            } while (UserInfo.Species < minSpecies);

            int minIndividuals = UserInfo.MinIndividualsAmount();
            do
            {
                Console.WriteLine("Input how individuals per species are going to be created. Must be a number larger than: " + minIndividuals + "\n");
                string input = Console.ReadLine();
                UserInfo.Individuals = -1;
                if (input != "") UserInfo.Individuals = Int32.Parse(input);
                Console.Clear();
            } while (UserInfo.Individuals < minIndividuals);

            return true;
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
                    double h = world.map[j / scale, i / scale].height;

                    if (val >= thres) SetPixel(j, i, Color.FromArgb(0, 255, 0), floraMapMask, scale);
                    else SetPixel(j, i, Color.FromArgb((int)(150 * (thres - val)), (int)(90 + (val * 165f / thres)), 0), floraMapMask, scale);


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
                    switch (val % 20)
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
                        case 10:
                            SetPixel(j, i, Color.Chocolate, voronoiMap, scale);
                            break;
                        case 11:
                            SetPixel(j, i, Color.Chartreuse, voronoiMap, scale);
                            break;
                        case 12:
                            SetPixel(j, i, Color.BurlyWood, voronoiMap, scale);
                            break;
                        case 13:
                            SetPixel(j, i, Color.DarkKhaki, voronoiMap, scale);
                            break;
                        case 14:
                            SetPixel(j, i, Color.DarkOrchid, voronoiMap, scale);
                            break;
                        case 15:
                            SetPixel(j, i, Color.Firebrick, voronoiMap, scale);
                            break;
                        case 16:
                            SetPixel(j, i, Color.DeepSkyBlue, voronoiMap, scale);
                            break;
                        case 17:
                            SetPixel(j, i, Color.IndianRed, voronoiMap, scale);
                            break;
                        case 18:
                            SetPixel(j, i, Color.Ivory, voronoiMap, scale);
                            break;
                        case 19:
                            SetPixel(j, i, Color.LimeGreen, voronoiMap, scale);
                            break;
                    }
                    #endregion
                    if ((j / scale) % 32 == 0 || (i / scale) % 32 == 0)
                    {
                        SetPixel(j, i, Color.White, voronoiMap, scale / 2);
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
    }
}
