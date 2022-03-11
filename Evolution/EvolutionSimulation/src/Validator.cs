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

        static public void Validate(float similarity)
        {
            SimilarityThresholdFunc(similarity);
        }

        static void SimilarityThresholdFunc(float similarity)
        {
            if (similarity < 0 || similarity >= 1)
                throw new SimilarityThesholdNotValid("The provided similarity species threshold must be a positive number smaller than one to allow reproduction");

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

            ValidateTrees(parameters);

            ValidateMemory(parameters);

            ValidateCreatureStates(parameters);

            ValidateCreatureTransitions(parameters);

            ValidateCorpse(parameters);
        }

        static void ValidateWorld(UniverseParameters parameters)
        {
            if (parameters.ticksPerHour <= 0 || parameters.hoursPerDay <= 0 || parameters.daysPerYear <= 0 || parameters.morningStart <= 0 || parameters.nightStart <= 0) 
                throw new UniverseParameterIsZeroException("The provided time parameters must be positive");
            if(parameters.morningStart > parameters.nightStart)
                throw new MinMaxValueSwappedException("The day must start before night starts");
            if(parameters.morningStart > parameters.hoursPerDay || parameters.nightStart > parameters.hoursPerDay)
                throw new UniverseParameterBeyondMaxException("The provided day change parameters are beyond the duration of a day in hours");
        }

        static void ValidateCreature(UniverseParameters parameters)
        {
            if (parameters.abilityUnlockPercentage <= 0 || parameters.minHealth <= 0 || parameters.healthGainMultiplier <= 0 || 
                parameters.healthRegeneration <= 0 || parameters.maxLimbs <= 0 || parameters.minRestExpense <= 0 || parameters.maxRestExpense <= 0 ||
                parameters.resourceAmount <= 0 || parameters.minLifeSpan <= 0 || parameters.exhaustToSleepRatio <= 0 || parameters.perceptionWithoutNightVision <= 0 || parameters.minPerceptionWithNightVision <= 0 ||
                parameters.minMobilityMedium <= 0 || parameters.mobilityPenalty <= 0 || parameters.maxSpeed <= 0 || parameters.hornIntimidationMultiplier <= 0 ||
                parameters.restRegenerationThreshold <= 0 || parameters.energyRegenerationThreshold <= 0 || parameters.hydrationRegenerationThreshold <= 0 || 
                parameters.regenerationRate <= 0 || parameters.hoursTilStarvation <= 0 || parameters.thirstToHungerRatio <= 0) 
                throw new UniverseParameterIsZeroException("The provided creature parameters must be positive");
            if (parameters.maxRestExpense < parameters.minRestExpense)
                throw new MinMaxValueSwappedException("The maximum amount of rest expense is lower than the minimium");
            if (parameters.abilityUnlockPercentage > 1 || parameters.perceptionWithoutNightVision > 1 || parameters.minPerceptionWithNightVision > 1 || parameters.minMobilityMedium > 1 || parameters.mobilityPenalty > 1 || 
                parameters.restRegenerationThreshold > 1 || parameters.energyRegenerationThreshold > 1 || parameters.hydrationRegenerationThreshold > 1 || 
                parameters.regenerationRate > 1)
                throw new PercentageOverOneException("The provided creature percentages are over one");
        }

        static void ValidateCreatureStats(UniverseParameters parameters)
        {
            if (parameters.newbornStatMultiplier <= 0 || parameters.adulthoodThreshold <= 0 || parameters.tiredThreshold <= 0 || parameters.exhaustThreshold <= 0 ||
                parameters.hungryThreshold <= 0 || parameters.veryHungryThreshold <= 0 || parameters.thirstyThreshold <= 0 || parameters.veryThirstyThreshold <= 0) 
                throw new UniverseParameterIsZeroException("The provided stat parameters must be positive");
            if (parameters.newbornStatMultiplier > 1 || parameters.adulthoodThreshold > 1 || parameters.tiredThreshold > 1 || parameters.exhaustThreshold > 1 ||
                parameters.hungryThreshold > 1 || parameters.veryHungryThreshold > 1 || parameters.thirstyThreshold > 1 || parameters.veryThirstyThreshold > 1)
                throw new PercentageOverOneException("The provided stat percentages are over one");
        }

        static void ValidateTrees(UniverseParameters parameters)
        {
            if (parameters.treeMovementPenalty <= 0) 
                throw new UniverseParameterIsZeroException("The provided tree parameters must be positive");
            if (parameters.treeMovementPenalty > 1)
                throw new PercentageOverOneException("The provided tree percentages are over one");
        }

        static void ValidateMemory(UniverseParameters parameters)
        {
            if (parameters.knowledgeTickMultiplier <= 0 || parameters.perceptionToRadiusMultiplier <= 0 || parameters.aggressivenessToRadiusMultiplier <= 0) 
                throw new UniverseParameterIsZeroException("The provided tree parameters must be positive");
        }

        static void ValidateCreatureStates(UniverseParameters parameters)
        {
            if (parameters.baseActionCost <= 0 || parameters.venomCostMultiplier <= 0 || parameters.chaseCostMultiplier <= 0 || parameters.fleeingCostMultiplier <= 0 ||
                parameters.drinkingCostMultiplier <= 0 || parameters.eatingCostMultiplier <= 0 || parameters.sleepingCostMultiplier <= 0|| parameters.mutationChance <= 0 ||
                 parameters.drinkingMultiplier <= 0 || parameters.adjacentLength <= 0) 
                throw new UniverseParameterIsZeroException("The provided state parameters must be positive");
            if (parameters.mutationChance > 1)
                throw new PercentageOverOneException("The provided state percentages are over one");
        }

        static void ValidateCreatureTransitions(UniverseParameters parameters)
        {
            if (parameters.fleeingTransitionMultiplier <= 0 || parameters.hidingTransitionMultiplier <= 0 || parameters.stopEatingTransitionEnergyMultiplier <= 0 || 
                parameters.combatTransitionHealthThresholdMultiplier <= 0 || parameters.escapeTransitionHealthThresholdMultiplier <= 0 || 
                parameters.safeTransitionAggressivenessThreshold <= 0 || parameters.experienceMaxAggresivenessMultiplier <= 0 || parameters.safePrefferedOverClosestResourceRatio <= 0) 
                throw new UniverseParameterIsZeroException("The provided transition parameters must be positive");
        }

        static void ValidateCorpse(UniverseParameters parameters)
        {
            if (parameters.rotStartMultiplier <= 0 || parameters.corpseNutritionPointsMultiplier <= 0) 
                throw new UniverseParameterIsZeroException("The provided corpse parameters must be positive");
        }
        #endregion
    }
}
