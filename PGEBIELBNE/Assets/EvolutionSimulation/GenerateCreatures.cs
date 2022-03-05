using System;
using UnityEngine;

namespace UnitySimulation
{
    public class GenerateCreatures : MonoBehaviour
    {
        
        [Serializable]
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

        public GameObject creature;
        
        public CreatureSpawn[] creatureDistribution;

        void Start()
        {
            Terrain terrain = GetComponent<Terrain>();
            System.Random rnd = new System.Random();

            foreach (CreatureSpawn spawn in creatureDistribution)
            {
                if (spawn.positionRadius < 0 || spawn.positionRadius > 1)
                    throw new Exception("The radius for the spawn of creatures cannot surpass the size of the world in neither the axis X or Z");

                if (spawn.positionX < 0 || spawn.positionX > 1 ||
                    spawn.positionZ < 0 || spawn.positionZ > 1)
                    throw new Exception("The position given to spawn any species must be within the logical confines of the world given the radius of spawn");

                // From percentage of terrain size to tangible values
                float worldRadius = spawn.positionRadius * terrain.terrainData.size.x;

                for (int i = 0; i < spawn.amount; ++i)
                {
                    // Calculates a random point within a circle to get the offset of the position.
                    float xPos = (float)(worldRadius * Math.Sqrt(rnd.NextDouble()) * Math.Cos(rnd.NextDouble() * 2 * Math.PI));
                    float zPos = (float)(worldRadius * Math.Sqrt(rnd.NextDouble()) * Math.Sin(rnd.NextDouble() * 2 * Math.PI));

                    // Adds the center of the circle to offset the position to the final value.
                    // Like with the radius, positions fo from percentage of terrain size to tangible values
                    xPos += spawn.positionX * terrain.terrainData.size.x;
                    zPos += spawn.positionZ * terrain.terrainData.size.z;

                    RaycastHit hit;
                    // Position in Y axis + 10 (no specific reason for that number) => No point in the world will be higher.  
                    Physics.Raycast(new Vector3(xPos, terrain.terrainData.size.y + 10, zPos), Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("World"));
                    Instantiate(creature, new Vector3(hit.point.x, hit.point.y, hit.point.z), Quaternion.identity).GetComponent<CreatureManager>().InitalizeCreature(spawn.species);
                }
            }
        }
    }
}
