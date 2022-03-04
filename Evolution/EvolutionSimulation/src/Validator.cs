using System;
using System.Collections.Generic;
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
    }
}
