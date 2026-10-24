#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using MonoMod.Utils;
using Quintessential;
using Quintessential.Settings;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Linq;
//using System.Reflection;

namespace DisjointAlchemy {

	public class DisjointAlchemy : QuintessentialMod, IDataMod {
    public static DisjointAlchemy Instance { get; }
    public override string ModId => "disjoint_alchemy";
    public static FieldInfo PrivateField<T>(string field) => typeof(T).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
    public static MethodInfo PrivateMethod<T>(string method) => typeof(T).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
    public static CampaignItem optionsUnlock = null;
    public static bool CurrentlyInJournal()
    {
        Stack<IScreen> screen_data = (Stack<IScreen>)PrivateField<GameLogic>("screenStack").GetValue(GameLogic.instance);

        foreach (IScreen o in screen_data.stack)
            {
                if (o is JournalScreen) {return true;}
            }

        return false;
    }

    public static bool CurrentCampaignIsDisjoint(){
        return Campaigns.opusMagnum.QuintTitle == "Disjoint Alchemy";
    }

	public static string FilePath = "";

    public static QuintessentialMod MainClassAsMod;

    /*
	public override Type SettingsType => typeof(MySettings);

    public class MySettings
	{
		[SettingsLabel("Increase Sigmar Garden Atom Count")]
		public bool sigmarEmpty = false;
	}

	public override void ApplySettings()
	{
		base.ApplySettings();

		var SET = (MySettings)Settings;
		Solitaire.sigmarEmpty = SET.sigmarEmpty;
	}
    */

    public override void Load()
    {

        string name = "disjoint_alchemy";
        foreach (ModMeta mod in QuintessentialLoader.Mods)
        {
            if (mod.ModId == name)
            {
                FilePath = mod.PathToDirectory;
                break;
            }
        }
        
        MainClassAsMod = this;
        //Settings = new MySettings();
    }

    public override void LoadContent()
    {
        Logger.Log("[DisjointAlchemy] Adding Jerin");
        Portraits.portraits.Add("Jerin", new Portrait(Translations.Translate("disjoint_alchemy.characters.jerin"), AssetLoaderHelper.LoadTexture("textures/disjoint/portraits/jerin_large") /* Cutscene Portrait */, AssetLoaderHelper.LoadTexture("textures/disjoint/portraits/jerin_small") /* Story Lore Portrait */, Color.FromHex(0x542C52), isLeftSide: false));
        Logger.Log("[DisjointAlchemy] Adding Serena");
        Portraits.portraits.Add("Serena", new Portrait(Translations.Translate("disjoint_alchemy.characters.serena"), AssetLoaderHelper.LoadTexture("textures/disjoint/portraits/serena_large") /* Cutscene Portrait */, AssetLoaderHelper.LoadTexture("textures/disjoint/portraits/serena_small") /* Story Lore Portrait */, Color.FromHex(0x564F2D), isLeftSide: true));
        Logger.Log("[DisjointAlchemy] Adding Talma");
        Portraits.portraits.Add("Talma", new Portrait(Translations.Translate("disjoint_alchemy.characters.talma"), AssetLoaderHelper.LoadTexture("textures/disjoint/portraits/talma_small") /* Cutscene Portrait */, AssetLoaderHelper.LoadTexture("textures/disjoint/portraits/talma_small") /* Story Lore Portrait */, Color.FromHex(0x38572d), isLeftSide: true));

        Parts.AddPartTypes();
    }

    public override void LoadCompatContent() { }
    public override void FinaliseContent() { }

    public override void Unload()
    {
        //Solitaire.Unload();
    }


    public override void PostLoad()
    {
        Solitaire.PostLoad();


        Campaign campaign_self = null;
        foreach (Campaign campaign in QuintessentialLoader.AllCampaigns)
        {
            Logger.Log("Checking for campaign.QuintTitle == \"Disjoint Alchemy\"...");
            if (campaign.QuintTitle == "Disjoint Alchemy")
            {
                campaign_self = campaign;
                break;
            }
        }

        CampaignChapter[] campaignChapters = campaign_self.chapters;
        foreach (var campaignChapter in campaignChapters)
        {
            foreach (var campaignItem in campaignChapter.campaignItems)
            {
                if (campaignItem.itemId == "Disjoint_0_0c") { optionsUnlock = campaignItem;}
            }
        }
    }
}
}
