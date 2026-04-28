//using Mono.Cecil.Cil;
//using MonoMod.Cil;
//using MonoMod.RuntimeDetour;
//using MonoMod.Utils;
using Quintessential;
using Quintessential.Serialization;
//using Quintessential.Settings;
//using SDL2;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
//using System.Globalization;
//using System.Reflection;

// all this code is taken from True Animismus Campaign and pared down, which I believe is taken from Reductive Metallurgy and pared down.

namespace DisjointAlchemy {

//using PartType = class_139;
//using Permissions = enum_149;
//using BondType = enum_126;
//using BondSite = class_222;
//using AtomTypes = class_175;
//using PartTypes = class_191;
//using Texture = class_256;
//using Song = class_186;
//using Tip = class_215;
//using Font = class_1;

public static class CampaignLoader
{
    const string FirstPuzzleID = "Disjoint-ch1-1-bacon-and-antimony";
    private static Campaign campaign_self;
    private static CampaignModelDisjoint campaign_model;

    public const enum_129 typePuzzle = (enum_129)0;
    public const enum_129 typeCutscene = (enum_129)1;
    public const enum_129 typeDocument = (enum_129)2;
    public const enum_129 typeSolitaire = (enum_129)3;

    public static bool CurrentCampaignIsDisjoint() => campaign_self == Campaigns.field_2330;
    public static CampaignModelDisjoint getModel() => campaign_model;

    /////////////////////////////////////////////////////////////////////////////////////////////////
    // helpers
    private static void patchCampaign(Campaign campaign)
    {
        foreach (CampaignChapter campaignChapter in campaign.field_2309)
        {
            foreach (CampaignItem campaignItem in campaignChapter.field_2314)
            {
                string field2322 = campaignItem.field_2322;
                if (campaignItem.field_2325.method_1085())
                {
                    Puzzle puzzle = campaignItem.field_2325.method_1087();
                    puzzle.field_2766 = field2322;
                    Array.Resize(ref Puzzles.field_2816, Puzzles.field_2816.Length + 1);
                    Puzzles.field_2816[Puzzles.field_2816.Length - 1] = puzzle;
                    foreach (PuzzleInputOutput puzzleInputOutput in puzzle.field_2770.Union(puzzle.field_2771))
                    {
                        if (!puzzleInputOutput.field_2813.field_2639.method_1085())
                            puzzleInputOutput.field_2813.field_2639 = (Maybe<LocString>)class_134.method_253("Molecule", string.Empty);
                    }
                }
            }
        }
    }
    public static void Load()
    {
        // load campaign model
        string filepath;
        if (!DisjointAlchemy.findModMetaFilepath("DisjointAlchemy", out filepath) || !File.Exists(filepath + "/Puzzles/Disjoint.advanced.yaml"))
        {
            Logger.Log("[DisjointAlchemy] Could not find 'Disjoint.advanced.yaml' in the folder '" + filepath + "/Puzzles/'");
            throw new Exception("modifyCampaignDisjoint: Campaign data is missing.");
        }
        using (StreamReader streamReader = new StreamReader(filepath + "/Puzzles/Disjoint.advanced.yaml"))
        {
            campaign_model = YamlHelper.Deserializer.Deserialize<CampaignModelDisjoint>(streamReader);
        }

        // hooking
        On.Solution.method_1958 += Solution_Method_1958;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////
    // tips
    static void initializeTips()
    {
        // manually load the puzzle file needed for tips
        string subpath = "/Puzzles/Disjoint-ch0-0-modding-sandbox.puzzle.yaml";
        string filepath;
        if (!DisjointAlchemy.findModMetaFilepath("DisjointAlchemy", out filepath) || !File.Exists(filepath + subpath))
        {
            Logger.Log("[DisjointAlchemy] Could not find 'Disjoint-ch0-0-modding-sandbox.puzzle.yaml in the folder '" + filepath + "/Puzzles/'");
            throw new Exception("LoadPuzzleContent: Tip data is missing.");
        }
        var tipsPuzzle = PuzzleModel.FromModel(YamlHelper.Deserializer.Deserialize<PuzzleModel>(File.ReadAllText(filepath + subpath)));

        Array.Resize(ref Puzzles.field_2816, Puzzles.field_2816.Length + 1);
        Puzzles.field_2816[Puzzles.field_2816.Length - 1] = tipsPuzzle;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////
    // puzzle-loader functions
    static Dictionary<string, Action<Puzzle>> LevelLoaders = new()
    {
        {FirstPuzzleID, LoadFirstPuzzle }
    };

    static void LoadFirstPuzzle(Puzzle puzzle) => StoryPanelPatcher.setOptionsUnlock(puzzle);

    #region polymer input/output puzzle loaders

    static void LoadPolymerOutputs(Puzzle puzzle) // REMOVE THIS ONCE QUINTESSENTIAL FIXES THE BUG
    {
        for (int i = 0; i < puzzle.field_2771.Length; i++)
        {
            var output = puzzle.field_2771[i];
            output.field_2813 = MoleculeEditorScreen.method_1133(output.field_2813, class_181.field_1716);
        }
    }

    #endregion

    /////////////////////////////////////////////////////////////////////////////////////////////////
    // main functions

    public static void modifyCampaign()
    {

        // fetch campaign data
        Logger.Log(QuintessentialLoader.AllCampaigns);
        foreach (Campaign campaign in QuintessentialLoader.AllCampaigns)
        {
            Logger.Log("Checking for campaign.QuintTitle == \"Disjoint Alchemy\"...");
            if (campaign.QuintTitle == "Disjoint Alchemy")
            {
                campaign_self = campaign;
                patchCampaign(campaign_self);
                break;
            }
        }
        List<string> sigmarsGardensIDList = new List<string>();
        StoryPanelPatcher.CreateSigmarStoryUnlocks(campaign_model.SigmarStoryUnlocks);

        foreach (var garden in campaign_model.SigmarsGardens)
        {
            sigmarsGardensIDList.Add(garden);
        }
        ////////////////////////////////////////
        // modify the campaign using the data //
        ////////////////////////////////////////
        Logger.Log("[DisjointAlchemy] Modifying campaign levels.");
        CampaignChapter[] campaignChapters = campaign_self.field_2309;
        foreach (var campaignChapter in campaignChapters)
        {
            if (campaignChapter.field_2310 == 1) campaignChapter.field_2321 = true;

            foreach (var campaignItem in campaignChapter.field_2314)
            {
                // modifiy puzzle data as necessary
                if (campaignItem.field_2325.method_1085())
                {
                    Puzzle puzzle = campaignItem.field_2325.method_1087();
                    string puzzleID = puzzle.field_2766;

                    if (sigmarsGardensIDList.Contains(puzzleID))
                    {
                        // change item into a Sigmars Garden
                        campaignItem.field_2324 = typeSolitaire;
                        DisjointAlchemy.customSolitaires.Add(campaignItem.field_2326); // SOLITAIRE_ICON_TEMP
                    }
                    
                }
            }
        }

        //JournalLoader.modifyJournals(campaign_self);
    }

    public static Maybe<Solution> Solution_Method_1958(On.Solution.orig_method_1958 orig, string filePath)
    {
        foreach (var dir in QuintessentialLoader.ModContentDirectories)
        {
            try
            {
                return orig(Path.Combine(dir, filePath));
            }
            catch (Exception) { }
        }

        return orig(filePath);
    }
}}