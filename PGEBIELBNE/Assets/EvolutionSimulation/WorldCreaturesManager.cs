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

        Dictionary<Creature, GameObject> _creatures = new Dictionary<Creature, GameObject>();

        public void OnNotify(World info)
        {
            CheckCreatures(new List<Creature>(info.Creatures.Values));
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
                _creatures[receiver].GetComponent<CreatureEffects>().Bite();
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