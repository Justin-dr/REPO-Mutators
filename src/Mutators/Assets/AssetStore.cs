namespace Mutators.Assets
{
    internal class AssetStore
    {
        internal const string FIRING_MY_LASER_PREFAB_ID = MyPluginInfo.PLUGIN_GUID + "/FiringMyLaser";
        internal static bool IsLaserLoaded { get; set; }
        internal static ExplosionPreset Preset { get; set; }
    }
}
