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

namespace DisjointAlchemy {

	public class DisjointAlchemy : QuintessentialMod {

    public static MethodInfo PrivateMethod<T>(string method) => typeof(T).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
    public static List<class_259> customSolitaires = new(); // LEft over from RMC debugging or something. Not touching this
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

		public override void Load()
        {
            CampaignLoader.Load();
            StoryPanelPatcher.Load();
        }

        public override void LoadPuzzleContent()
        {
    		Wheel.LoadContent();
            StoryPanelPatcher.LoadContent();
    		QApi.AddPuzzlePermission("DisjointAlchemy:talma", "Talma's Wheel", "Disjoint Alchemy");
            IL.SolutionEditorBase.method_1984 += drawTalmaWheelAtoms;

            Logger.Log("[DisjointAlchemy] Adding Jerin");
            class_172.field_1670.Add("Jerin", new class_230(class_134.method_253("Jerin Tenka", string.Empty), class_235.method_615("textures/portraits/jerin_large") /* Cutscene Portrait */, class_235.method_615("textures/portraits/jerin_small") /* Story Lore Portrait */, Color.FromHex(0x542C52), param_3968: false));
            Logger.Log("[DisjointAlchemy] Adding Serena");
            class_172.field_1670.Add("Serena", new class_230(class_134.method_253("Serena Penney", string.Empty), class_235.method_615("textures/portraits/serena_large") /* Cutscene Portrait */, class_235.method_615("textures/portraits/serena_small") /* Story Lore Portrait */, Color.FromHex(0x564F2D), param_3968: true));
            Logger.Log("[DisjointAlchemy] Adding Talma");
            class_172.field_1670.Add("Talma", new class_230(class_134.method_253("Professor Genea Talma", string.Empty), class_235.method_615("textures/portraits/talma_small") /* Cutscene Portrait */, class_235.method_615("textures/portraits/talma_small") /* Story Lore Portrait */, Color.FromHex(0x38572d), param_3968: true));
        
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
