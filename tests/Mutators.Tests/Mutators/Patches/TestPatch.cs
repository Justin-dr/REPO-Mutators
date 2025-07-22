namespace Mutators.Tests.Mutators.Patches
{
    public class TestPatch
    {
        internal static IDictionary<string, object>? Metadata { get; set; }
        
        static void OnMetadataChanged(IDictionary<string, object> metadata)
        {
            Metadata = metadata;
        }
    }
}