using System.Collections.Generic;
using EvolutionSimulation;
using EvolutionSimulation.Entities;
using UnityEngine;

namespace UnitySimulation
{
    /// <summary>
    /// Manage the objects asociated with the simulation creatures
    /// </summary>
    public class WorldCreaturesManager : MonoBehaviour
    {
        [Tooltip("Object that represents the creatures")]
        public GameObject creaturePrefab;

        [Tooltip("World terrain component")]
        public Terrain terrain;

        [Tooltip(" Additional distance from the ground where the creatures will fly")]
        public float AirHeight;

        [Tooltip(" Additional distance from the ground where the creature move through trees")]
        public float TreeHeight;

        /// <summary>
        /// Dictionary to manage the scene creatures and the simulation creatures.
        /// Keys are the simulation creatures IDs, and the values are the Game Object associated with the
        /// real creature ID.
        /// </summary>
        Dictionary<int, GameObject> _creatures = new Dictionary<int, GameObject>();
        private void Start()
        {
            if (terrain == null)
                Debug.LogError("Terrain is not assigned");
        }

        /// <summary>
        /// Restart the manager to its initial state
        /// </summary>
        public void Restart()
        {
            // Destroy every creature that was on the scene
            foreach (GameObject creature in _creatures.Values)
            {
                Destroy(creature);
            }
            _creatures.Clear();
        }

        /// <summary>
        /// Updates the creatures with the updated simulation world information
        /// </summary>
        public void StepUpdate(World info)
        {
            // Get the simulation world current creatures
            List<int> currCreatures = new List<int>(info.Creatures.Keys);
            /*for (int i = 0; i < l.Count; ++i)
            {
                Creature c = info.GetCreature(l[i]);
                if (info.map[c.x, c.y].isWater)
                    Debug.Log("In water. CriatureID " + c.ID + ". pos: (" + c.x + " " + c.y + ")");
            }*/
            CheckCreatures(info, currCreatures);
        }

        /// <summary>
        /// Checks the creatures of the scene regarding the updated information of the world. 
        /// It deletes creatures that have died and creautres that have been born in 
        /// the last tick of the simulation.
        /// </summary>
        void CheckCreatures(World w, List<int> creatures)
        {
            //Check if a creature has died
            List<int> keys = new List<int>(_creatures.Keys);

            for (int i = 0; i < _creatures.Count; ++i)
            {
                int c = keys[i];
                if (w.GetCreature(c) == null)
                {
                    Destroy(_creatures[c]);
                    _creatures.Remove(c);
                }
            }

            // Check if a new creature has been born
            // and update every position with the new info
            foreach (int c in creatures)
            {
                Creature creature = w.GetCreature(c);
                if (!_creatures.ContainsKey(c))
                {
                    // Instantiate creature
                    _creatures.Add(c, SpawnCreature(creature));
                }
                // Update creature GO position
                UpdateCreature(creature, _creatures[c]);
            }
        }
        /// <summary>
        /// Spawns a new creature game object corresponds to a simulation creature
        /// </summary>
        GameObject SpawnCreature(Creature c)
        {
            RaycastHit hit;
            // Get the terrain point that correspond to the creature.  
            Physics.Raycast(new Vector3(c.x * terrain.terrainData.size.x / c.world.map.GetLength(0), terrain.terrainData.size.y + 10, (c.world.map.GetLength(1) - c.y) * terrain.terrainData.size.z / c.world.map.GetLength(1)), Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("World"));
            GameObject gO = Instantiate(creaturePrefab, new Vector3(hit.point.x, hit.point.y, hit.point.z), Quaternion.identity);
            CreatureManager cMG = gO.GetComponent<CreatureManager>();
            cMG.InitalizeCreature(c);
            gO.name = c.speciesName + " " + c.ID;
            // Subscribes to the event launched when a creature receives an interaction
            c.AddInteraction(Interactions.attack, (interacter) => { _creatures[c.ID].GetComponent<CreatureEffects>().Bite(); });
            c.AddInteraction(Interactions.attack,
                            (interacter) =>
                             {
                                 float percentage = c.stats.CurrHealth / c.stats.MaxHealth ;
                                 cMG.SetStatusBar(percentage);
                             }
                         );
            return gO;
        }

        /// <summary>
        /// Updates the creature game object with the simulation creature information
        /// </summary>
        void UpdateCreature(Creature c, GameObject gO)
        {
            // Position
            Vector3 nextPos = new Vector3(c.x * terrain.terrainData.size.x / c.world.map.GetLength(0), 0, (c.world.map.GetLength(1) - c.y) * terrain.terrainData.size.z / c.world.map.GetLength(1));
            CreatureManager cM = gO.GetComponent<CreatureManager>();
            //Debug.Log("NextPos: " + nextPos);
            //Debug.Log("Layer " + c.creatureLayer);
            //Debug.Log(c.GetStateInfo());
            switch (c.creatureLayer)
            {
                case Creature.HeightLayer.Air:
                    nextPos.y = Mathf.Max(terrain.SampleHeight(nextPos), terrain.terrainData.size.y / 2 - 0.05f) + AirHeight;
                    cM.ActivateLegAnimation(false);
                    cM.ActivateWingsAnimation(true);
                    break;
                case Creature.HeightLayer.Tree:
                    nextPos.y = terrain.SampleHeight(nextPos) + TreeHeight;
                    break;
                case Creature.HeightLayer.Ground:
                    nextPos.y = terrain.SampleHeight(nextPos);
                    cM.ActivateLegAnimation(true);
                    cM.ActivateWingsAnimation(false);
                    break;
            }
            gO.GetComponent<CreatureLerpPosition>().LerpToPosition(nextPos);

            // State visualization
            cM.SetStatusTexts(c.GetState(), c.GetStateInfo());

            if (c.GetState() == "EatingState")
            {
                gO.GetComponent<CreatureEffects>().Eat();
            }
            else if (c.GetState() == "DrinkingState")
            {
                gO.GetComponent<CreatureEffects>().Drink();
            }
        }

    }
}