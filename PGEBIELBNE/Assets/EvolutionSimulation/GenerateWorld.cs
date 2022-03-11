using UnityEngine;
using EvolutionSimulation;
using System.Collections.Generic;

namespace UnitySimulation
{
    [RequireComponent(typeof(Terrain))]
    public class GenerateWorld : MonoBehaviour
    {
        public GameObject tree, edibleTree, bush, grass;
        public TextAsset world;
        public float heightMultiplier;
        World.MapData[,] map;

        void Start()
        {
            map = Newtonsoft.Json.JsonConvert.DeserializeObject<World.MapData[,]>(world.text);

            //Terrain terrain = GetComponent<Terrain>();
            //terrain.terrainData.size = new Vector3(map.GetLength(0), terrain.terrainData.size.y, map.GetLength(1));
            UpdateMeshVertices(map);
            GenerateFlora(map);
        }

        private void GenerateFlora(World.MapData[,] heightMap)
        {
            List<TreeInstance> trees = new List<TreeInstance>();
            TerrainData terrain = GetComponent<Terrain>().terrainData;
            terrain.SetTreeInstances(trees.ToArray(), true);
            int tileDepth = heightMap.GetLength(0);
            int tileWidth = heightMap.GetLength(1);
            int size = terrain.heightmapResolution;

            for (int z = 0; z < tileDepth; z++)
                for (int x = 0; x < tileWidth; x++)
                {
                    if (heightMap[z, x].plant == null) continue;

                    TreeInstance tree = new TreeInstance();
                    switch (heightMap[z, x].plant.type)
                    {
                        case EvolutionSimulation.Entities.Plant.PlantType.Grass:
                            tree.prototypeIndex = 0;
                            break;
                        case EvolutionSimulation.Entities.Plant.PlantType.Bush:
                            tree.prototypeIndex = 1;
                            break;
                        case EvolutionSimulation.Entities.Plant.PlantType.Tree:
                            tree.prototypeIndex = 2;
                            break;
                        case EvolutionSimulation.Entities.Plant.PlantType.EdibleTree:
                            tree.prototypeIndex = 3;
                            break;
                        default:
                            tree.prototypeIndex = -1;
                            break;
                    }


                    tree.color = Color.white;
                    tree.lightmapColor = Color.white;
                    tree.heightScale = 1;
                    tree.widthScale = 1;
                    tree.position = new Vector3((x + 1.5f) / (float)tileDepth, 0, (z + 1.5f) / (float)tileWidth);
                    Debug.Log("Pongo una puta planta en: " + tree.position);
                    trees.Add(tree);
                }

            terrain.SetTreeInstances(trees.ToArray(), true);
        }

        private void UpdateMeshVertices(World.MapData[,] heightMap)
        {
            TerrainData terrain = GetComponent<Terrain>().terrainData;
            int size = terrain.heightmapResolution;
            int tileDepth = heightMap.GetLength(0);
            int tileWidth = heightMap.GetLength(1);
            terrain.size = new Vector3(tileDepth * 2, 200, tileWidth * 2);
            float[,] h = terrain.GetHeights(0, 0, size, size);
            Vector3 mapSize = terrain.size;
            for (int zIndex = 0; zIndex < size; zIndex++)
                for (int xIndex = 0; xIndex < size; xIndex++)
                {
                    World.MapData tile = heightMap[(int)(zIndex * (float)tileDepth / size), (int)(xIndex * (float)tileWidth / size)];
                    h[zIndex, xIndex] = (float)tile.height * heightMultiplier;
                    //if (tile.plant != null)
                    //{
                    //    Vector3 pos = new Vector3(xIndex * mapSize.x / size + 2f * xIndex / size, h[zIndex, xIndex] * mapSize.y, zIndex * mapSize.z / size + 2f * zIndex / size);
                    //    switch (tile.plant.type)
                    //    {
                    //        case EvolutionSimulation.Entities.Plant.PlantType.Grass:
                    //            Instantiate(grass, pos, Quaternion.identity, transform);
                    //            break;
                    //        case EvolutionSimulation.Entities.Plant.PlantType.Bush:
                    //            Instantiate(bush, pos, Quaternion.identity, transform);
                    //            break;
                    //        case EvolutionSimulation.Entities.Plant.PlantType.Tree:
                    //            Instantiate(tree, pos, Quaternion.identity, transform);
                    //            break;
                    //        case EvolutionSimulation.Entities.Plant.PlantType.EdibleTree:
                    //            Instantiate(edibleTree, pos, Quaternion.identity, transform);
                    //            break;
                    //        default:
                    //            break;
                    //    }
                    //}
                }
            terrain.SetHeights(0, 0, h);
        }
    }
}