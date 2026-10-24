﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

//using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
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
using System.Reflection;
using DisjointAlchemy;

namespace DisjointAlchemy {

// all of this code has been through the fucking ringer. most of it is from rmc indirectly though

public class Solitaire
{
    public const string solitaireID = "Disjoint-solitaire";

    public static Texture sigmar_foreground = AssetLoaderHelper.LoadTexture("textures/disjoint/prod_solitaire/overlay");
    public static Texture sigmarSprite = AssetLoaderHelper.LoadTexture("textures/disjoint/prod_solitaire/solitaire");
    public static Texture sigmarHoverSprite = AssetLoaderHelper.LoadTexture("textures/disjoint/prod_solitaire/solitaire_hover");

    public static bool sigmarEmpty = false;

    public static int sigmarWins_Disjoint = 0;
    public static SolitaireState solitaireState_Disjoint;

    public static bool currentCampaignIsDisjoint() {
        return (DisjointAlchemy.CurrentCampaignIsDisjoint() && !DisjointAlchemy.CurrentlyInJournal()) || (DisjointAlchemy.CurrentlyInJournal() && JournalScreen.CurrentJournalTitle().ToString() == "The Journal of Disjoint Alchemy");
    }

    public static void setSigmarWins_Disjoint() => GameLogic.instance.saveData.configFile.Set("Disjoint-SigmarWins", Utils.ToInvariant(sigmarWins_Disjoint));
    public static void getSigmarWins_Disjoint() { sigmarWins_Disjoint = GameLogic.instance.saveData.configFile.Get<int>(new ConfigValueMap<int>(int.TryParse), "Disjoint-SigmarWins").GetOrDefault(0); }
    public static AtomType getAtomType(int i)
    {
        return new AtomType[17]
        {
            null, // 00 - filler
			AtomTypes.lead, // 01 - lead
			AtomTypes.tin, // 02 - tin
			AtomTypes.iron, // 03 - iron
			AtomTypes.copper, // 04 - copper
			AtomTypes.silver, // 05 - silver
			AtomTypes.gold, // 06 - gold
			AtomTypes.quicksilver, // 07 - quicksilver
			AtomTypes.vitae, // 08 - vitae
			AtomTypes.mors, // 09 - mors
			AtomTypes.salt, // 10 - salt
			AtomTypes.air, // 11 - air
			AtomTypes.water, // 12 - water
			AtomTypes.fire, // 13 - fire
			AtomTypes.earth, // 14 - earth
			AtomTypes.repeat, // 15 - repeat
			AtomTypes.quintessence, // 16 - quintessence
		}[i];
    }
    public static void PostLoad()
    {
        getSigmarWins_Disjoint();
    }

    public static bool HexesAreCompatible(HexIndex hexA, HexIndex hexB)
    {
        HexIndex leftHex = hexA.Q + hexA.R < hexB.Q + hexB.R ? hexA : hexB;
        HexIndex rightHex = leftHex == hexB ? hexA : hexB;

        if ((leftHex.Q + leftHex.R) < 5 && (rightHex.Q + rightHex.R) > 5 && leftHex.R < 0 && rightHex.Q > 5) return false;

        return true;
    }

    public static SolitaireGameState getRandomizedSolitaireBoard()
    {
        int RandomInt(int max) => PlatformBridge.rng.GetInt(0, max);
        // try to find solitaire-bitboards.dat
        string subpath = "/Content/solitaire-bitboards.txt";
        if (DisjointAlchemy.FilePath == null || !File.Exists(DisjointAlchemy.FilePath + subpath))
        {
            Logger.Log("[DisjointAlchemy] Could not find 'solitaire-bitboards.txt' in the folder '" + DisjointAlchemy.FilePath + "/Content/'");
            throw new Exception("getRandomizedSolitaireBoard: Solitaire data is missing.");
        }

        // first, pick a bitboard and generate a board template
        HexIndex center = new HexIndex(2,3);
        List<HexIndex> marbleHexes = new();

        string[] bitStrings = File.ReadAllLines(DisjointAlchemy.FilePath + subpath);

        //const int bytesPerBitboard = 16;

        int boardID = RandomInt(bitStrings.Length);
        string bitString = bitStrings[boardID];
        //binaryReader.BaseStream.Seek(boardID * bytesPerBitboard, SeekOrigin.Current);

        bool mirrorBoard = false;
        //HexRotation rotation = new HexRotation(RandomInt(3)*2);

        if (sigmarEmpty) {bitString = "00000000111000000011110000000111100110011110011110110001111100000011110110000111011110000001111100000011110000000111000000000000";}

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
            List<HexIndex> temporaryMarbles = marbleHexes.Select(p => new HexIndex(p.Q, p.R)).ToList();
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
                    List<HexIndex> MarbleAs = choosableMarbles.Select(p => new HexIndex(p.Q, p.R)).ToList();;
                    
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
		int[] cardinals = sigmarEmpty ? new int[5] { 4, 8, 8, 8, 8 } : new int[5] { 4, 6, 6, 6, 6 }; // salt, air, water, fire, earth

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
                solitaireGameState.atoms.Add(hexes.Item1, getAtomType(6));
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
            solitaireGameState.atoms.Add(hexes.Item1, match.Item1);
            solitaireGameState.atoms.Add(hexes.Item2, match.Item2);
        }

        // tada! randomized board
        return solitaireGameState;
    }
}
}

public class patch_SolitaireScreen : SolitaireScreen {
    public patch_SolitaireScreen(SolitaireType solitaireType) : base(solitaireType) {}

    [MonoModIgnore]
    private StoryPanel storyPanel;

    private extern SolitaireState orig_GetState();
    private SolitaireState GetState()
    {
        if (Solitaire.currentCampaignIsDisjoint()) return Solitaire.solitaireState_Disjoint;
        return orig_GetState();
    }

    private extern void orig_SetState(SolitaireState state);
    private void SetState(SolitaireState state)
    {
        if (Solitaire.currentCampaignIsDisjoint())
        {
            Solitaire.solitaireState_Disjoint = state;
            return;
        }
        orig_SetState(state);
    }

    private extern bool orig_CanGenerateNewGame();
    private bool CanGenerateNewGame()
    {
        // used to enable the NEW GAME button
        if (Solitaire.currentCampaignIsDisjoint())
        {
            return storyPanel.GetCompletion() >= 1 && !GetState().IsRevealNewGame();
        }
        return orig_CanGenerateNewGame();
    }


    private extern void orig_RenderFrame(float timeDelta);
    public void RenderFrame(float timeDelta)
    {
        if (Solitaire.currentCampaignIsDisjoint())
        {
            var screen_dyn = new DynamicData((SolitaireScreen)(object)this);
            screen_dyn.Set("solitaireStat", Solitaire.sigmarWins_Disjoint);
            var currentStoryPanel = screen_dyn.Get<StoryPanel>("storyPanel");
            var stringArray = new DynamicData(currentStoryPanel).Get<string[]>("parsedText");
            if (!stringArray.Any(x => x.Contains("Jerin") || x.Contains("Serena")))
            {
                var myVignette = new LocalizedVignette("solitaire-Disjoint");
                myVignette.vignettePath = Solitaire.solitaireID;
                screen_dyn.Set("storyPanel", new StoryPanel((Maybe<LocalizedVignette>)myVignette, true));
            }
        }

        orig_RenderFrame(timeDelta);

        if (!Solitaire.currentCampaignIsDisjoint()) return;

        // Render production overlay
        Vector2 offset = (InputManager.screenSize / 2 - new Vector2(1516f, 922f) / 2 + new Vector2(-2f, -11f)).Rounded();
        TextureRenderer.Render(Solitaire.sigmar_foreground, offset + new Vector2(597f, 92f));
    }

}

public class patch_SolitaireFileManager : SolitaireScreen {
    public patch_SolitaireFileManager(SolitaireType solitaireType) : base(solitaireType) {}

    public static extern SolitaireGameState orig_GetRandomFromFile(SolitaireType type);
    public static SolitaireGameState GetRandomFromFile(SolitaireType type) {
        return Solitaire.currentCampaignIsDisjoint() ? Solitaire.getRandomizedSolitaireBoard() : orig_GetRandomFromFile(type);
    }
}

// remember to patch in a change to the help screen, maybe

public class patch_SolitaireGameState : SolitaireGameState {


    public extern bool orig_IsEmpty();
    public bool IsEmpty()
    {
        bool ret = orig_IsEmpty();
        if (ret && Solitaire.currentCampaignIsDisjoint()) Solitaire.sigmarWins_Disjoint++;
        Solitaire.setSigmarWins_Disjoint();
        return ret;
    }

    public extern bool orig_IsValidPair(HexIndex atomA, HexIndex atomB);
    public bool IsValidPair(HexIndex atomA, HexIndex atomB)
    {
        if (Solitaire.currentCampaignIsDisjoint())
        {
            if (!Solitaire.HexesAreCompatible(atomA, atomB)) {return false;}
        }
        return orig_IsValidPair(atomA, atomB);
    }

}

public class patch_CampaignItem : CampaignItem {
    public patch_CampaignItem(string itemId, LocString itemName, CampaignItemType itemType, Maybe<Puzzle> puzzle, UnlockRequirement requirement, MusicTrack musicTrack, Sound fanfare, Campaign campaign) : base(itemId, itemName, itemType, puzzle, requirement, musicTrack, fanfare, campaign) {}

    [MonoModIgnore]
    public CampaignItemType itemType;

    public extern bool orig_IsComplete();
    public bool IsComplete()
    {
        if (Solitaire.currentCampaignIsDisjoint() && itemType == CampaignItemType.Solitaire)
            return Solitaire.sigmarWins_Disjoint > 0;
        return orig_IsComplete();
    }

}