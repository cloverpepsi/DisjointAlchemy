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
using System.Diagnostics.Eventing.Reader;
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
	static Texture ch5_locked, ch5_unlocked, ch5_hover, ch6_locked, ch6_unlocked, ch6_hover;
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
		string path = "textures/story/";
		ch5_locked = class_235.method_615(path + "chapter_locked_5");
		ch5_unlocked = class_235.method_615(path + "chapter_unlocked_5");
		ch5_hover = class_235.method_615(path + "chapter_hover_5");
		ch6_locked = class_235.method_615(path + "chapter_locked_6");
		ch6_unlocked = class_235.method_615(path + "chapter_unlocked_6");
		ch6_hover = class_235.method_615(path + "chapter_hover_6");
	}


	public static void Load()
	{
		On.class_172.method_480 += new On.class_172.hook_method_480(AddCharactersToDictionary);
	}

	public static void PostLoad()
	{
		IL.StoryPanel.method_2175 += skipDrawingTheReturnButton;
		On.class_135.method_272 += hotswapChapterSelectTextures;
		On.OptionsScreen.method_50 += hotswapOptionsStorypanel;
		On.StoryPanel.method_2172 += customStorypanelUnlocks;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////
	// hooking

	private static void AddCharactersToDictionary(On.class_172.orig_method_480 orig)
	{
		orig();
		Logger.Log("[DisjointAlchemy] Adding vignette actors.");
		
		foreach (CharacterModelDisjoint character in DisjointAlchemy.AdvancedContent.Characters)
		{
			class_172.field_1670[character.ID] = character.FromModel();
		}
	}

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
			if (JournalLoader.journal_puzzles.Any(x => x.ID == storyPanelID)) return false;

			return isOptionsScreen;
		});
	}
	private static void hotswapChapterSelectTextures(On.class_135.orig_method_272 orig, Texture texture, Vector2 position)
	{
		if (CampaignLoader.CurrentCampaignIsDisjoint())
		{
			if (texture == class_238.field_1989.field_96.field_853) { texture = ch5_locked; }
			else if (texture == class_238.field_1989.field_96.field_854) { texture = ch6_locked; }
			else if (texture == class_238.field_1989.field_96.field_860) { texture = ch5_unlocked; }
			else if (texture == class_238.field_1989.field_96.field_861) { texture = ch6_unlocked; }
			else if (texture == class_238.field_1989.field_96.field_846) { texture = ch5_hover; }
			else if (texture == class_238.field_1989.field_96.field_847) { texture = ch6_hover; }
		}
		orig(texture, position);
		return;
	}

	public static void hotswapOptionsStorypanel(On.OptionsScreen.orig_method_50 orig, OptionsScreen screen_self, float timeDelta)
	{
		if (CampaignLoader.CurrentCampaignIsDisjoint())
		{
			var screen_dyn = new DynamicData(screen_self);
			var currentStoryPanel = screen_dyn.Get<StoryPanel>("field_2680");
			var stringArray = new DynamicData(currentStoryPanel).Get<string[]>("field_4093");
			if (!stringArray.Any(x => x.Contains("Serena") || x.Contains("Jerin") || x.Contains("Talma")))
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

		bool currentIsDisjoint = (CampaignLoader.CurrentCampaignIsDisjoint() && !DisjointAlchemy.CurrentlyInJournal()) || (DisjointAlchemy.CurrentlyInJournal() && JournalScreen.CurrentJournalName() == "The Journal of Disjoint Alchemy");		

		if (CampaignLoader.CurrentCampaignIsDisjoint() && tuple.Length == 2 && tuple[0].Item2 == class_134.method_253("Complete the prologue", string.Empty))
		{
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
		else if (currentIsDisjoint && tuple.Length == 7 && tuple[0].Item2 == class_134.method_253("Win 1 game", string.Empty))
		{
			// then we're doing the solitaire code while in the Disjoint campaign
			// hijack the inputs so we draw it our way
			tuple = SigmarStoryUnlocks;
		}

		orig(panel_self, timeDelta, pos, index, tuple);
	}
}