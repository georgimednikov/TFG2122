using System;
using UnityEngine;

namespace UnitySimulation
{
    public class GenerateCreatures : MonoBehaviour
    {
        public GameObject creature;
        public int MapSize;
        public CreatureSpawn[] creatureDistribution;
       
        void Start()
        {
            System.Random rnd = new System.Random();

            foreach (CreatureSpawn spawn in creatureDistribution)
            {
                if (spawn.positionRadius < 0 || spawn.positionRadius > 1)
                    throw new Exception("The radius for the spawn of creatures cannot surpass the size of the world in neither the axis X or Z");

                if (spawn.positionX < 0 || spawn.positionX > MapSize ||
                    spawn.positionZ < 0 || spawn.positionZ > MapSize)
                    throw new Exception("The position given to spawn any species must be within the logical confines of the world given the radius of spawn");

                // From percentage of terrain size to tangible values
                float worldRadius = spawn.positionRadius * MapSize;

                for (int i = 0; i < spawn.amount; ++i)
                {
                    // Calculates a random point within a circle to get the offset of the position.
                    float xPos = (float)(worldRadius * Math.Sqrt(rnd.NextDouble()) * Math.Cos(rnd.NextDouble() * 2 * Math.PI));
                    float zPos = (float)(worldRadius * Math.Sqrt(rnd.NextDouble()) * Math.Sin(rnd.NextDouble() * 2 * Math.PI));

                    Instantiate(creature, new Vector3(xPos, 0, zPos), Quaternion.identity).GetComponent<CreatureManager>().InitalizeCreature(spawn.species);
                }
            }
        }
    }
}
