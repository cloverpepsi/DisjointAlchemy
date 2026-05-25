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
using Texture = class_256;
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

    static Texture sigmar_foreground = class_235.method_615("textures/prod_solitaire/overlay");

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
		}[i];
    }
    public static void PostLoad()
    {
        getSigmarWins_Disjoint();
        On.CampaignItem.method_825 += DetermineIfCampaignItemIsCompleted;
        On.SolitaireGameState.method_1884 += DetermineIfMatchIsValid;
        On.SolitaireGameState.method_1885 += DetermineIfSolitaireGameWasWon;
        On.SolitaireScreen.method_50 += SolitaireScreen_Method_50;
        On.class_16.method_50 += SolitaireRulesScreen_Method_50;
        On.class_198.method_537 += getRandomizedSolitaireBoard;
        On.class_135.method_272 += hotswapBoardTexture;

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

	private static void hotswapBoardTexture(On.class_135.orig_method_272 orig, Texture texture, Vector2 position)
	{
		if (CampaignLoader.CurrentCampaignIsDisjoint())
		{
			//if (texture == class_238.field_1989.field_98.field_545) { texture = sigmar_background; }
		}
		orig(texture, position);
		return;
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

    public static bool DetermineIfMatchIsValid(On.SolitaireGameState.orig_method_1884 orig, SolitaireGameState state_self, HexIndex atomA, HexIndex atomB)
    {
        if (CampaignLoader.CurrentCampaignIsDisjoint())
        {
            if (!HexesAreCompatible(atomA, atomB)) {return false;}
        }
        return orig(state_self, atomA, atomB);
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

        Vector2 offset = (class_115.field_1433 / 2 - new Vector2(1516f, 922f) / 2 + new Vector2(-2f, -11f)).Rounded();

        class_135.method_272(sigmar_foreground, offset + new Vector2(597f, 92f));

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

    static bool HexesAreCompatible(HexIndex hexA, HexIndex hexB)
    {
        
        HexIndex leftHex = hexA.Q + hexA.R < hexB.Q + hexB.R ? hexA : hexB;
        HexIndex rightHex = leftHex == hexB ? hexA : hexB;

        if ((leftHex.Q + leftHex.R) < 5 && (rightHex.Q + rightHex.R) > 5 && leftHex.R < 0 && rightHex.Q > 5) return false;

        return true;
    }

    public static SolitaireGameState getRandomizedSolitaireBoard(On.class_198.orig_method_537 orig, bool quintessenceSigmar)
    {
        int RandomInt(int max) => class_269.field_2103.method_299(0, max);

        if (!CampaignLoader.CurrentCampaignIsDisjoint() || quintessenceSigmar) return orig(quintessenceSigmar);

        // try to find solitaire-bitboards.dat
        string subpath = "/Content/solitaire-bitboards.dat";
        string filepath;
        if (!DisjointAlchemy.findModMetaFilepath("DisjointAlchemy", out filepath) || !File.Exists(filepath + subpath))
        {
            Logger.Log("[TrueAnimismusCampaign] Could not find 'solitaire-bitboards.dat' in the folder '" + filepath + "/Content/'");
            throw new Exception("getRandomizedSolitaireBoard: Solitaire data is missing.");
        }

        // first, pick a bitboard and generate a board template
        HexIndex center = new HexIndex(2,3);
        List<HexIndex> marbleHexes = new();
        using (BinaryReader binaryReader = new BinaryReader(new FileStream(filepath + subpath, FileMode.Open, FileAccess.Read)))
        {

            string bitString = "00000000110000000011110000000111100110011110011110010000111000000011110010000110011110000000111100000011110000000110000000000000";

            const int bytesPerBitboard = 16;
            int bitboardCount = binaryReader.ReadInt32();

            int boardID = RandomInt(1);
            binaryReader.BaseStream.Seek(boardID * bytesPerBitboard, SeekOrigin.Current);

            bool mirrorBoard = false;
            HexRotation rotation = new HexRotation(RandomInt(3)*2);

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int num = i * 8 + j;
                    if (bitString[num] == '1')
                    {
                        // add hex
                        int q = num / 11;
                        int r = (num % 11) - 5;
                        if (mirrorBoard) { q += r; r = -r; }
                        marbleHexes.Add(new HexIndex(q, r));
                    }
                }
            }
        }

        // "solve" the board template by generating a move history
        // for convenience, we will assume only one Gold marble exists, and that it is always the center of the board
        // additionally, we assume there are no Quintessence marbles, so every match is always a *pair* of marbles
        bool HexIsChoosable(HexIndex hex, List<HexIndex> marbleHexes)
        {
            // based on method_1881
            int val2 = 0;
            int val1 = 0;
            for (int index = 0; index < 2; ++index)
            {
                foreach (HexIndex adjacentOffset in HexIndex.AdjacentOffsets)
                {
                    if (marbleHexes.Contains(hex + adjacentOffset))
                    {
                        val2 = 0;
                    }
                    else
                    {
                        val2++;
                        val1 = Math.Max(val1, val2);
                    }
                }
            }
            return val1 >= 3;
        };

        bool boardGenerated = false;
        List<Tuple<HexIndex, HexIndex>> moveHistory = new();

        while (!boardGenerated)
        {
            List<HexIndex> temporaryMarbles = [.. marbleHexes];
            moveHistory = new();
            while (temporaryMarbles.Count > 0)
            {
                // find all marbles that could be chosen for the next move
                List<HexIndex> choosableMarbles = new();
                foreach (var hex in temporaryMarbles.Where(x => HexIsChoosable(x, temporaryMarbles) && (x != center)))
                {
                    choosableMarbles.Add(hex);
                }
                // choose the next move
                if (choosableMarbles.Count >= 2)
                {
                    HexIndex marbleA = new HexIndex(100,100), marbleB = new HexIndex(100,100);
                    List<HexIndex> MarbleAs = [.. choosableMarbles];
                    
                    while (MarbleAs.Count >= 1) {

                        // choose a random marble A to check
                        marbleA = MarbleAs[RandomInt(MarbleAs.Count)];
                        MarbleAs.Remove(marbleA);

                        // iterate over all possible chooseable other marbles to make sure one is choosable
                        List<HexIndex> MarbleBs = new();
                        foreach (var hex in choosableMarbles)
                        {
                            if (HexesAreCompatible(marbleA, hex)) {MarbleBs.Add(hex);}
                        }
                        MarbleBs.Remove(marbleA);

                        if (MarbleBs.Count > 0) 
                        {
                            marbleB = MarbleBs[RandomInt(MarbleBs.Count)];
                            break;
                        }
                    }

                    if (marbleB != new HexIndex(100,100)) {
                        moveHistory.Add(Tuple.Create(marbleA, marbleB));
                        temporaryMarbles.Remove(marbleA);
                        temporaryMarbles.Remove(marbleB);
                    }
                    else
                    {
                        break;
                    }
                }
                else if (HexIsChoosable(center, temporaryMarbles))
                {
                    // only option is to choose Gold as our next move
                    moveHistory.Add(Tuple.Create(center, center));
                    temporaryMarbles.Remove(center);
                    boardGenerated = true;
                }
                else
                {
                    Logger.Log("[DisjointAlchemy] Encountered a board-template state where no move is possible:");
                    foreach (var hex in temporaryMarbles)
                    {
                        Logger.Log("    " + hex.Q + "," + hex.R);
                    }
                    //throw new Exception("GetRandomizedSolitaireBoard: Impossible unsolvable state reached.");
                }
            }
        }

        // reverse the list, so moveHistory[0] is the LAST move made to solve the board
        moveHistory.Reverse();

        // generate "marble bags" that store the moves to be made
        List<Tuple<AtomType, AtomType>> saltlikeBag = new();
        List<Tuple<AtomType, AtomType>> metalBag = new();

        // put animismus matches in the saltlikeBag. 
        for (int i = 0; i < 3; i++)
        {
            saltlikeBag.Add(Tuple.Create(getAtomType(8), getAtomType(9))); // vitae and mors
        }
		int[] cardinals = new int[5] { 4, 6, 6, 6, 6 }; // salt, air, water, fire, earth

		// put salt matches in the saltlikeBag
		while (cardinals[0] > 0)
		{
			cardinals[0] -= 2;
			int match = RandomInt(5);
			if (match == 0)
			{
				saltlikeBag.Add(Tuple.Create(getAtomType(10), getAtomType(10)));
			}
			else
			{	
				cardinals[match] -= 2;
				saltlikeBag.Add(Tuple.Create(getAtomType(10), getAtomType(10 + match)));
				saltlikeBag.Add(Tuple.Create(getAtomType(10), getAtomType(10 + match)));
			}
		}

		// put the remaining cardinal matches in the saltlikeBag
		for (int i = 1; i < 5; i++)
		{
			while (cardinals[i] > 0)
			{
				cardinals[i] -= 2;
				saltlikeBag.Add(Tuple.Create(getAtomType(10 + i), getAtomType(10 + i)));
			}
		}

        //One of each metal on the board
        int[] metals = new int[6] { 1, 1, 1, 1, 1, 1 };

        // add the non-Gold metals into the metalBag, from Silver to Lead
        // we need to insert them in order, since we must solve them in order!
        for (int i = 5; i > 0; i--)
        {
            // generate temporary bag of marbles containing a specific tier of metal
            List<Tuple<AtomType, AtomType>> tempBag = new() { };
            while (metals[i] > 0)
            {
                metals[i]--;
                tempBag.Add(Tuple.Create(getAtomType(i), getAtomType(7)));
            }

            // randomly pour the tempBag into the metalBag. All of this is redundant because I'm repurposing RMC code
            while (tempBag.Count > 0)
            {
                var pick = tempBag[RandomInt(tempBag.Count)];
                metalBag.Add(pick);
                tempBag.Remove(pick);
            }
        }

        // "unsolve" the board by using the move history in reverse to place marbles
        SolitaireGameState solitaireGameState = new SolitaireGameState();

        bool placedGold = false;
        for (int m = 0; m < moveHistory.Count; m++)
        {
            var hexes = moveHistory[m];

            if (hexes.Item1 == hexes.Item2)
            {
                // the Gold match!
                solitaireGameState.field_3864.Add(hexes.Item1, getAtomType(6));
                placedGold = true;
                continue;
            }

            if (saltlikeBag.Count + metalBag.Count == 0) { break; }
            //Sometimes the code tries to remove marbles when the bags are both empty.
            //I don't know why.
            //If it tries to do this, stop generating the board.
            //This leaves it asymmetrical, which is a problem to be fixed in the future if future me still cares.

            // non-Gold matches happen here
            int pick = RandomInt(saltlikeBag.Count + metalBag.Count);

            if (!placedGold)
            {
                pick = RandomInt(saltlikeBag.Count);
            }

            Tuple<AtomType, AtomType> match;
            if (pick < saltlikeBag.Count)
            {
                match = saltlikeBag[pick];
                saltlikeBag.Remove(match);
            }
            else
            {
                match = metalBag[0];
                metalBag.Remove(match);
            }
            solitaireGameState.field_3864.Add(hexes.Item1, match.Item1);
            solitaireGameState.field_3864.Add(hexes.Item2, match.Item2);
        }

        // tada! randomized board
        return solitaireGameState;
    }

    
	public static void SolitaireRulesScreen_Method_50(On.class_16.orig_method_50 orig, class_16 screen_self, float timeDelta)
	{
		if (CampaignLoader.CurrentCampaignIsDisjoint())
		{
			var screen_dyn = new DynamicData(screen_self);
			string rule = "Select a free marble, and then pick a matching marble to remove them both from the board.\n\nMarbles in different chambers can only be matched if a conduit is connecting the two chambers.";
			screen_dyn.Set("field_64", class_134.method_253(rule, string.Empty));
		}

		orig(screen_self, timeDelta);
	}

}