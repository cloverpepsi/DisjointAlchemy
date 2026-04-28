﻿//using Mono.Cecil.Cil;
//using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
//using Quintessential.Serialization;
//using Quintessential.Settings;
//using SDL2;
using System;
using System.IO;
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
//using Texture = class_256;
//using Song = class_186;
//using Tip = class_215;
using Font = class_1;

// all this code is taken from True Animismus Campaign and pared down, which I believe is taken from Reductive Metallurgy and pared down.

public class SigmarGardenPatcher
{
    private static IDetour hook_SolitaireScreen_method_1889;
    private static IDetour hook_SolitaireScreen_method_1890;
    private static IDetour hook_SolitaireScreen_method_1893;
    private static IDetour hook_SolitaireScreen_method_1894;

    public const string solitaireID = "Disjoint-solitaire";

    private static int sigmarWins_Disjoint = 0;
    public static AtomType nullAtom;
    public static SolitaireState solitaireState_Disjoint;

    private static bool isQuintessenceSigmarGarden(SolitaireScreen screen) => new DynamicData(screen).Get<bool>("field_3874");
    private static bool currentCampaignIsDisjoint(SolitaireScreen screen) => CampaignLoader.CurrentCampaignIsDisjoint() && !isQuintessenceSigmarGarden(screen);
    private static void setSigmarWins_Disjoint() => GameLogic.field_2434.field_2451.field_1929.method_858("Disjoint-SigmarWins", sigmarWins_Disjoint.method_453());
    private static void getSigmarWins_Disjoint() { sigmarWins_Disjoint = GameLogic.field_2434.field_2451.field_1929.method_862<int>(new delegate_384<int>(int.TryParse), "Disjoint-SigmarWins").method_1090(0); }
    public static AtomType getAtomType(int i)
    {
        return new AtomType[17]
        {
            SigmarGardenPatcher.nullAtom, // 00 - filler
			class_175.field_1681, // 01 - lead
			class_175.field_1683, // 02 - tin
			class_175.field_1684, // 03 - iron
			class_175.field_1682, // 04 - copper
			class_175.field_1685, // 05 - silver
			class_175.field_1686, // 06 - gold
			class_175.field_1680, // 07 - quicksilver
			class_175.field_1687, // 08 - vitae
			class_175.field_1688, // 09 - mors
			class_175.field_1675, // 10 - salt
			class_175.field_1676, // 11 - air
			class_175.field_1679, // 12 - water
			class_175.field_1678, // 13 - fire
			class_175.field_1677, // 14 - earth
			class_175.field_1689, // 15 - repeat
			class_175.field_1690, // 16 - quintessence
			// TrueAnimismus.ModdedAtoms.RedVitae, // 17 - red vitae
			// TrueAnimismus.ModdedAtoms.TrueVitae,// 18 - true vitae
			// TrueAnimismus.ModdedAtoms.GreyMors, // 19 - grey mors
			// TrueAnimismus.ModdedAtoms.TrueMors, // 20 - true mors
		}[i];
    }
    public static void PostLoad()
    {
        getSigmarWins_Disjoint();
        On.CampaignItem.method_825 += DetermineIfCampaignItemIsCompleted;
        //On.SolitaireGameState.class_301.method_1888 += DetermineIfMatchIsValid;
        On.SolitaireGameState.method_1885 += DetermineIfSolitaireGameWasWon;
        On.SolitaireScreen.method_50 += SolitaireScreen_Method_50;
        On.class_198.method_537 += getRandomizedSolitaireBoard;

        nullAtom = new AtomType()
        {
            field_2283 = (byte)0,
            field_2284 = (string)class_134.method_254("Null"),
            field_2285 = class_134.method_253("Elemental Null", string.Empty),
            field_2286 = class_134.method_253("Null", string.Empty),
            field_2287 = class_238.field_1989.field_81.field_598,
            field_2288 = class_238.field_1989.field_81.field_599,
            field_2290 = new class_106()
            {
                field_994 = class_238.field_1989.field_81.field_596,
                field_995 = class_238.field_1989.field_81.field_597
            }
        };

        //On.SolitaireScreen.method_47 += OnSolitaireScreen_Method_47;
        hook_SolitaireScreen_method_1889 = new Hook(DisjointAlchemy.PrivateMethod<SolitaireScreen>("method_1889"), OnSolitaireScreen_Method_1889);
        hook_SolitaireScreen_method_1890 = new Hook(DisjointAlchemy.PrivateMethod<SolitaireScreen>("method_1890"), OnSolitaireScreen_Method_1890);
        hook_SolitaireScreen_method_1893 = new Hook(DisjointAlchemy.PrivateMethod<SolitaireScreen>("method_1893"), OnSolitaireScreen_Method_1893);
        hook_SolitaireScreen_method_1894 = new Hook(DisjointAlchemy.PrivateMethod<SolitaireScreen>("method_1894"), OnSolitaireScreen_Method_1894);
    }

    private delegate SolitaireState orig_SolitaireScreen_method_1889(SolitaireScreen self);
    private delegate void orig_SolitaireScreen_method_47(SolitaireScreen self, bool param_5434);
    private delegate void orig_SolitaireScreen_method_1890(SolitaireScreen self, SolitaireState param_5433);
    private delegate bool orig_SolitaireScreen_method_1893(SolitaireScreen self);
    private delegate bool orig_SolitaireScreen_method_1894(SolitaireScreen self);

    private delegate void orig_SolitaireScreen_method_1905(SolitaireScreen self, SolitaireState.struct_124 param_5446);

    private static SolitaireState OnSolitaireScreen_Method_1889(orig_SolitaireScreen_method_1889 orig, SolitaireScreen screen_self)
    {
        if (currentCampaignIsDisjoint(screen_self)) return solitaireState_Disjoint;
        return orig(screen_self);
    }
    private static void OnSolitaireScreen_Method_1890(orig_SolitaireScreen_method_1890 orig, SolitaireScreen screen_self, SolitaireState param_5433)
    {
        if (currentCampaignIsDisjoint(screen_self))
        {
            solitaireState_Disjoint = param_5433;
            return;
        }
        orig(screen_self, param_5433);
    }
    private static bool OnSolitaireScreen_Method_1893(orig_SolitaireScreen_method_1894 orig, SolitaireScreen screen_self)
    {
        // used to show the rules button
        if (currentCampaignIsDisjoint(screen_self))
        {
            var state = (SolitaireState)DisjointAlchemy.PrivateMethod<SolitaireScreen>("method_1889").Invoke(screen_self, new object[] { });
            return new DynamicData(screen_self).Get<StoryPanel>("field_3872").method_2170() >= 8;
        }
        return orig(screen_self);
    }
    private static bool OnSolitaireScreen_Method_1894(orig_SolitaireScreen_method_1894 orig, SolitaireScreen screen_self)
    {
        // used to enable the NEW GAME button
        if (currentCampaignIsDisjoint(screen_self))
        {
            var state = (SolitaireState)DisjointAlchemy.PrivateMethod<SolitaireScreen>("method_1889").Invoke(screen_self, new object[] { });
            return new DynamicData(screen_self).Get<StoryPanel>("field_3872").method_2170() >= 1 && !state.method_1922();
        }
        return orig(screen_self);
    }

    public static void Unload()
    {
        hook_SolitaireScreen_method_1889.Dispose();
        hook_SolitaireScreen_method_1890.Dispose();
        hook_SolitaireScreen_method_1893.Dispose();
        hook_SolitaireScreen_method_1894.Dispose();
    }

    public static bool DetermineIfCampaignItemIsCompleted(On.CampaignItem.orig_method_825 orig, CampaignItem item_self)
    {
        bool ret = orig(item_self);
        if (CampaignLoader.CurrentCampaignIsDisjoint())
            ret = ret || (item_self.field_2324 == CampaignLoader.typeSolitaire && sigmarWins_Disjoint > 0);
        return ret;
    }

    public static bool DetermineIfSolitaireGameWasWon(On.SolitaireGameState.orig_method_1885 orig, SolitaireGameState state_self)
    {
        bool ret = orig(state_self);
        AtomType quintessence = class_175.field_1690;
        if (ret && CampaignLoader.CurrentCampaignIsDisjoint() && !state_self.field_3864.ContainsValue(quintessence)) sigmarWins_Disjoint++;
        setSigmarWins_Disjoint();
        return ret;
    }

    public static void SolitaireScreen_Method_50(On.SolitaireScreen.orig_method_50 orig, SolitaireScreen screen_self, float timeDelta)
    {
        if (currentCampaignIsDisjoint(screen_self))
        {
            var screen_dyn = new DynamicData(screen_self);
            screen_dyn.Set("field_3871", sigmarWins_Disjoint);
            var currentStoryPanel = screen_dyn.Get<StoryPanel>("field_3872");
            var stringArray = new DynamicData(currentStoryPanel).Get<string[]>("field_4093");
            if (!stringArray.Any(x => x.Contains("Jerin") || x.Contains("Serena")))
            {
                var class264 = new class_264("solitaire-Disjoint");
                class264.field_2090 = solitaireID;
                screen_dyn.Set("field_3872", new StoryPanel((Maybe<class_264>)class264, true));
            }
        }

        orig(screen_self, timeDelta);

        if (!currentCampaignIsDisjoint(screen_self)) return;

        //Some 'draw metals remaining' code from RMC solitaire; I don't know how much of this is safe to yank out, so I just delabled the actual draw-numbers-pls line.
        SolitaireScreen.class_412 class412 = new SolitaireScreen.class_412();
        class412.field_3883 = screen_self;
        class412.field_3886 = timeDelta;
        if (GameLogic.field_2434.method_938() is class_16) return;

        Vector2 vector2_1 = new Vector2(1516f, 922f);
        //class412.field_3884 = (class_115.field_1433 / 2 - vector2_1 / 2 + new Vector2(-2f, -11f)).Rounded();


        int Method_1901(AtomType atomType, Vector2 pos)
        {
            SolitaireScreen.class_413 class413 = new SolitaireScreen.class_413();
            class413.field_3889 = atomType;
            class413.field_3888 = 0;
            class413.field_3890 = 0;

            var class301 = SolitaireScreen.class_301.field_2343;
            void Method_1907(SolitaireState.struct_123 param_5448) => DisjointAlchemy.PrivateMethod<SolitaireScreen.class_413>("method_1907").Invoke(class413, new object[] { param_5448 });
            void Method_1909(SolitaireState.struct_124 param_5449) => DisjointAlchemy.PrivateMethod<SolitaireScreen.class_413>("method_1909").Invoke(class413, new object[] { param_5449 });
            void Method_1911(SolitaireState.WaitingForNewGameFields param_5451) => DisjointAlchemy.PrivateMethod<SolitaireScreen.class_301>("method_1907").Invoke(class301, new object[] { param_5451 });
            void Method_1914(SolitaireState.WonLastGameFields param_5452) => DisjointAlchemy.PrivateMethod<SolitaireScreen.class_301>("method_1907").Invoke(class301, new object[] { param_5452 });
            var state = (SolitaireState)DisjointAlchemy.PrivateMethod<SolitaireScreen>("method_1889").Invoke(class412.field_3883, new object[] { });
            state.method_1933(SolitaireScreen.class_301.field_3893 ?? (SolitaireScreen.class_301.field_3893 = new Action<SolitaireState.WaitingForNewGameFields>(Method_1911)), new Action<SolitaireState.struct_123>(Method_1907), new Action<SolitaireState.struct_124>(Method_1909), SolitaireScreen.class_301.field_3896 ?? (SolitaireScreen.class_301.field_3896 = new Action<SolitaireState.WonLastGameFields>(Method_1914)));

            // draw the number of atoms remaining for that atomType
            int count = class413.field_3888;
            Color color = count == 0 ? class_181.field_1718.WithAlpha(0.2f) : class_181.field_1718;
            if (count % 2 == 1) color = class_181.field_1720;
            string total = count.ToString();
            Font crimson_10_5 = class_238.field_1990.field_2141;
            pos += new Vector2(19f, 12f);
            class_135.method_290(total, pos, crimson_10_5, color, (enum_0)1, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
            return count;
        }
    }

	public static SolitaireGameState getRandomizedSolitaireBoard(On.class_198.orig_method_537 orig, bool quintessenceSigmar) { return orig(quintessenceSigmar); }

}