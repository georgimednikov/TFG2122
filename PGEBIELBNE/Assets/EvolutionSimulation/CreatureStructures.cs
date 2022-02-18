using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EvolutionSimulation.Unity
{
    [System.Serializable]
    // These attributes can't have a get; or set; method, as JsonUtility doesn't know how to interpret them
    public class CreatureStats
    {
        private float startMultiplier = 0.33f; //Starting multiplier of newborns
        private float adulthoodThreshold = 0.25f; //After which percentage of lifespan the creature has his stats not dimished by age

        public float tiredThreshold = 0.40f; //After which percentage of currRest the creature should sleep with low priority
                                             //After which percentage of currRest the creature should sleep with high priority and some stats are dimished
        public float exhaustThreshold = 0.20f;

        /// <summary>
        /// Modifies the given stat based on age
        /// </summary>
        public float ModifyStatByAge(float stat)
        {
            return stat * Math.Min(1.0f, (1 - startMultiplier) / (LifeSpan * adulthoodThreshold) * currAge + startMultiplier);
        }

        public bool IsNewBorn() { return LifeSpan * adulthoodThreshold < currAge; }

        public Gender Gender;

        //Nutrition related stats
        public Diet Diet;
        public float Scavenger; //From 0 (normal chance of getting poisoned) to 1 (cannot get poisoned)

        //Health and damage related stats
        float maxHealth;
        public float MaxHealth;
        public float CurrHealth;
        int damage;
        public int Damage;
        int armor;
        public int Armor;
        int perforation;
        public int Perforation;
        float venom;
        public float Venom;
        float counter; // Puas
        public float Counter;

        //Mobility related stats
        public int AerialSpeed;
        public int ArborealSpeed;
        public int GroundSpeed;

        //Reaches
        public bool AirReach; // TODO: que afecte la edad?
        public bool TreeReach;

        //Energy related stats
        float maxEnergy;
        public float MaxEnergy;
        public float CurrEnergy;
        public float EnergyExpense;

        //Hydration related stats
        public float MaxHydration;
        public float CurrHydration;
        public float HydrationExpense;

        //Rest related stats
        public float MaxRest;
        float currRest;
        public float CurrRest;
        public float RestRecovery;
        public float RestExpense;

        //Environment related stats
        public int Camouflage;
        int aggressiveness;
        public int Aggressiveness;
        int intimidation;
        public int Intimidation;
        public int Perception;
        public float NightDebuff;

        //Physique related stats
        int size;
        public int Size;
        public int LifeSpan;
        int currAge;
        public int CurrAge;
        public int Members;//limbs
        public int Metabolism;
        public float MinTemperature;
        public float MaxTemperature;
        public float IdealTemperature;

        //Behaviour related stats
        public int Knowledge;
        public int Paternity;

        //Multipliers
        public float HealthRegeneration;
        public float MaxSpeed;

        //Reproduction stats
        public int TimeBetweenHeats;
        public bool InHeat;
    }

    [System.Serializable]
    public enum Gender
    {
        Male,
        Female
    }

    [System.Serializable]
    public enum Diet
    {
        Herbivore,
        Omnivore,
        Carnivore,

        //Total diets
        Count
    }

    [System.Serializable]
    public struct SpeciesExport
    {
        public string name;
        public CreatureStats stats;

        public SpeciesExport(string name, CreatureStats stats)
        {
            this.name = name;
            this.stats = stats;
        }
    }
}