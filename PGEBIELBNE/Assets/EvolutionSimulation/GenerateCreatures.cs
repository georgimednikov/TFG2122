using System;
using UnityEngine;

namespace EvolutionSimulation.Unity
{
    public class GenerateCreatures : MonoBehaviour
    {
        [Serializable]
        public struct CreatureSpawn
        {
            public TextAsset species;
            public int amount;
            public float positionX;
            public float positionZ;
            public float positionRadius;
        }

        public GameObject creature;
        
        public CreatureSpawn[] creatureDistribution;

        // Start is called before the first frame update
        void Start()
        {
            Terrain terrain = GetComponent<Terrain>();
            System.Random rnd = new System.Random();

            foreach (CreatureSpawn spawn in creatureDistribution)
            {
                for (int i = 0; i < spawn.amount; ++i)
                {
                    if (spawn.positionRadius > terrain.terrainData.size.x / 2 || spawn.positionRadius > terrain.terrainData.size.z / 2)
                        throw new Exception("The radius for the spawn of creatures cannot surpass the size of the world in neither the axis X or Z");

                    if (spawn.positionX < transform.position.x + spawn.positionRadius || spawn.positionX > transform.position.x + terrain.terrainData.size.x - spawn.positionRadius ||
                        spawn.positionZ < transform.position.z + spawn.positionRadius || spawn.positionZ > transform.position.z + terrain.terrainData.size.z - spawn.positionRadius)
                        throw new Exception("The position given to spawn any species must be within the logical confines of the world given the radius of spawn");

                    // Calculates a random point within a circle to get the offset of the position.
                    float xPos = (float)((spawn.positionRadius * Math.Sqrt(rnd.NextDouble())) * Math.Cos(rnd.NextDouble() * 2 * Math.PI));
                    float zPos = (float)((spawn.positionRadius * Math.Sqrt(rnd.NextDouble())) * Math.Sin(rnd.NextDouble() * 2 * Math.PI));

                    // Adds the center of the circle to offset the position to the final value.
                    xPos += spawn.positionX;
                    zPos += spawn.positionZ;

                    RaycastHit hit;
                    // Position in Y axis + 10 (no specific reason for that number) => No point in the world will be higher.  
                    Physics.Raycast(new Vector3(xPos, terrain.terrainData.size.y + 10, zPos), Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("World"));
                    Instantiate(creature, new Vector3(hit.point.x, hit.point.y, hit.point.z), Quaternion.identity).GetComponent<InterpretJSON>().CreateCreature(spawn.species);
                }
            }
        }
    }
}
