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

        [Tooltip(" Additional distance from the ground where the creatures will fly")]
        public float AirHeight;
        [Tooltip(" Additional distance from the ground where the creature move through trees")]
        public float TreeHeight;

        Dictionary<int, GameObject> _creatures = new Dictionary<int, GameObject>();
        Terrain terrain;

        private void Start()
        {
            terrain = GetComponent<Terrain>();
            //AirHeight += terrain.terrainData.size.y;
        }

        public void OnNotify(World info)
        {
            List<int> l = new List<int>(info.Creatures.Keys);
            for (int i = 0; i < l.Count; ++i)
            {
                Creature c = info.GetCreature(l[i]);
                if (info.map[c.x, c.y].isWater)
                    Debug.Log("In water. CriatureID " + c.ID + ". pos: (" + c.x + " " + c.y + ")");
            }
            CheckCreatures(info, l);
        }

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
                    // Instantiate creatura
                    _creatures.Add(c, SpawnCreature(creature));
                }
                // Update creature GO position
                UpdateCreature(creature, _creatures[c]);
            }
        }


        GameObject SpawnCreature(Creature c)
        {
            RaycastHit hit;
            // Position in Y axis + 10 (no specific reason for that number) => No point in the world will be higher.  
            Physics.Raycast(new Vector3(c.x * terrain.terrainData.size.x / c.world.map.GetLength(0), terrain.terrainData.size.y + 10, (c.world.map.GetLength(1) - c.y) * terrain.terrainData.size.z / c.world.map.GetLength(1)), Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("World"));
            GameObject gO = Instantiate(creaturePrefab, new Vector3(hit.point.x, hit.point.y, hit.point.z), Quaternion.identity);
            CreatureManager cMG = gO.GetComponent<CreatureManager>();
            cMG.InitalizeCreature(c);
            gO.name = c.speciesName + " " + c.ID;
            // Subscribes to the event launched when a creature receives an interaction
            c.ReceiveInteractionEvent += ReceiveInteractionListener;
            c.AddInteraction(Interactions.attack,
                             (c) =>
                             {
                                 float percentage = c.stats.CurrHealth / c.stats.MaxHealth ;
                                 cMG.SetStatusBar(percentage);
                             }
                         );
            return gO;
        }

        /// <param name="receiver">Which creature received an interaction</param>
        /// <param name="sender">Which creature sent an interaction</param>
        /// <param name="type">Which interaction was sent</param>
        void ReceiveInteractionListener(Creature receiver, Creature sender, Interactions type)
        {
            if (type == Interactions.attack)
            {
                _creatures[receiver.ID].GetComponent<CreatureEffects>().Bite();
            }
        }

        void UpdateCreature(Creature c, GameObject gO)
        {
            // Position
            //Vector3 nextPos = new Vector3(c.x, 0, c.y);
            Vector3 nextPos = new Vector3(c.x * terrain.terrainData.size.x / c.world.map.GetLength(0), 0, (c.world.map.GetLength(1) - c.y) * terrain.terrainData.size.z / c.world.map.GetLength(1));
            CreatureManager cM = gO.GetComponent<CreatureManager>();
            //Debug.Log("NextPos: " + nextPos);
            //Debug.Log("Layer " + c.creatureLayer);
            //Debug.Log(c.GetStateInfo());
            switch (c.creatureLayer)
            {
                case Creature.HeightLayer.Air:
                    nextPos.y = terrain.SampleHeight(nextPos) + AirHeight;
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
            cM.SetStatusTexts(""/*c.GetState()*/, ""/*c.GetStateInfo()*/);            
        }

    }
}