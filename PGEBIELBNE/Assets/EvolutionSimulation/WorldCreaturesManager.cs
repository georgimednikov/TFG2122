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
                    // Instancio creatura
                }
                // Update creature GO position
            }
        }

        Dictionary<Creature, GameObject> _creatures;
    }
}