using System.Reflection;
using Mutators.Mutators;

namespace Mutators.Tests.Extensions.Test
{
    internal static class MutatorTestExtensions
    {
        private const string metadataFieldName = "_metadata";
        
        internal static IDictionary<string, object> GetMetadata(this IMutator mutator)
        {
            FieldInfo? metadataField = mutator.GetType().GetField(metadataFieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            if (metadataField == null)
            {
                throw new MissingFieldException(nameof(IMutator), metadataFieldName);
            }
            
            return (IDictionary<string, object>) metadataField.GetValue(mutator)!;
        }
    }
}