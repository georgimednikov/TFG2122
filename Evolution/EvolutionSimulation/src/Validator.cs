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
        #region WorldGenConfig
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
            if (config.mapSize <= 0) throw new ApplicationException("The provided size is equal or less than 0");

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

                if (defHumidity && (sizeX != config.humidityMap.GetLength(0) || sizeY != config.humidityMap.GetLength(1))) { throw new ApplicationException("The provided maps aren't of the same size"); }
                if (defTemp && (sizeX != config.temperatureMap.GetLength(0) || sizeY != config.temperatureMap.GetLength(1))) { throw new ApplicationException("The provided maps aren't of the same size"); }
            }
            else if (defHumidity)
            {
                sizeX = config.humidityMap.GetLength(0);
                sizeY = config.humidityMap.GetLength(1);

                if (defTemp && (sizeX != config.temperatureMap.GetLength(0) || sizeY != config.temperatureMap.GetLength(1))) { throw new ApplicationException("The provided maps aren't of the same size"); }
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
                    throw new ApplicationException("The provided height function is not defined for the requested input range");
                }
                if (val < 0 || val > 1) throw new ApplicationException("The provided height function has an incorrect output range");
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
                    throw new ApplicationException("The provided influence function is not defined for the requested input range");
                }
                if (val < -1 || val > 1) throw new ApplicationException("The provided influence function has an incorrect output range");
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
                        throw new ApplicationException("The provided flora function is not defined for the requested input range");
                    }
                    if (val < 0 || val > 1) throw new ApplicationException("The provided flora function has an incorrect output range");
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
                        throw new ApplicationException("The provided flora function is not defined for the requested input range");
                    }
                    if (val < 0 || val > 1) throw new ApplicationException("The provided flora function has an incorrect output range");
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
                    throw new ApplicationException("The provided height function is not defined for the requested input range");
                }
                if (val < 0 || val > 4) throw new ApplicationException("The provided flora selector function has an incorrect output range");
            }
            return;
        }
        #endregion
    }
}
