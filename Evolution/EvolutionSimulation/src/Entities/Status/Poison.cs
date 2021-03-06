using EvolutionSimulation.Genetics;
using System;

using Telemetry;
using Telemetry.Events;

namespace EvolutionSimulation.Entities.Status
{
    /// <summary>
    /// This status deal damage per second to the owner
    /// </summary>
    public class Poison : Status
    {
        // Damage the poison will do per tick
        float damage;

        // ID of the creature that gave the poison
        int giverID;    

        public Poison(int duration, float dmg, int id) : base(duration)
        {
            damage = dmg;
            giverID = id;
        } 

        public override bool OnTick()
        {
            owner.stats.CurrHealth -= damage;
#if DEBUG
            Console.WriteLine("POSION DEALT " + damage.ToString() + " DMG TO " + owner.speciesName + " " + owner.ID + " (" + owner.stats.CurrHealth + " HP LEFT)");
#endif
#if TRACKER_ENABLED
            Tracker.Instance.Track(new CreatureReceiveDamage(owner.world.CurrentTick, owner.ID, owner.speciesName, giverID, damage, DamageType.Poison, owner.stats.CurrHealth, owner.x, owner.y));
#endif
            if (owner.causeOfDeath == CauseOfDeath.NONE && owner.stats.CurrHealth <= 0)
            {
                owner.causeOfDeath = CauseOfDeath.Poison;
                owner.killingBlow = damage;
                owner.killerID = owner.ID;
            }

            return base.OnTick();
        }

#if DEBUG
        public override void OnApply()
        {
            Console.WriteLine(owner.speciesName + " " + owner.ID + " POSIONED FOR " + damage.ToString() + " DMG/TICK");
        }

        public override void OnRemove()
        {
            Console.WriteLine("POISON REMOVED FROM " + owner.speciesName + " " + owner.ID);
        }

        public override void OnExpire()
        {
            Console.WriteLine("POISON EXPIRED FROM " + owner.speciesName + " " + owner.ID);
        }
#endif
    }
}
