using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
using Quintessential.Settings;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DisjointAlchemy {

	public class DisjointAlchemy : QuintessentialMod {

    public static MethodInfo PrivateMethod<T>(string method) => typeof(T).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
    public static List<class_259> customSolitaires = new(); // LEft over from RMC debugging or something. Not touching this


        public static bool findModMetaFilepath(string name, out string filepath)
        {
            filepath = "<missing mod directory>";
            foreach (ModMeta mod in QuintessentialLoader.Mods)
            {
                if (mod.Name == name)
                {
                    filepath = mod.PathDirectory;
                    return true;
                }
            }
            return false;
        }

		public override void Load()
        {
            CampaignLoader.Load();
            StoryPanelPatcher.Load();
        }

        public override void LoadPuzzleContent()
        {
            Logger.Log("[DisjointAlchemy] Adding Jerin");
            class_172.field_1670.Add("Jerin", new class_230(class_134.method_253("Jerin Tenka", string.Empty), class_235.method_615("textures/portraits/jerin_large") /* Cutscene Portrait */, class_235.method_615("textures/portraits/jerin_small") /* Story Lore Portrait */, Color.FromHex(0x542C52), param_3968: false));
            Logger.Log("[DisjointAlchemy] Adding Serena");
            class_172.field_1670.Add("Serena", new class_230(class_134.method_253("Serena Penney", string.Empty), class_235.method_615("textures/portraits/serena_large") /* Cutscene Portrait */, class_235.method_615("textures/portraits/serena_small") /* Story Lore Portrait */, Color.FromHex(0x564F2D), param_3968: true));
            Logger.Log("[DisjointAlchemy] Adding Talma");
            class_172.field_1670.Add("Talma", new class_230(class_134.method_253("Professor Talma", string.Empty), class_235.method_615("textures/portraits/talma_small") /* Cutscene Portrait */, class_235.method_615("textures/portraits/talma_small") /* Story Lore Portrait */, Color.FromHex(0x38572d), param_3968: true));
        }

		public override void Unload()
        {
            SigmarGardenPatcher.Unload();
        }

		public override void PostLoad()
        {
            SigmarGardenPatcher.PostLoad();
            CampaignLoader.modifyCampaign();
            StoryPanelPatcher.PostLoad();
        }

	}
}
