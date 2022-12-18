using BepInEx;
using ButterHands.Configs;
using ButterHands.Customizations;
using FistVR;
using System;
using UnityEngine;

namespace ButterHands
{
	[BepInPlugin(PluginInfo.GUID, PluginInfo.NAME, PluginInfo.VERSION)]
	[BepInDependency("nrgill28.Sodalite")]
	[BepInProcess("h3vr.exe")]
	public class Plugin : BaseUnityPlugin
	{
		private readonly RootConfig _config;
		private readonly Customization _customization;
		private readonly MagPalm _magPalm;

		//private IDisposable? _leaderboardLock;

		public Plugin()
		{
			_config = new RootConfig(Config);
			_customization = new Customization(_config);
			_magPalm = new MagPalm(_config, Logger);
		}

		private void Awake()
		{
			_magPalm.Hook();
			if (_config.Customization.Hands.aEnable.Value) _customization.HandsHook();

			if (_config.Customization.QBSlots.aEnable.Value) _customization.QBSlotsHook();
		}

		private void OnDestroy()
		{
			_magPalm.Unhook();
		}

		// Return the gameobject geo we are using
		public static GameObject GetControllerFrom(FVRViveHand hand)
		{
			var controllerGeos = new GameObject[]
			{
				hand.Display_Controller_Cosmos,
				hand.Display_Controller_HPR2,
				hand.Display_Controller_Index,
				hand.Display_Controller_Quest2,
				hand.Display_Controller_RiftS,
				hand.Display_Controller_Touch,
				hand.Display_Controller_Vive,
				hand.Display_Controller_WMR
			};

			var geo = Array.Find(controllerGeos, g => g.activeSelf);

			return geo ?? hand.Display_Controller;
		}
	}
}