using UnityEngine;
using EvolutionSimulation;

namespace UnitySimulation
{
    public class GenerateWorld : MonoBehaviour
    {
        public TextAsset world;

        World.MapData[,] map;

        void Start()
        {
            map = Newtonsoft.Json.JsonConvert.DeserializeObject<World.MapData[,]>(world.text);
            Terrain terrain = GetComponent<Terrain>();
            terrain.terrainData.size = new Vector3(map.GetLength(0), terrain.terrainData.size.y, map.GetLength(1));
        }
    }
}