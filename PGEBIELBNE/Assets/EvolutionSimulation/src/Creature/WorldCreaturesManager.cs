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
        public GameObject creaturePrefab;

        Dictionary<int, GameObject> _creatures = new Dictionary<int, GameObject>();

        public void OnNotify(World info)
        {
            CheckCreatures(info, new List<int>(info.Creatures.Keys));
        }

        void CheckCreatures(World w, List<int> creatures)
        {
            // Check if a creature has died
            foreach (int c in _creatures.Keys)
            {
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
                    // Instantiate creatura
                    _creatures.Add(c, SpawnCreature(creature));
                }
                // Update creature GO position
                UpdateCreature(creature, _creatures[c]);
            }
        }


        GameObject SpawnCreature(Creature c)
        {
            Terrain terrain = GetComponent<Terrain>();

            RaycastHit hit;
            // Position in Y axis + 10 (no specific reason for that number) => No point in the world will be higher.  
            Physics.Raycast(new Vector3(c.x, terrain.terrainData.size.y + 10, c.y), Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("World"));
            GameObject gO = Instantiate(creaturePrefab, new Vector3(hit.point.x, hit.point.y, hit.point.z), Quaternion.identity);
            gO.GetComponent<CreatureManager>().InitalizeCreature(c);
            // Subscribes to the event launched when a creature receives an interaction
            c.ReceiveInteractionEvent += ReceiveInteractionListener;
            return gO;
        }

        /// <param name="receiver">Which creature received an interaction</param>
        /// <param name="sender">Which creature sent an interaction</param>
        /// <param name="type">Which interaction was sent</param>
        void ReceiveInteractionListener(Creature receiver, Creature sender, Interactions type)
        {
            if(type == Interactions.attack)
            {
                _creatures[receiver.ID].GetComponent<CreatureEffects>().Bite();
            }
        }

        void UpdateCreature(Creature c, GameObject gO)
        {
            // Position
            gO.GetComponent<CreatureLerpPosition>().LerpToPosition(new Vector3(c.x, gO.transform.position.y, c.y));
            
            // State visualization
            string state = c.GetState();
            gO.GetComponent<CreatureManager>().statusBar.GetComponent<StatusBar>().SetStatus(state);
            gO.GetComponent<CreatureManager>().statusBar.GetComponent<StatusBar>().SetStatusInfo(c.GetStateInfo());
        }

    }
}