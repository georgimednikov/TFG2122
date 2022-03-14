using UnityEngine;
using EvolutionSimulation;
using System.Collections.Generic;
using UnityEditor;

namespace UnitySimulation
{
#if (UNITY_EDITOR)
    [CustomEditor(typeof(GenerateWorld))]
    public class TerrainRegenerator : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GenerateWorld myScript = (GenerateWorld)target;

            if (GUILayout.Button("Update"))
            {
                myScript.MapGen();
            }
        }
    }
#endif

    [RequireComponent(typeof(Terrain))]
    public class GenerateWorld : MonoBehaviour
    {
        public TextAsset world;
        World.MapData[,] map;
        public GameObject waterPlane;
        GameObject waterPlaneInstance;

        void Start()
        {
            if (waterPlaneInstance != null)
                MapGen();
        }

        public void MapGen()
        {
            map = Newtonsoft.Json.JsonConvert.DeserializeObject<World.MapData[,]>(world.text);
            UpdateMeshVertices(map);
            GenerateFlora(map);
            SetWaterPlane();
        }

        private void SetWaterPlane()
        {
            if(waterPlaneInstance != null)
                DestroyImmediate(waterPlaneInstance);
            
            TerrainData terrain = GetComponent<Terrain>().terrainData;
            waterPlaneInstance = Instantiate(waterPlane, new Vector3(terrain.size.x / 2, terrain.size.y / 2 - 0.05f, terrain.size.z / 2), Quaternion.identity, transform);
            waterPlaneInstance.transform.localScale = new Vector3(terrain.size.x / 10, 1, terrain.size.z / 10);
        }

        private void GenerateFlora(World.MapData[,] heightMap)
        {
            List<TreeInstance> trees = new List<TreeInstance>();
            TerrainData terrain = GetComponent<Terrain>().terrainData;
            terrain.SetTreeInstances(trees.ToArray(), true);
            int tileDepth = heightMap.GetLength(0);
            int tileWidth = heightMap.GetLength(1);
            int size = terrain.heightmapResolution;
            int grassDensity = terrain.detailResolution;
            int[,] newMap = new int[grassDensity, grassDensity];

            for (int z = 0; z < tileWidth; z++)
                for (int x = 0; x < tileDepth; x++)
                {
                    for (int i = 0; i < grassDensity / tileWidth; i++)
                        for (int j = 0; j < grassDensity / tileDepth; j++)
                            if (Random.Range(0f, 1f) < heightMap[x, z].flora)
                                newMap[grassDensity - 1 - ((grassDensity / tileWidth) * z + i), ((grassDensity / tileDepth * x) + j)] = (int)(Mathf.Round(1f + (float)heightMap[x, z].flora * 2));
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
                    tree.heightScale = 1;// Random.Range(0.75f, 2.25f);
                    tree.widthScale = 1;//Random.Range(0.5f, 1f);
                    tree.position = new Vector3((z + 0.5f + Random.Range(-0.5f, 0.5f)) / (float)tileWidth, 0, 1 - (x + 0.5f + Random.Range(-0.5f, 0.5f)) / (float)tileDepth);
                    Debug.Log("Pongo una puta planta en: " + tree.position);
                    trees.Add(tree);
                }

            terrain.SetTreeInstances(trees.ToArray(), true);
            terrain.SetDetailLayer(0, 0, 0, newMap);
        }

        private void UpdateMeshVertices(World.MapData[,] heightMap)
        {
            TerrainData terrain = GetComponent<Terrain>().terrainData;
            int size = terrain.heightmapResolution;
            int tileDepth = heightMap.GetLength(0);
            int tileWidth = heightMap.GetLength(1);
            terrain.size = new Vector3(tileDepth * 2, terrain.size.y, tileWidth * 2);
            float[,] h = terrain.GetHeights(0, 0, size, size);
            Vector3 mapSize = terrain.size;
            for (int zIndex = 0; zIndex < size; zIndex++)
                for (int xIndex = 0; xIndex < size; xIndex++)
                {
                    World.MapData tile = heightMap[(int)(zIndex * (float)tileDepth / size), (int)(xIndex * (float)tileWidth / size)];
                    h[size - 1 - xIndex, zIndex] = (float)tile.height;
                }
            terrain.SetHeights(0, 0, h);
        }
    }
}