using UnityEngine;

namespace UnitySimulation
{
    [System.Serializable]
    public struct CreatureSpawn
    {
        public TextAsset species;
        public int amount;

        //Position in X for the center of the spawning radius in percentage of total terrain size
        public float positionX;
        //Position in Z for the center of the spawning radius in percentage of total terrain size
        public float positionZ;
        //Radius for the center of the spawning radius in percentage of total terrain size
        public float positionRadius;
    }
}
