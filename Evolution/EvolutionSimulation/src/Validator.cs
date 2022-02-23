using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation
{
    /// <summary>
    /// Class used to validate.
    /// </summary>
    public static class Validator
    {
        public enum ValidatorResult
        { 
            #region WorldGenConfig
            NonPositiveSizeMap,
            NoSameSizeMaps,
            HeightFunctionOutOfBounds,
            HeightFunctionNotWhollyDefined,
            InfluenceFunctionOutOfBounds,
            InfluenceFunctionNotWhollyDefined,
            FloraFunctionOutOfBounds,
            FloraFunctionNotWhollyDefined,
            FloraSelectorFunctionOutOfBounds,
            FloraSelectorFunctionNotWhollyDefined,
            #endregion
            NoError
        }

        static public void ExceptionThrow(ValidatorResult result)
        {
            switch (result)
            {
                #region WorldGenConfig
                case ValidatorResult.NonPositiveSizeMap:
                    throw new ApplicationException("The provided size is equal or less than 0");
                case ValidatorResult.NoSameSizeMaps:
                    throw new ApplicationException("The provided maps aren't of the same size");
                case ValidatorResult.HeightFunctionOutOfBounds:
                    throw new ApplicationException("The provided height function has an incorrect output range");
                case ValidatorResult.HeightFunctionNotWhollyDefined:
                    throw new ApplicationException("The provided height function is not defined for the requested input range");
                case ValidatorResult.InfluenceFunctionOutOfBounds:
                    throw new ApplicationException("The provided influence function has an incorrect output range");
                case ValidatorResult.InfluenceFunctionNotWhollyDefined:
                    throw new ApplicationException("The provided influence function is not defined for the requested input range");
                case ValidatorResult.FloraFunctionOutOfBounds:
                    throw new ApplicationException("The provided flora function has an incorrect output range");
                case ValidatorResult.FloraFunctionNotWhollyDefined:
                    throw new ApplicationException("The provided flora function is not defined for the requested input range");
                case ValidatorResult.FloraSelectorFunctionOutOfBounds:
                    throw new ApplicationException("The provided flora selector function has an incorrect output range");
                case ValidatorResult.FloraSelectorFunctionNotWhollyDefined:
                    throw new ApplicationException("The provided height function is not defined for the requested input range");
                #endregion
                default:
                    break;
            }
        }

        #region WorldGenConfig
        static double funcValidationAccuracy = 0.1;
        static public ValidatorResult Validate(WorldGenConfig configuration)
        {
            ValidatorResult result = ValidatorResult.NoError;
            if ((result = NonZeroSize(configuration)) != ValidatorResult.NoError) return result;
            if ((result = SameSizeMaps(configuration)) != ValidatorResult.NoError) return result;
            if ((result = HeightFunc(configuration)) != ValidatorResult.NoError) return result;
            if ((result = InfluenceFunc(configuration)) != ValidatorResult.NoError) return result;
            if ((result = FloraFunc(configuration)) != ValidatorResult.NoError) return result;
            if ((result = TemperatureSoftenerFunc(configuration)) != ValidatorResult.NoError) return result;
            if ((result = FloraSelectorFunc(configuration)) != ValidatorResult.NoError) return result;
            return result;
        }

        static ValidatorResult NonZeroSize(WorldGenConfig config)
        {
            if (config.humidityMap != null || config.temperatureMap != null || config.heightMap != null) return ValidatorResult.NoError;
            if (config.mapSize <= 0) return ValidatorResult.NonPositiveSizeMap;

            return ValidatorResult.NoError;
        }

        static ValidatorResult SameSizeMaps(WorldGenConfig config)
        {

            int defined = 0;
            bool defHeight = (config.heightMap != null), defHumidity = (config.humidityMap != null), defTemp = (config.temperatureMap != null);
            if (defHeight) defined++;
            if (defHumidity) defined++;
            if (defTemp) defined++;

            if (defined <= 1) return ValidatorResult.NoError;

            int sizeX, sizeY;
            if (defHeight)
            {
                sizeX = config.heightMap.GetLength(0);
                sizeY = config.heightMap.GetLength(1);

                if (defHumidity && (sizeX != config.humidityMap.GetLength(0) || sizeY != config.humidityMap.GetLength(1))) { return ValidatorResult.NoSameSizeMaps; }
                if (defTemp && (sizeX != config.temperatureMap.GetLength(0) || sizeY != config.temperatureMap.GetLength(1))) { return ValidatorResult.NoSameSizeMaps; }
            }
            else if (defHumidity)
            {
                sizeX = config.humidityMap.GetLength(0);
                sizeY = config.humidityMap.GetLength(1);

                if (defTemp && (sizeX != config.temperatureMap.GetLength(0) || sizeY != config.temperatureMap.GetLength(1))) { return ValidatorResult.NoSameSizeMaps; }
            }
            return ValidatorResult.NoError;
        }

        static ValidatorResult HeightFunc(WorldGenConfig config)
        {
            if (config.evaluateHeight == null) return ValidatorResult.NoError;
            double val;
            for (double i = 0; i <= 1; i += funcValidationAccuracy)
            {
                try
                {
                    val = config.evaluateHeight(i);
                }
                catch
                {
                    return ValidatorResult.HeightFunctionNotWhollyDefined;
                }
                if (val < 0 || val > 1) return ValidatorResult.HeightFunctionOutOfBounds;
            }
            return ValidatorResult.NoError;
        }

        static ValidatorResult InfluenceFunc(WorldGenConfig config)
        {
            if (config.evaluateInfluence == null) return ValidatorResult.NoError;
            double val;
            for (double i = 0; i <= 1; i += funcValidationAccuracy)
            {
                try
                {
                    val = config.evaluateInfluence(i);
                }
                catch
                {
                    return ValidatorResult.InfluenceFunctionNotWhollyDefined;
                }
                if (val < -1 || val > 1) return ValidatorResult.InfluenceFunctionOutOfBounds;
            }
            return ValidatorResult.NoError;
        }

        static ValidatorResult FloraFunc(WorldGenConfig config)
        {
            if (config.evaluateFlora == null) return ValidatorResult.NoError;
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
                        return ValidatorResult.FloraFunctionNotWhollyDefined;
                    }
                    if (val < 0 || val > 1) return ValidatorResult.FloraFunctionOutOfBounds;
                }
            return ValidatorResult.NoError;
        }

        static ValidatorResult TemperatureSoftenerFunc(WorldGenConfig config)
        {
            if (config.evaluateFlora == null) return ValidatorResult.NoError;
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
                        return ValidatorResult.FloraFunctionNotWhollyDefined;
                    }
                    if (val < 0 || val > 1) return ValidatorResult.FloraFunctionOutOfBounds;
                }
            return ValidatorResult.NoError;
        }

        static ValidatorResult FloraSelectorFunc(WorldGenConfig config)
        {
            if (config.floraSelector == null) return ValidatorResult.NoError;
            int val;
            for (double i = funcValidationAccuracy; i <= 1; i += funcValidationAccuracy)
            {
                try
                {
                    val = config.floraSelector(i);
                }
                catch
                {
                    return ValidatorResult.FloraSelectorFunctionNotWhollyDefined;
                }
                if (val < 0 || val > 4) return ValidatorResult.FloraSelectorFunctionOutOfBounds;
            }
            return ValidatorResult.NoError;
        }
        #endregion
    }
}
