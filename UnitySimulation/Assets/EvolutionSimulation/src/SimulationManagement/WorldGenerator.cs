using UnityEngine;
using EvolutionSimulation;
using System.Collections.Generic;
using UnityEditor;
using Newtonsoft.Json;

namespace UnitySimulation
{
#if (UNITY_EDITOR)
    [CustomEditor(typeof(WorldGenerator))]
    public class TerrainRegenerator : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            WorldGenerator myScript = (WorldGenerator)target;

            if (GUILayout.Button("Update"))
            {
                myScript.MapGenSkipEvolution();
            }
        }
    }
#endif

    /// <summary>
    /// Generates the terrain according with the simulation world information
    /// </summary>
    [RequireComponent(typeof(Terrain))]
    public class WorldGenerator : MonoBehaviour
    {
        [Tooltip("World json file showed when the 'Update' button is clicked")]
        public TextAsset worldJson;

        [Tooltip("Object to represent the world water")]
        public GameObject waterPlane;
        GameObject waterPlaneInstance;

        /// <summary>
        /// Simulation World, it the World file in the SimulationManager script,
        /// Not the one that is in this script.
        /// </summary>
        World world;

        /// <summary>
        /// Sets the world to be generated.
        /// </summary>
        public void SetWorld(World world)
        {
            this.world = world;
        }

        /// <summary>
        /// Generates the terrain with the current world setted
        /// in this component. Used to check the map without needing
        /// to simulate anything (using the 'Update' editor button) 
        /// </summary>
        public void MapGenSkipEvolution()
        {
            World.MapData[,] map = JsonConvert.DeserializeObject<World.MapData[,]>(worldJson.ToString());
            UpdateMeshVertices(map);
            GenerateFlora(map);
            SetWaterPlane();
        }

        /// <summary>
        /// Generates the terrain corresponding to the setted evolution map of the simulation
        /// </summary>
        public void MapGen()
        {
            UpdateMeshVertices(world.map);
            GenerateFlora(world.map);
            SetWaterPlane();
            Debug.Log("Map generation done");
        }

        /// <summary>
        /// Creates the water plane
        /// </summary>
        private void SetWaterPlane()
        {
            if(waterPlaneInstance != null)
                DestroyImmediate(waterPlaneInstance);
            
            TerrainData terrain = GetComponent<Terrain>().terrainData;
            waterPlaneInstance = Instantiate(waterPlane, new Vector3(terrain.size.x / 2, terrain.size.y / 2 - 0.05f, terrain.size.z / 2), Quaternion.identity, transform);
            waterPlaneInstance.transform.localScale = new Vector3(terrain.size.x / 10, 1, terrain.size.z / 10);
        }

        /// <summary>
        /// Creates the flora on the terrain that corresponds with the simulation world flora
        /// </summary>
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
                    trees.Add(tree);
                }

            terrain.SetTreeInstances(trees.ToArray(), true);
            terrain.SetDetailLayer(0, 0, 0, newMap);
        }

        /// <summary>
        /// Updates the terrain mesh vertices to adapt to simulation world heights
        /// </summary>
        private void UpdateMeshVertices(World.MapData[,] heightMap)
        {
            TerrainData terrain = GetComponent<Terrain>().terrainData;
            int size = terrain.heightmapResolution;
            int tileDepth = heightMap.GetLength(0);
            int tileWidth = heightMap.GetLength(1);
            //terrain.size = new Vector3(tileDepth * 2, terrain.size.y, tileWidth * 2);
            float[,] h = terrain.GetHeights(0, 0, size, size);
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