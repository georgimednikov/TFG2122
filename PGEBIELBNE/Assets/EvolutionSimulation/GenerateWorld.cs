using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EvolutionSimulation.Unity
{
    public class JsonHelper
    {
        public static T[,] getJsonArray<T>(string json)
        {
            string newJson = "{ \"array\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[,] array;
        }
    }

    public class GenerateWorld : MonoBehaviour
    {
        public TextAsset world;

        MapData[,] map;

        void Start()
        {
            //map = JsonUtility.FromJson<MapDataWrapper>(world.text).map;
            //map = JsonHelper.getJsonArray<MapData>(world.text);
            //Terrain terrain = GetComponent<Terrain>();
            //terrain.terrainData.size = new Vector3(map.GetLength(0), terrain.terrainData.size.y, map.GetLength(1));
        }
    }
}