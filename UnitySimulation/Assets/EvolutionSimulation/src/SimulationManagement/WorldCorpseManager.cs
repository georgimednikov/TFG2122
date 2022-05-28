using System.Collections.Generic;
using EvolutionSimulation;
using EvolutionSimulation.Entities;
using UnityEngine;

namespace UnitySimulation
{
    public class WorldCorpseManager : MonoBehaviour
    {
        [Tooltip("Object that represents the corpses")]
        public GameObject corpsePrefab;

        /// <summary>
        /// Dictionary to manage the scene corpses and the simulation corpses.
        /// Keys are the simulation corpses IDs, and the values are the Game Object associated with the
        /// real corpse ID.
        /// </summary>
        Dictionary<int, GameObject> _corpses = new Dictionary<int, GameObject>();

        /// <summary>
        /// Restart the manager to its initial state
        /// </summary>
        public void Restart()
        {
            // Destroy every corpse that was on the scene
            foreach (GameObject corpse in _corpses.Values)
            {
                Destroy(corpse);
            }
            _corpses.Clear();
        }

        /// <summary>
        /// Updates the corpses with the updated simulation world information
        /// </summary>
        public void StepUpdate(World info)
        {
            // Get the simulation world current corpses
            List<int> currCorpses = new List<int>();
            foreach (KeyValuePair<int, StaticEntity> se in info.StaticEntities)
            {
                if (se.Value is Plant) continue;
                currCorpses.Add(se.Key);
            }

            CheckCorpses(info, currCorpses);
        }

        /// <summary>
        /// Checks the creatures of the scene regarding the updated information of the world. 
        /// It deletes creatures that have died and creautres that have been born in 
        /// the last tick of the simulation.
        /// </summary>
        void CheckCorpses(World w, List<int> corpses)
        {
            // Check if a corpse has despawned
            List<int> keys = new List<int>(_corpses.Keys);

            for (int i = 0; i < _corpses.Count; ++i)
            {
                int c = keys[i];
                if (w.GetStaticEntity(c) == null)
                {
                    Destroy(_corpses[c]);
                    _corpses.Remove(c);
                }
            }

            // Check if a new corpse has spawned
            foreach (int c in corpses)
            {
                if (!_corpses.ContainsKey(c))
                {
                    // Instantiate corpse
                    _corpses.Add(c, SpawnCorpse(w.GetStaticEntity(c) as Corpse));
                }
            }
        }

        /// <summary>
        /// Spawns a new corpse game object corresponds to a simulation corpse
        /// </summary>
        GameObject SpawnCorpse(Corpse c)
        {
            Terrain terrain = GetComponent<Terrain>();

            RaycastHit hit;
            // Get the terrain point that correspond to the creature.  
            Physics.Raycast(new Vector3(c.x * terrain.terrainData.size.x / c.world.map.GetLength(0), terrain.terrainData.size.y + 10, (c.world.map.GetLength(1) - c.y) * terrain.terrainData.size.z / c.world.map.GetLength(1)), Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("World"));
            GameObject gO = Instantiate(corpsePrefab, new Vector3(hit.point.x, hit.point.y, hit.point.z), Quaternion.identity);
            return gO;
        }
    }
}