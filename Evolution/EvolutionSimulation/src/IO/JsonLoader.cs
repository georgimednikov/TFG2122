using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace EvolutionSimulation.IO
{
    static class JsonLoader
    {
        public static T Deserialize<T>(string file)
        {
            if (file == null) throw new NullFileException("Could not find the JSON to deserialize");

            T deserialized;
            try
            {
                deserialized = JsonConvert.DeserializeObject<T>(file);
            }
            catch { throw new WrongFormattingException("Could not deserialize JSON due to wrong formatting"); }
            return deserialized;
        }
    }

    public class NullFileException : Exception
    {
        public NullFileException() { }
        public NullFileException(string message) : base(message) { }
        public NullFileException(string message, Exception inner) : base(message, inner) { }
        protected NullFileException(
          SerializationInfo info,
          StreamingContext context) : base(info, context) { }
    }
    public class WrongFormattingException : Exception
    {
        public WrongFormattingException() { }
        public WrongFormattingException(string message) : base(message) { }
        public WrongFormattingException(string message, Exception inner) : base(message, inner) { }
        protected WrongFormattingException(
          SerializationInfo info,
          StreamingContext context) : base(info, context) { }
    }
}
