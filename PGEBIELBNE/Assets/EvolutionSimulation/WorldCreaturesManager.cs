using System;
using System.Collections.Generic;
using EvolutionSimulation;
using EvolutionSimulation.Entities;
using EvolutionSimulation.Utils;
using UnityEngine;

namespace UnitySimulation
{
    public class WorldCreaturesManager : MonoBehaviour, IListener<World>
    {
        [Serializable]
        public struct SpawnRadius
        {
            public float positionX;
            public float positionZ;
            public float positionRadius;
        } 

        public GameObject creaturePrefab;
        public SpawnRadius spawn;

        Dictionary<Creature, GameObject> _creatures = new Dictionary<Creature, GameObject>();

        public void OnNotify(World info)
        {
            CheckCreatures(info.Creatures);
        }

        // TODO: esto es tope de ineficiente mejor luego hacer eventos
        // que notifiquen cuando se ha creado una creatura y cuando se ha destruido
        void CheckCreatures(List<Creature> creatures)
        {
            // Check if a creature has died
            List<Creature> managedCreatures = new List<Creature>(_creatures.Keys);
            foreach (Creature c in managedCreatures)
            {
                if (!creatures.Contains(c))
                {
                    Destroy(_creatures[c]);
                    _creatures.Remove(c);
                }
            }


            // Check if a new creature has been born
            // and update every position with the new info
            foreach (Creature c in creatures)
            {
                if (!_creatures.ContainsKey(c))
                {
                    // Instantiate creatura
                    _creatures.Add(c, SpawnCreature(c));
                }
                // Update creature GO position
                UpdateCreature(c, _creatures[c]);
            }
        }


        GameObject SpawnCreature(Creature c)
        {
            Terrain terrain = GetComponent<Terrain>();
            System.Random rnd = new System.Random();
            // TODO: ahora se escoge un spawn random de los que haya y solo se instancia uno

            if (spawn.positionRadius < 0 || spawn.positionRadius > 1)
                throw new Exception("The radius for the spawn of creatures cannot surpass the size of the world in neither the axis X or Z");

            if (spawn.positionX < 0 || spawn.positionX > 1 ||
                spawn.positionZ < 0 || spawn.positionZ > 1)
                throw new Exception("The position given to spawn any species must be within the logical confines of the world given the radius of spawn");

            // From percentage of terrain size to tangible values
            float worldRadius = spawn.positionRadius * terrain.terrainData.size.x;

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
            GameObject gO = Instantiate(creaturePrefab, new Vector3(hit.point.x, hit.point.y, hit.point.z), Quaternion.identity);
            gO.GetComponent<CreatureManager>().InitalizeCreature(c);
            return gO;
        }

        void UpdateCreature(Creature c, GameObject gO)
        {
            gO.transform.position = new Vector3(c.x, gO.transform.position.y, c.y);
        }

    }
}