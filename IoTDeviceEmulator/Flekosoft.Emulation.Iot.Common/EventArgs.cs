using System;

namespace Flekosoft.Emulation.Iot.Common
{
    public class JsonStringEventArgs : EventArgs
    {
        public JsonStringEventArgs(string jsonString)
        {
            JsonString = jsonString;
        }

        public string JsonString { get; }
    }
}
