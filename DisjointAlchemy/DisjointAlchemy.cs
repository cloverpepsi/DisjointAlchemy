using MonoMod.RuntimeDetour;
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

using Texture = class_256;

namespace DisjointAlchemy {

	public class DisjointAlchemy : QuintessentialMod {
	public static AdvancedContentModelDisjoint AdvancedContent;
    public static FieldInfo PrivateField<T>(string field) => typeof(T).GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
    public static MethodInfo PrivateMethod<T>(string method) => typeof(T).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
    public static List<class_259> customSolitaires = new(); // Left over from RMC debugging or something. Not touching this
    private delegate void orig_MoleculeEditorScreen_method_1132(MoleculeEditorScreen self);

	public static string FilePath = "";

	public override Type SettingsType => typeof(MySettings);
	public static QuintessentialMod MainClassAsMod;
	public class MySettings
	{
		public static MySettings Instance => MainClassAsMod.Settings as MySettings;

		[SettingsLabel("Increase Sigmar Garden Atom Count")]
		public bool sigmarEmpty = false;
		[SettingsLabel("")]
		public DisplaySettings displayEditingSettings = new();
		public class DisplaySettings : SettingsGroup
		{
			public override bool Enabled => Instance.sigmarEmpty;

		}
	}

	public override void ApplySettings()
	{
		base.ApplySettings();

		var SET = (MySettings)Settings;
		SigmarGardenPatcher.sigmarEmpty = SET.sigmarEmpty;
	}

	public static Vector2 hexGraphicalOffset(HexIndex hex) => class_187.field_1742.method_492(hex);
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

        static void LoadAdvancedContent()
        {
            string subpath = "/Puzzles/";
            string file = "Disjoint.advanced.yaml";
            using (StreamReader streamReader = new StreamReader(FilePath + subpath + file))
            {
                AdvancedContent = YamlHelper.Deserializer.Deserialize<AdvancedContentModelDisjoint>(streamReader);
            }
        }


		public override void Load()
        {

            string name = "DisjointAlchemy";
            foreach (ModMeta mod in QuintessentialLoader.Mods)
            {
                if (mod.Name == name)
                {
                    FilePath = mod.PathDirectory;
                    break;
                }
            }
            
    		MainClassAsMod = this;
    		Settings = new MySettings();

            LoadAdvancedContent();
            CampaignLoader.Load();
            CutscenePatcher.Load();
            Document.Load();
            JournalLoader.Load();
            StoryPanelPatcher.Load();
        }

        public override void LoadPuzzleContent()
        {
    		Wheel.LoadContent();
            Parts.AddPartTypes();
            StoryPanelPatcher.LoadContent();
    		QApi.AddPuzzlePermission("DisjointAlchemy:talma", "Talma's Wheel", "Disjoint Alchemy");
            QApi.AddPuzzlePermission("DisjointAlchemy:disjunction", "Glyph of Disjunction", "Disjoint Alchemy");
            IL.SolutionEditorBase.method_1984 += drawTalmaWheelAtoms;
            Wheel.LoadPuzzleContent();
        }

        private static void drawTalmaWheelAtoms(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            // skip ahead to roughly where method_2015 is called
            cursor.Goto(658);

            // jump ahead to just after the method_2015 for-loop
            if (!cursor.TryGotoNext(MoveType.After, instr => instr.Match(OpCodes.Ldarga_S))) return;

            // load the SolutionEditorBase self and the class423 local onto the stack so we can use it
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloc_0);
            // then run the new code
            cursor.EmitDelegate<Action<SolutionEditorBase, SolutionEditorBase.class_423>>((seb_self, class423) =>
            {
                if (seb_self.method_503() != enum_128.Stopped)
                {
                    var partList = seb_self.method_502().field_3919;
                    foreach (var talma in partList.Where(x => x.method_1159() == Wheel.Talma))
                    {
                        Wheel.drawTalmaAtoms(seb_self, talma, class423.field_3959);
                    }
                }
            });
        }

		public override void Unload()
        {
            SigmarGardenPatcher.Unload();
            JournalLoader.Unload();
        }


		public override void PostLoad()
        {
            On.SolutionEditorBase.method_1997 += DrawPartSelectionGlows;
            SigmarGardenPatcher.PostLoad();
            CampaignLoader.modifyCampaign();
            StoryPanelPatcher.PostLoad();
        }

        
        public void DrawPartSelectionGlows(On.SolutionEditorBase.orig_method_1997 orig, SolutionEditorBase seb_self, Part part, Vector2 pos, float alpha)
        {
            if (part.method_1159() == Wheel.Talma) Wheel.drawSelectionGlow(seb_self, part, pos, alpha);
            orig(seb_self, part, pos, alpha);
        }

	}
}
