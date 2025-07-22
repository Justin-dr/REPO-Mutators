namespace Mutators.Providers.Semiwork
{
    /// <summary>
    /// Wrapper around SemiFunc to keep R.E.P.O. references out of the MutatorManager.
    /// </summary>
    internal interface ISemiFuncProvider
    {
        int MoonLevel();
        bool IsMultiplayer();
    }
}