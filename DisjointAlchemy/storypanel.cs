﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
//using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
//using Quintessential.Serialization;
//using Quintessential.Settings;
//using SDL2;
using System;
//using System.IO;
using System.Linq;
using System.Collections.Generic;
//using System.Globalization;
//using System.Reflection;

namespace DisjointAlchemy;

//using PartType = class_139;
//using Permissions = enum_149;
//using BondType = enum_126;
//using BondSite = class_222;
//using AtomTypes = class_175;
//using PartTypes = class_191;
using Texture = class_256;
//using Song = class_186;
//using Tip = class_215;
//using Font = class_1;

public static class StoryPanelPatcher
{
	private static Puzzle optionsUnlock = null;
	public const string optionsID = "Disjoint-options";

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// helpers
	public static void setOptionsUnlock(Puzzle puzzle)
	{
		if (optionsUnlock == null) optionsUnlock = puzzle;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// public functions
	public static void LoadContent()
	{
	}

	public static void Load()
	{
	}

	public static void PostLoad()
	{
		IL.StoryPanel.method_2175 += skipDrawingTheReturnButton;
		//On.OptionsScreen.method_50 += hotswapOptionsStorypanel;
		On.StoryPanel.method_2172 += customStorypanelUnlocks;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// hooking

	private static void skipDrawingTheReturnButton(ILContext il)
	{
		ILCursor cursor = new ILCursor(il);
		// skip ahead to roughly where the "check if we need to draw the Return button" code occurs
		cursor.Goto(772);

		// jump ahead to just after the comparison to the string "options" was made
		if (!cursor.TryGotoNext(MoveType.After, instr => instr.Match(OpCodes.Call))) return;

		// load the StoryPanel self onto the stack so we can use it
		cursor.Emit(OpCodes.Ldarg_0);

		// then run the new code
		cursor.EmitDelegate<Func<bool, StoryPanel, bool>>((isOptionsScreen, panel_self) =>
		{
			// return TRUE if we need to draw the Return button
			string storyPanelID = new DynamicData(panel_self).Get<Maybe<class_264>>("field_4090").method_1087().field_2090;
			if (storyPanelID == SigmarGardenPatcher.solitaireID) return false;
			if (storyPanelID == optionsID) return false;
			//if (JournalLoader.journal_puzzles.Any(x => x.ID == storyPanelID)) return false;

			return isOptionsScreen;
		});
	}

	public static void hotswapOptionsStorypanel(On.OptionsScreen.orig_method_50 orig, OptionsScreen screen_self, float timeDelta)
	{
		if (false) //(CampaignLoader.CurrentCampaignIsDisjoint())
		{
			var screen_dyn = new DynamicData(screen_self);
			var currentStoryPanel = screen_dyn.Get<StoryPanel>("field_2680");
			var stringArray = new DynamicData(currentStoryPanel).Get<string[]>("field_4093");
			if (!stringArray.Any(x => x.Contains("Talma") || x.Contains("Jerin") || x.Contains("Serena")))
			{
				var class264 = new class_264("options-Disjoint");
				class264.field_2090 = optionsID;
				screen_dyn.Set("field_2680", new StoryPanel((Maybe<class_264>)class264, false));
			}
		}
		orig(screen_self, timeDelta);
	}

	static Tuple<int, LocString>[] SigmarStoryUnlocks;

	public static void CreateSigmarStoryUnlocks(List<int> unlocks)
	{
		SigmarStoryUnlocks = new Tuple<int, LocString>[unlocks.Count + 1];

		for (int i = 0; i < unlocks.Count; i++)
		{
			int k = unlocks[i];
			string msg = "Win " + k + (k == 1 ? " game" : " games");
			SigmarStoryUnlocks[i] = Tuple.Create(k, class_134.method_253(msg, string.Empty));
		}
		SigmarStoryUnlocks[unlocks.Count] = Tuple.Create(int.MaxValue, LocString.field_2597);
	}

	public static void customStorypanelUnlocks(On.StoryPanel.orig_method_2172 orig, StoryPanel panel_self, float timeDelta, Vector2 pos, int index, Tuple<int, LocString>[] tuple)
	{
		if (CampaignLoader.CurrentCampaignIsDisjoint() && tuple.Length == 2 && tuple[0].Item2 == class_134.method_253("Complete the prologue", string.Empty))
		{
            if (false){
			// then we're doing the options code while in the Disjoint campaign
			// hijack the inputs so we draw it our way
			bool flag = GameLogic.field_2434.field_2451.method_573(optionsUnlock);
			index = flag ? 1 : 0;
			tuple = new Tuple<int, LocString>[2]
			{
				Tuple.Create(1, class_134.method_253("Complete the prologue", string.Empty)),
				Tuple.Create(int.MaxValue, LocString.field_2597)
			};
            }
		}
		else if (CampaignLoader.CurrentCampaignIsDisjoint() && tuple.Length == 7 && tuple[0].Item2 == class_134.method_253("Win 1 game", string.Empty))
		{
			// then we're doing the solitaire code while in the Disjoint campaign
			// hijack the inputs so we draw it our way
			tuple = SigmarStoryUnlocks;
		}

		orig(panel_self, timeDelta, pos, index, tuple);
	}
}