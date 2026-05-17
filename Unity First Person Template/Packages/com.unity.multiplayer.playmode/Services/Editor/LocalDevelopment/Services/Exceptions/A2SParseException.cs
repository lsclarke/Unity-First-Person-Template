using System;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Exceptions
{
    internal class A2SParseException : Exception
    {
        public A2SParseException(string field, Exception inner) : base($"failed to parse A2S packet at field '{field}'",
            inner)
        {
        }
    }
}
