using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation
{
    class Corpse : IEntity, IInteractable<Creature>
    {
        public Corpse()
        {
            rnd = new Random();
        }
        

        /// <summary>
        /// Initializes the corpse.
        /// </summary>
        /// <param name="timeLimit"> The number of ticks that the corpses stays </param>
        /// <param name="poisonProbability"> 
        /// The probability of being poisone when interacting with the copse. 
        /// A value between 0 and 100 (%), higher or lower values will be clamped.
        /// </param>
        public void Init(int timeLimit, int poisonProbability)
        {
            lifeTime = timeLimit;
            poisonProb = poisonProbability;
        }

        public void OnInteract(Creature other)
        {
            // TODO: Considerar la habilidad carroniero
            if(rnd.Next(0,100) < poisonProb)
                Console.WriteLine("The creature is poisoned");  //TODO: Hacer que se envenene la creatura de verdad
        }

        public void Tick()
        {
            lifeTime--;
            if (lifeTime == 0)
                Console.WriteLine("Desaparece");
        }

        Random rnd;
        float lifeTime;
        float poisonProb;
    }
}
