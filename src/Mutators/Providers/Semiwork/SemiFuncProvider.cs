namespace Mutators.Providers.Semiwork
{
    internal class SemiFuncProvider : ISemiFuncProvider
    {
        public int MoonLevel()
        {
            return SemiFunc.MoonLevel();
        }

        public bool IsMultiplayer()
        {
            return SemiFunc.IsMultiplayer();
        }
    }
}