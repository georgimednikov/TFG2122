using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EvolutionSimulation.Genetics;

namespace EvolutionSimulation
{
    /// <summary>
    /// Class used to validate.
    /// </summary>
    public static class Validator
    {
        #region WorldGenConfig
        public class NonZeroSizeMapException : Exception
        {
            public NonZeroSizeMapException() { }
            public NonZeroSizeMapException(string message) : base(message) { }
            public NonZeroSizeMapException(string message, Exception inner) : base(message, inner) { }
            protected NonZeroSizeMapException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
        public class NotSameSizeMapsException : Exception
        {
            public NotSameSizeMapsException() { }
            public NotSameSizeMapsException(string message) : base(message) { }
            public NotSameSizeMapsException(string message, Exception inner) : base(message, inner) { }
            protected NotSameSizeMapsException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
        public class FunctionNotWhollyDefinedException : Exception
        {
            public FunctionNotWhollyDefinedException() { }
            public FunctionNotWhollyDefinedException(string message) : base(message) { }
            public FunctionNotWhollyDefinedException(string message, Exception inner) : base(message, inner) { }
            protected FunctionNotWhollyDefinedException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
        public class FunctionIncorrectOutputRangeException : Exception
        {
            public FunctionIncorrectOutputRangeException() { }
            public FunctionIncorrectOutputRangeException(string message) : base(message) { }
            public FunctionIncorrectOutputRangeException(string message, Exception inner) : base(message, inner) { }
            protected FunctionIncorrectOutputRangeException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        static double funcValidationAccuracy = 0.1;
        static public void Validate(WorldGenConfig configuration)
        {
            NonZeroSize(configuration);
            SameSizeMaps(configuration);
            HeightFunc(configuration);
            InfluenceFunc(configuration);
            FloraFunc(configuration);
            TemperatureSoftenerFunc(configuration);
            FloraSelectorFunc(configuration);
            RegionMap(configuration);
        }

        static void NonZeroSize(WorldGenConfig config)
        {
            if (config.humidityMap != null || config.temperatureMap != null || config.heightMap != null) return;
            if (config.mapSize <= 0) throw new NonZeroSizeMapException("The provided size is equal or less than 0");

            return;
        }

        static void SameSizeMaps(WorldGenConfig config)
        {

            int defined = 0;
            bool defHeight = (config.heightMap != null), defHumidity = (config.humidityMap != null), defTemp = (config.temperatureMap != null);
            if (defHeight) defined++;
            if (defHumidity) defined++;
            if (defTemp) defined++;

            if (defined <= 1) return;

            int sizeX, sizeY;
            if (defHeight)
            {
                sizeX = config.heightMap.GetLength(0);
                sizeY = config.heightMap.GetLength(1);

                if (defHumidity && (sizeX != config.humidityMap.GetLength(0) || sizeY != config.humidityMap.GetLength(1))) { throw new NotSameSizeMapsException("The provided maps aren't of the same size"); }
                if (defTemp && (sizeX != config.temperatureMap.GetLength(0) || sizeY != config.temperatureMap.GetLength(1))) { throw new NotSameSizeMapsException("The provided maps aren't of the same size"); }
            }
            else if (defHumidity)
            {
                sizeX = config.humidityMap.GetLength(0);
                sizeY = config.humidityMap.GetLength(1);

                if (defTemp && (sizeX != config.temperatureMap.GetLength(0) || sizeY != config.temperatureMap.GetLength(1))) { throw new NotSameSizeMapsException("The provided maps aren't of the same size"); }
            }
            return;
        }

        static void HeightFunc(WorldGenConfig config)
        {
            if (config.evaluateHeight == null) return;
            double val;
            for (double i = 0; i <= 1; i += funcValidationAccuracy)
            {
                try
                {
                    val = config.evaluateHeight(i);
                }
                catch
                {
                    throw new FunctionNotWhollyDefinedException("The provided height function is not defined for the requested input range");
                }
                if (val < 0 || val > 1) throw new FunctionIncorrectOutputRangeException("The provided height function has an incorrect output range");
            }
            return;
        }

        static void InfluenceFunc(WorldGenConfig config)
        {
            if (config.evaluateInfluence == null) return;
            double val;
            for (double i = 0; i <= 1; i += funcValidationAccuracy)
            {
                try
                {
                    val = config.evaluateInfluence(i);
                }
                catch
                {
                    throw new FunctionNotWhollyDefinedException("The provided influence function is not defined for the requested input range");
                }
                if (val < -1 || val > 1) throw new FunctionIncorrectOutputRangeException("The provided influence function has an incorrect output range");
            }
            return;
        }

        static void FloraFunc(WorldGenConfig config)
        {
            if (config.evaluateFlora == null) return;
            double val;
            for (double i = 0; i <= 1; i += funcValidationAccuracy)
                for (double j = 0; j <= 1; j += funcValidationAccuracy)
                {
                    try
                    {
                        val = config.evaluateFlora(i, j);
                    }
                    catch
                    {
                        throw new FunctionNotWhollyDefinedException("The provided flora function is not defined for the requested input range");
                    }
                    if (val < 0 || val > 1) throw new FunctionIncorrectOutputRangeException("The provided flora function has an incorrect output range");
                }
            return;
        }

        static void TemperatureSoftenerFunc(WorldGenConfig config)
        {
            if (config.evaluateFlora == null) return;
            double val;
            for (double i = 0; i <= 1; i += funcValidationAccuracy)
                for (double j = 0; j <= 1; j += funcValidationAccuracy)
                {
                    try
                    {
                        val = config.evaluateFlora(i, j);
                    }
                    catch
                    {
                        throw new FunctionNotWhollyDefinedException("The provided flora function is not defined for the requested input range");
                    }
                    if (val < 0 || val > 1) throw new FunctionIncorrectOutputRangeException("The provided flora function has an incorrect output range");
                }
            return;
        }

        static void FloraSelectorFunc(WorldGenConfig config)
        {
            if (config.floraSelector == null) return;
            int val;
            for (double i = funcValidationAccuracy; i <= 1; i += funcValidationAccuracy)
            {
                try
                {
                    val = config.floraSelector(i);
                }
                catch
                {
                    throw new FunctionNotWhollyDefinedException("The provided height function is not defined for the requested input range");
                }
                if (val < 0 || val > 4) throw new FunctionIncorrectOutputRangeException("The provided flora selector function has an incorrect output range");
            }
            return;
        }

        static void RegionMap(WorldGenConfig config)
        {
            if (config.regionMap == null) return;

        }
        #endregion

        #region Chromosome
        public class NotAllGenesDefinedException : Exception
        {
            public NotAllGenesDefinedException() { }
            public NotAllGenesDefinedException(string message) : base(message) { }
            public NotAllGenesDefinedException(string message, Exception inner) : base(message, inner) { }
            protected NotAllGenesDefinedException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
        public class NotAllValuesArePositiveException : Exception
        {
            public NotAllValuesArePositiveException() { }
            public NotAllValuesArePositiveException(string message) : base(message) { }
            public NotAllValuesArePositiveException(string message, Exception inner) : base(message, inner) { }
            protected NotAllValuesArePositiveException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
        public class GeneHasRelationWithItself : Exception
        {
            public GeneHasRelationWithItself() { }
            public GeneHasRelationWithItself(string message) : base(message) { }
            public GeneHasRelationWithItself(string message, Exception inner) : base(message, inner) { }
            protected GeneHasRelationWithItself(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
        public class GeneDependsCompletelyOnOthers : Exception
        {
            public GeneDependsCompletelyOnOthers() { }
            public GeneDependsCompletelyOnOthers(string message) : base(message) { }
            public GeneDependsCompletelyOnOthers(string message, Exception inner) : base(message, inner) { }
            protected GeneDependsCompletelyOnOthers(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
        public class GeneDependsOnUnprocessed : Exception
        {
            public GeneDependsOnUnprocessed() { }
            public GeneDependsOnUnprocessed(string message) : base(message) { }
            public GeneDependsOnUnprocessed(string message, Exception inner) : base(message, inner) { }
            protected GeneDependsOnUnprocessed(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        static public void Validate(List<Gene> genes)
        {
            ChromosomeAllGenesWithValidValuesFunc(genes);
            ChromosomeGeneDependencyOrderFunc(genes);
            ChromosomeCircularOrWrongOrderDependencyFunc(genes);
        }

        static void ChromosomeAllGenesWithValidValuesFunc(List<Gene> genes)
        {
            if (genes.Count != (int)CreatureFeature.Count)
                throw new NotAllGenesDefinedException("The chromosome must have as many genes as features listed in 'CreatureFeature', " + (int)CreatureFeature.Count);

            bool[] featuresGiven = new bool[(int)CreatureFeature.Count]; //Bool default value = false
            foreach (Gene gene in genes)
            {
                featuresGiven[(int)gene.feature] = true;
                if (gene.maxValue < 0)
                    throw new NotAllValuesArePositiveException("All genes in the chromosome must have positive values");
            }

            foreach (bool feat in featuresGiven)
                if (!feat)
                    throw new NotAllFeaturesWeightsDefinedException("Each one of the " + (int)CreatureFeature.Count + " features declared in 'CreatureFeature' must be represented in the chromosome through a gene");

            return;
        }
        static void ChromosomeGeneDependencyOrderFunc(List<Gene> genes)
        {
            bool[] featuresGiven = new bool[(int)CreatureFeature.Count]; //Bool default value = false
            foreach (Gene gene in genes)
            {
                featuresGiven[(int)gene.feature] = true;
                foreach (Relation relation in gene.relations)
                {
                    if (!featuresGiven[(int)relation.feature])
                        throw new GeneDependsOnUnprocessed("The chromosome's genes must be given in such an order that a gene that depends on other genes" +
                            "must be processed after said genes");
                }
            }

            return;
        }
        static void ChromosomeCircularOrWrongOrderDependencyFunc(List<Gene> genes)
        {
            int[] geneMaxValues = new int[genes.Count];
            for (int i = 0; i < genes.Count; ++i)
            {
                geneMaxValues[(int)genes[i].feature] = genes[i].maxValue;
            }
            foreach (Gene gene in genes)
            {
                int relationsMaxValue = 0;
                foreach (Relation relation in gene.relations)
                {
                    if (relation.feature == gene.feature)
                        throw new GeneHasRelationWithItself("The chromosome's genes must not have any relation with themselves");

                    relationsMaxValue += (int)Math.Ceiling(relation.percentage * geneMaxValues[(int)relation.feature]);
                }
                int leftover = geneMaxValues[(int)gene.feature] - relationsMaxValue;

                if (leftover <= 0)
                    throw new GeneDependsCompletelyOnOthers("The chromosome's genes must not depend completely on other genes");
            }

            return;
        }
        #endregion

        #region SimilarityGeneWeight
        public class NotAllFeaturesWeightsDefinedException : Exception
        {
            public NotAllFeaturesWeightsDefinedException() { }
            public NotAllFeaturesWeightsDefinedException(string message) : base(message) { }
            public NotAllFeaturesWeightsDefinedException(string message, Exception inner) : base(message, inner) { }
            protected NotAllFeaturesWeightsDefinedException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
        public class SimilarityValuesDoNotAddUp : Exception
        {
            public SimilarityValuesDoNotAddUp() { }
            public SimilarityValuesDoNotAddUp(string message) : base(message) { }
            public SimilarityValuesDoNotAddUp(string message, Exception inner) : base(message, inner) { }
            protected SimilarityValuesDoNotAddUp(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        static float weightsTotalValueWiggleRoom = 0.02f;
        static public void Validate(Tuple<CreatureFeature, float>[] weights)
        {
            CheckAllFeatureWeightsFunc(weights);
        }

        static void CheckAllFeatureWeightsFunc(Tuple<CreatureFeature, float>[] weights)
        {
            if (weights.Length != (int)CreatureFeature.Count)
                throw new NotAllFeaturesWeightsDefinedException("There must be as many weights as features listed in 'CreatureFeature', " + (int)CreatureFeature.Count);

            bool[] featuresGiven = new bool[(int)CreatureFeature.Count]; //Bool default value = false
            float weightsSum = 0;
            foreach (Tuple<CreatureFeature, float> weight in weights)
            {
                featuresGiven[(int)weight.Item1] = true;
                weightsSum += weight.Item2;
            }

            if (weightsSum > 1)
                throw new SimilarityValuesDoNotAddUp("The total values of the weights of the genes must not exceed 1 as it is the maximum");
            if (weightsSum < 1 - weightsTotalValueWiggleRoom)
                throw new SimilarityValuesDoNotAddUp("The total values of the weights of the genes must be between " + (1 - weightsTotalValueWiggleRoom) + " and 1");
            foreach (bool feat in featuresGiven)
                if (!feat)
                    throw new NotAllFeaturesWeightsDefinedException("Each one of the " + (int)CreatureFeature.Count + " features declared in 'CreatureFeature' must have a weight value");

            return;
        }
        #endregion

        #region SimilaritySpeciesThreshold
        public class SimilarityThesholdNotValid : Exception
        {
            public SimilarityThesholdNotValid() { }
            public SimilarityThesholdNotValid(string message) : base(message) { }
            public SimilarityThesholdNotValid(string message, Exception inner) : base(message, inner) { }
            protected SimilarityThesholdNotValid(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

       
        #endregion

        #region AbilityUnlock
        public class NotAllAbilitiesUnlockDefinedException : Exception
        {
            public NotAllAbilitiesUnlockDefinedException() { }
            public NotAllAbilitiesUnlockDefinedException(string message) : base(message) { }
            public NotAllAbilitiesUnlockDefinedException(string message, Exception inner) : base(message, inner) { }
            protected NotAllAbilitiesUnlockDefinedException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
        public class AbilitiesUnlockNotNotInRange : Exception
        {
            public AbilitiesUnlockNotNotInRange() { }
            public AbilitiesUnlockNotNotInRange(string message) : base(message) { }
            public AbilitiesUnlockNotNotInRange(string message, Exception inner) : base(message, inner) { }
            protected AbilitiesUnlockNotNotInRange(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        static public void ValidateAbUnlock(Tuple<CreatureFeature, float>[] abUnlock)
        {
            CheckAllAbilitiesUnlockFunc(abUnlock);
        }

        static void CheckAllAbilitiesUnlockFunc(Tuple<CreatureFeature, float>[] abUnlock)
        {
            if (abUnlock.Length != ((int)CreatureFeature.Count - (int)CreatureFeature.Arboreal))
                throw new NotAllAbilitiesUnlockDefinedException("There must be as many abilities as abilities features listed in 'CreatureFeature', " + ((int)CreatureFeature.Count - (int)CreatureFeature.Arboreal));

            bool[] featuresGiven = new bool[abUnlock.Length]; //Bool default value = false

            foreach (Tuple<CreatureFeature, float> ab in abUnlock)
            {
                featuresGiven[(int)ab.Item1 - (int)CreatureFeature.Arboreal] = true;
                if (ab.Item2 < 0 || ab.Item2 > 1)
                {
                    throw new AbilitiesUnlockNotNotInRange("The value of " + ab.Item1 + " must be between 0 and 1");
                }
            }

            foreach (bool feat in featuresGiven)
                if (!feat)
                    throw new NotAllAbilitiesUnlockDefinedException("Each one of the " + ((int)CreatureFeature.Count - (int)CreatureFeature.Arboreal) + " abilities features declared in 'CreatureFeature' must have an ability unlock value");

            return;
        }
        #endregion

        #region UniverseParameters  
        public class UniverseParameterIsZeroException : Exception
        {
            public UniverseParameterIsZeroException() { }
            public UniverseParameterIsZeroException(string message) : base(message) { }
            public UniverseParameterIsZeroException(string message, Exception inner) : base(message, inner) { }
            protected UniverseParameterIsZeroException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        public class UniverseParameterBeyondMaxException : Exception
        {
            public UniverseParameterBeyondMaxException() { }
            public UniverseParameterBeyondMaxException(string message) : base(message) { }
            public UniverseParameterBeyondMaxException(string message, Exception inner) : base(message, inner) { }
            protected UniverseParameterBeyondMaxException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        public class MinMaxValueSwappedException : Exception
        {
            public MinMaxValueSwappedException() { }
            public MinMaxValueSwappedException(string message) : base(message) { }
            public MinMaxValueSwappedException(string message, Exception inner) : base(message, inner) { }
            protected MinMaxValueSwappedException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        public class PercentageOverOneException : Exception
        {
            public PercentageOverOneException() { }
            public PercentageOverOneException(string message) : base(message) { }
            public PercentageOverOneException(string message, Exception inner) : base(message, inner) { }
            protected PercentageOverOneException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }

        public static void Validate(UniverseParameters parameters)
        {
            ValidateWorld(parameters);

            ValidateCreature(parameters);

            ValidateCreatureStats(parameters);

            ValidatePlants(parameters);

            ValidateMemory(parameters);

            ValidateCreatureStates(parameters);

            ValidateCreatureTransitions(parameters);

            ValidateCorpse(parameters);

            ValidateTaxonomy(parameters);
        }

        static void ValidateWorld(UniverseParameters parameters) // TODO: REVISAR QUE ESTAN TODOS
        {
            if (parameters.ticksPerHour <= 0 || parameters.hoursPerDay <= 0 || parameters.daysPerYear <= 0 || parameters.morningStart <= 0 || parameters.nightStart <= 0)
                throw new UniverseParameterIsZeroException("The provided time parameters must be positive");
            if (parameters.morningStart > parameters.nightStart)
                throw new MinMaxValueSwappedException("The day must start before night starts");
            if (parameters.morningStart > parameters.hoursPerDay || parameters.nightStart > parameters.hoursPerDay)
                throw new UniverseParameterBeyondMaxException("The provided day change parameters are beyond the duration of a day in hours");
        }

        static void ValidateCreature(UniverseParameters parameters)
        {
            if (parameters.abilityUnlockPercentage <= 0) throw new UniverseParameterIsZeroException("The provided abilityUnlockPercentage must be positive");
            if (parameters.minHealth <= 0) throw new UniverseParameterIsZeroException("The provided minHealth must be positive");
            if (parameters.healthGainMultiplier <= 0) throw new UniverseParameterIsZeroException("The provided healthGainMultiplier must be positive");
            if (parameters.healthRegeneration <= 0) throw new UniverseParameterIsZeroException("The provided healthRegeneration must be positive");
            if (parameters.maxLimbs <= 0) throw new UniverseParameterIsZeroException("The provided maxLimbs must be positive");
            if (parameters.minRestExpense <= 0) throw new UniverseParameterIsZeroException("The provided minRestExpense must be positive");
            if (parameters.maxRestExpense <= 0) throw new UniverseParameterIsZeroException("The provided maxRestExpense must be positive");
            if (parameters.maxRestExpense < parameters.minRestExpense) throw new MinMaxValueSwappedException("The maximum amount of rest expense is lower than the minimium");
            if (parameters.resourceAmount <= 0) throw new UniverseParameterIsZeroException("The provided resourceAmount must be positive");
            if (parameters.minLifeSpan <= 0) throw new UniverseParameterIsZeroException("The provided minLifeSpan must be positive");
            if (parameters.exhaustToSleepRatio <= 0) throw new UniverseParameterIsZeroException("The provided exhaustToSleepRatio must be positive");
            if (parameters.hoursTilExhaustion <= 0) throw new UniverseParameterIsZeroException("The provided hoursTilExhaustion must be positive");
            if (parameters.perceptionWithoutNightVision <= 0) throw new UniverseParameterIsZeroException("The provided perceptionWithoutNightVision must be positive");
            if (parameters.minPerceptionWithNightVision <= 0) throw new UniverseParameterIsZeroException("The provided minPerceptionWithNightVision must be positive");
            // TODO: "nightPerceptionPenalty": 0.4, hay que ver que era y por que estaba en el jason
            if (parameters.minMobilityMedium <= 0) throw new UniverseParameterIsZeroException("The provided minMobilityMedium must be positive");
            if (parameters.mobilityPenalty <= 0) throw new UniverseParameterIsZeroException("The provided mobilityPenalty must be positive");
            if (parameters.yearsBetweenHeats <= 0) throw new UniverseParameterIsZeroException("The provided yearsBetweenHeats must be positive");
            if (parameters.ticksToReproduce <= 0) throw new UniverseParameterIsZeroException("The provided ticksToReproduce must be positive");
            if (parameters.maxSpeed <= 0) throw new UniverseParameterIsZeroException("The provided maxSpeed must be positive");
            if (parameters.hornIntimidationMultiplier <= 0) throw new UniverseParameterIsZeroException("The provided hornIntimidationMultiplier must be positive");
            if (parameters.hairTemperatureMultiplier < 0 || parameters.hairTemperatureMultiplier > 1) throw new UniverseParameterIsZeroException("The provided hairTemperatureMultiplier must be between 0 and 1");
            if (parameters.restRegenerationThreshold <= 0) throw new UniverseParameterIsZeroException("The provided restRegenerationThreshold must be positive");
            if (parameters.energyRegenerationThreshold <= 0) throw new UniverseParameterIsZeroException("The provided energyRegenerationThreshold must be positive");
            if (parameters.hydrationRegenerationThreshold <= 0) throw new UniverseParameterIsZeroException("The provided hydrationRegenerationThreshold must be positive");
            if (parameters.regenerationRate <= 0) throw new UniverseParameterIsZeroException("The provided regenerationRate must be positive");
            if (parameters.hoursTilStarvation <= 0) throw new UniverseParameterIsZeroException("The provided hoursTilStarvation must be positive");
            if (parameters.thirstToHungerRatio <= 0) throw new UniverseParameterIsZeroException("The provided thirstToHungerRatio must be positive");
            if (parameters.maxTemperatureAggressivenessPercentage <= 0) throw new UniverseParameterIsZeroException("The provided maxTemperatureAggressivenessPercentage must be positive");
            if (parameters.maxTemperatureDifference <= 0) throw new UniverseParameterIsZeroException("The provided maxTemperatureDifference must be positive");
            if (parameters.minHealthTemperatureDamage <= 0) throw new UniverseParameterIsZeroException("The provided minHealthTemperatureDamage must be positive");
            if (parameters.maxHealthTemperatureDamage <= 0) throw new UniverseParameterIsZeroException("The provided maxHealthTemperatureDamage must be positive");
            if (parameters.maxHealthTemperatureDamage < parameters.minHealthTemperatureDamage) throw new MinMaxValueSwappedException("The maximum amount of temperature damage is lower than the minimium");
            if (parameters.venomDamageMultiplier <= 0) throw new PercentageOverOneException("The provided venomDamageMultiplier must be positive");
            if (parameters.abilityUnlockPercentage > 1) throw new PercentageOverOneException("The provided abilityUnlockPercentage is over one");
            if (parameters.perceptionWithoutNightVision > 1) throw new PercentageOverOneException("The provided perceptionWithoutNightVision is over one");
            if (parameters.minPerceptionWithNightVision > 1) throw new PercentageOverOneException("The provided minPerceptionWithNightVision is over one");
            if (parameters.minMobilityMedium > 1) throw new PercentageOverOneException("The provided minMobilityMedium is over one");
            if (parameters.mobilityPenalty > 1) throw new PercentageOverOneException("The provided mobilityPenalty is over one");
            if (parameters.restRegenerationThreshold > 1) throw new PercentageOverOneException("The provided restRegenerationThreshold is over one");
            if (parameters.energyRegenerationThreshold > 1) throw new PercentageOverOneException("The provided energyRegenerationThreshold is over one");
            if (parameters.hydrationRegenerationThreshold > 1) throw new PercentageOverOneException("The provided hydrationRegenerationThreshold is over one");
            if (parameters.maxTemperatureAggressivenessPercentage > 1) throw new PercentageOverOneException("The provided maxTemperatureAggressivenessPercentage is over one");
            if (parameters.minHealthTemperatureDamage > 1) throw new PercentageOverOneException("The provided minHealthTemperatureDamage is over one");
            if (parameters.maxHealthTemperatureDamage > 1) throw new PercentageOverOneException("The provided maxHealthTemperatureDamage is over one");
            if (parameters.regenerationRate > 1) throw new PercentageOverOneException("The provided regenerationRate is over one");

        }

        static void ValidateCreatureStats(UniverseParameters parameters)
        {
            //TODO: Hay que ver que de Creature va aqui y viceversa, pues son metodos distintos

            if (parameters.newbornStatMultiplier <= 0) throw new UniverseParameterIsZeroException("The provided newbornStatMultiplier must be positive");
            if (parameters.adulthoodThreshold <= 0) throw new UniverseParameterIsZeroException("The provided adulthoodThreshold must be positive");
            if (parameters.tiredThreshold <= 0) throw new UniverseParameterIsZeroException("The provided tiredThreshold must be positive");
            if (parameters.exhaustThreshold <= 0) throw new UniverseParameterIsZeroException("The provided exhaustThreshold must be positive");
            if (parameters.hungryThreshold <= 0) throw new UniverseParameterIsZeroException("The provided hungryThreshold must be positive");
            if (parameters.veryHungryThreshold <= 0) throw new UniverseParameterIsZeroException("The provided veryHungryThreshold must be positive");
            if (parameters.thirstyThreshold <= 0) throw new UniverseParameterIsZeroException("The provided thirstyThreshold must be positive");
            if (parameters.veryThirstyThreshold <= 0) throw new UniverseParameterIsZeroException("The provided veryThirstyThreshold must be positive");
            if (parameters.actionPerceptionPercentage <= 0) throw new UniverseParameterIsZeroException("The provided actionPerceptionPercentage must be positive");
            if (parameters.minPerception <= 0) throw new UniverseParameterIsZeroException("The provided minPerception must be positive");
            if (parameters.maxPerception <= 0) throw new UniverseParameterIsZeroException("The provided maxPerception must be positive");
            if (parameters.newbornStatMultiplier > 1) throw new PercentageOverOneException("The provided newbornStatMultiplier is over one");
            if (parameters.adulthoodThreshold > 1) throw new PercentageOverOneException("The provided adulthoodThreshold is over one");
            if (parameters.tiredThreshold > 1) throw new PercentageOverOneException("The provided tiredThreshold is over one");
            if (parameters.exhaustThreshold > 1) throw new PercentageOverOneException("The provided exhaustThreshold is over one");
            if (parameters.hungryThreshold > 1) throw new PercentageOverOneException("The provided hungryThreshold is over one");
            if (parameters.veryHungryThreshold > 1) throw new PercentageOverOneException("The provided veryHungryThreshold is over one");
            if (parameters.thirstyThreshold > 1) throw new PercentageOverOneException("The provided thirstyThreshold is over one");
            if (parameters.veryThirstyThreshold > 1) throw new PercentageOverOneException("The provided veryThirstyThreshold is over one");
            if (parameters.actionPerceptionPercentage > 1) throw new PercentageOverOneException("The provided actionPerceptionPercentage is over one");
        }

        static void ValidatePlants(UniverseParameters parameters)
        {
            if (parameters.treeMovementPenalty <= 0) throw new UniverseParameterIsZeroException("The provided treeMovementPenalty must be positive");
            if (parameters.treeMovementPenalty > 1) throw new PercentageOverOneException("The provided treeMovementPenalty is over one");

            if(parameters.bushHp <= 0) throw new PercentageOverOneException("The provided bushHp muts be positive");
            if(parameters.grassHp <= 0) throw new PercentageOverOneException("The provided grassHp muts be positive");
            if(parameters.eTreeHp <= 0) throw new PercentageOverOneException("The provided eTreeHp muts be positive");
        }

        static void ValidateMemory(UniverseParameters parameters)
        {
            if (parameters.knowledgeTickMultiplier <= 0 || /*parameters.perceptionToRadiusMultiplier <= 0 || */parameters.aggressivenessToRadiusMultiplier <= 0 
                || parameters.maxResourcesRemembered < 0 || parameters.maxPositionsRemembered < 0)
                throw new UniverseParameterIsZeroException("The provided memory parameters must be positive");
        }

        static void ValidateCreatureStates(UniverseParameters parameters)
        {
            if (parameters.baseActionCost <= 0 || parameters.venomCostMultiplier <= 0 || parameters.chaseCostMultiplier <= 0 || parameters.fleeingCostMultiplier <= 0 ||
                parameters.drinkingCostMultiplier <= 0 || parameters.eatingCostMultiplier <= 0 || parameters.sleepingCostMultiplier <= 0 || parameters.mutationChance <= 0 ||
                 parameters.drinkingMultiplier <= 0 || parameters.adjacentLength <= 0)
                throw new UniverseParameterIsZeroException("The provided state parameters must be positive");
            if (parameters.mutationChance > 1)
                throw new PercentageOverOneException("The provided state percentages are over one");
        }

        static void ValidateCreatureTransitions(UniverseParameters parameters)
        {
            if (parameters.fleeingTransitionMultiplier <= 0 || parameters.hidingTransitionMultiplier <= 0 || parameters.stopEatingTransitionEnergyMultiplier <= 0 ||
                parameters.combatTransitionHealthThresholdMultiplier <= 0 || parameters.safeTransitionAggressivenessThreshold <= 0 || parameters.experienceMaxAggresivenessMultiplier <= 0 || 
                parameters.safePrefferedOverClosestResourceRatio <= 0)
                throw new UniverseParameterIsZeroException("The provided transition parameters must be positive");
            // TODO: "combatTransitionAggressivenessThreshold": 0.5, hay que saber que era y que hacia y por que no estaba en los parameters
            // TODO: "escapeTransitionAggressivenessThreshold": 0.5, hay que saber que era y que hacia y por que no estaba en los parameters
            // TODO: "escapeTransitionHealthThresholdMultiplier": 50.0, hay que saber que era y que hacia y por que no estaba en los parameters
        }

        static void ValidateCorpse(UniverseParameters parameters)
        {
            if (parameters.rotStartMultiplier <= 0 || parameters.corpseNutritionPointsMultiplier <= 0)
                throw new UniverseParameterIsZeroException("The provided corpse parameters must be positive");
        }

        static void ValidateTaxonomy(UniverseParameters parameters)
        {
            if (parameters.percentageOfSpeciesToExport <= 0)
                throw new UniverseParameterIsZeroException("The provided taxonomy parameters species to export must be positive");
            if (parameters.percentageOfSpeciesToExport > 1)
                throw new PercentageOverOneException("The provided taxonomy parameters species to export cannot be over 1");

            if (parameters.percentageSimilaritySpecies < 0 || parameters.percentageSimilaritySpecies >= 1)
                throw new SimilarityThesholdNotValid("The provided similarity species threshold must be a positive number smaller than one to allow reproduction");
        }
        #endregion
    }
}
