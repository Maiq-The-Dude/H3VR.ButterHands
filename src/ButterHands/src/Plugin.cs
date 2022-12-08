using BepInEx;

namespace ButterHands.src
{
	[BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
	[BepInDependency("nrgill28.Sodalite")]
	[BepInProcess("h3vr.exe")]
	public class Plugin : BaseUnityPlugin
	{
		private readonly Hooks _hooks;

		public Plugin()
		{
			_hooks = new Hooks();
			_hooks.Hook();
		}

		private void Awake()
		{

		}

		private void OnDestroy()
		{
			_hooks.Unhook();
		}
	}
}