namespace Mutators.Tests.Mutators.Patches
{
    public class HookOrderTestPatch
    {
        internal static List<string> Calls { get; } = [];

        static void BeforePatchAll() => Calls.Add("patch:BeforePatchAll");
        static void AfterPatchAll() => Calls.Add("patch:AfterPatchAll");
        static void BeforeUnpatchAll() => Calls.Add("patch:BeforeUnpatchAll");
        static void AfterUnpatchAll() => Calls.Add("patch:AfterUnpatchAll");
    }
}
