using BepInEx.Logging;
using ButterHands.Configs;
using FistVR;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;

namespace ButterHands
{
	public class MagPalm
	{
		private readonly RootConfig _config;

		private readonly ManualLogSource _loggers;
		public MagPalm(RootConfig config, ManualLogSource logger)
		{
			_config = config;
			_loggers = logger;
		}

		public void Hook()
		{
			On.FistVR.FVRFireArmMagazine.Awake += FVRFireArmMagazine_Awake;
			IL.FistVR.FVRFireArmMagazine.UpdateInteraction += FVRFireArmMagazine_UpdateInteraction;
		}

		public void Unhook()
		{
			On.FistVR.FVRFireArmMagazine.Awake -= FVRFireArmMagazine_Awake;
			IL.FistVR.FVRFireArmMagazine.UpdateInteraction -= FVRFireArmMagazine_UpdateInteraction;
		}

		private void FVRFireArmMagazine_Awake(On.FistVR.FVRFireArmMagazine.orig_Awake orig, FistVR.FVRFireArmMagazine self)
		{
			orig(self);

			if (_config.zCheat.CursedPalms.Value)
			{
				self.m_canPalm = true;
			}
		}

		private void FVRFireArmMagazine_UpdateInteraction(ILContext il)
		{
			ILCursor c = new ILCursor(il);
			var x = c.TryGotoNext(
				MoveType.Before,
				i => i.MatchLdloc(0),
				i => i.MatchBrfalse(out _),
				i => i.MatchLdarg(0),
				i => i.MatchCall(typeof(FVRFireArmMagazine), nameof(FistVR.FVRFireArmMagazine.GetCanPalm)),
				i => i.MatchBrfalse(out _)
			);

			int i = c.Index + 1; // +1 because we want to also remove the ldloc.0 (first instr in above match)
			c.Index = 0;
			c.RemoveRange(i);
			c.Emit(OpCodes.Ldarg_1);
			c.EmitDelegate<Func<FVRViveHand, bool>>(GetInput);
		}

		private bool GetInput(FVRViveHand hand)
		{
			// Get input from hand & config
			var cfg = _config.MagPalm.Controls;
			var value = hand.IsThisTheRightHand ? cfg.RightKeybind.Value : cfg.LeftKeybind.Value;
			var handInput = hand.Input;
			var magnitude = handInput.TouchpadAxes.magnitude > cfg.ClickPressure.Value;
			return value switch
			{
				MagPalmControlsConfig.Keybind.AXButton => handInput.AXButtonDown,
				MagPalmControlsConfig.Keybind.BYButton => handInput.BYButtonDown,
				MagPalmControlsConfig.Keybind.Grip => handInput.GripDown,
				MagPalmControlsConfig.Keybind.Secondary2AxisNorth => handInput.Secondary2AxisNorthDown,
				MagPalmControlsConfig.Keybind.Secondary2AxisSouth => handInput.Secondary2AxisSouthDown,
				MagPalmControlsConfig.Keybind.Secondary2AxisEast => handInput.Secondary2AxisEastDown,
				MagPalmControlsConfig.Keybind.Secondary2AxisWest => handInput.Secondary2AxisWestDown,
				MagPalmControlsConfig.Keybind.TouchpadClickNorth => handInput.TouchpadDown && magnitude && Vector2.Angle(handInput.TouchpadAxes, Vector2.up) <= 45f,
				MagPalmControlsConfig.Keybind.TouchpadClickSouth => handInput.TouchpadDown && magnitude && Vector2.Angle(handInput.TouchpadAxes, Vector2.down) <= 45f,
				MagPalmControlsConfig.Keybind.TouchpadClickEast => handInput.TouchpadDown && magnitude && Vector2.Angle(handInput.TouchpadAxes, Vector2.right) <= 45f,
				MagPalmControlsConfig.Keybind.TouchpadClickWest => handInput.TouchpadDown && magnitude && Vector2.Angle(handInput.TouchpadAxes, Vector2.left) <= 45f,
				MagPalmControlsConfig.Keybind.TouchpadTapNorth => handInput.TouchpadNorthDown,
				MagPalmControlsConfig.Keybind.TouchpadTapSouth => handInput.TouchpadSouthDown,
				MagPalmControlsConfig.Keybind.TouchpadTapEast => handInput.TouchpadEastDown,
				MagPalmControlsConfig.Keybind.TouchpadTapWest => handInput.TouchpadWestDown,
				MagPalmControlsConfig.Keybind.Trigger => handInput.TriggerDown,
				_ => false,
			};
		}
	}
}