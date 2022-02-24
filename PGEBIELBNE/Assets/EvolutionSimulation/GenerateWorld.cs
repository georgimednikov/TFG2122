using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EvolutionSimulation.Unity
{
    public class GenerateWorld : MonoBehaviour
    {
        public TextAsset world;

        MapData[,] map;

        void Start()
        {
            map = Newtonsoft.Json.JsonConvert.DeserializeObject<MapData[,]>(world.text);
            Terrain terrain = GetComponent<Terrain>();
            terrain.terrainData.size = new Vector3(map.GetLength(0), terrain.terrainData.size.y, map.GetLength(1));
        }
    }
}