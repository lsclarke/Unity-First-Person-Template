using System;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Model
{
    internal class SqpParseException : Exception
    {
        public SqpParseException(string field, Exception inner) : base($"failed to parse SQP packet at field '{field}'",
            inner)
        {
        }
    }
}
