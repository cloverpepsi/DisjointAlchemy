#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CS0618 // Ignore the error for RunAfterCycle i'll fix it later
using MonoMod;
using MonoMod.Cil;
using MonoMod.Utils;
using Mono.Cecil.Cil;
using Mono.Cecil;
using Quintessential;
//using Quintessential.Settings;
//using SDL2;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
//using System.Reflection;
using DisjointAlchemy;

namespace DisjointAlchemy {

public static class Parts
{
	public static PartType Talma;

    public static Texture GetTexture(string path = "Quintessential/missing") => AssetLoaderHelper.LoadTexture(path);
    public static PartType Disjunction;


    public static Texture disjunctionBase = GetTexture("textures/disjoint/parts/disjunction/disjunction_base");
    public static Texture disjunctionGlow = GetTexture("textures/disjoint/parts/disjunction/disjunction_glow");
    public static Texture disjunctionStroke = GetTexture("textures/disjoint/parts/disjunction/disjunction_stroke");
    public static Texture saltSymbol = GetTexture("textures/disjoint/parts/disjunction/symbol_salt");
    public static Texture saltPip = GetTexture("textures/disjoint/parts/disjunction/symbol_pip");

    public static Texture talmaIcon = GetTexture("textures/disjoint/parts/talma");
    public static Texture talmaHover = GetTexture("textures/disjoint/parts/talma_hover");

    static HexIndex[] TalmaHexes = { new HexIndex(0, 2), new HexIndex(2, 0), new HexIndex(2, -2), new HexIndex(0, -2), new HexIndex(-2, 0), new HexIndex(-2, 2), new HexIndex(2, -1), new HexIndex(-1, 2), new HexIndex(1, -2), new HexIndex(-2, 1)};

    public static Texture disjunctionIcon = GetTexture("textures/disjoint/parts/disjunction/disjunction_icon");
    public static Texture disjunctionHover = GetTexture("textures/disjoint/parts/disjunction/disjunction_icon_hover");

	public static Texture[] disjunctionFlash = new Texture[10]
		{
			GetTexture("textures/disjoint/parts/disjunction_flash/calcify_glyph_0001"),
			GetTexture("textures/disjoint/parts/disjunction_flash/calcify_glyph_0002"),
			GetTexture("textures/disjoint/parts/disjunction_flash/calcify_glyph_0003"),
			GetTexture("textures/disjoint/parts/disjunction_flash/calcify_glyph_0004"),
			GetTexture("textures/disjoint/parts/disjunction_flash/calcify_glyph_0005"),
			GetTexture("textures/disjoint/parts/disjunction_flash/calcify_glyph_0006"),
			GetTexture("textures/disjoint/parts/disjunction_flash/calcify_glyph_0007"),
			GetTexture("textures/disjoint/parts/disjunction_flash/calcify_glyph_0008"),
			GetTexture("textures/disjoint/parts/disjunction_flash/calcify_glyph_0009"),
			GetTexture("textures/disjoint/parts/disjunction_flash/calcify_glyph_0010")
		};

    public static Texture disjunctionBowl = Assets.textures.parts.calcinator_bowl;

    public static readonly HexIndex disjunctionL = new(0, 0);
    public static readonly HexIndex disjunctionR = new(1, 1);

    // these two methods are taken from Brimstone API
    public static Maybe<Molecule> FindMoleculeRelative(Sim sim, Part part, HexIndex offset)
    {
        if (!sim.FindAtom(part.InFrontBy(offset)).GetOrDefault(out AtomReference atom))
        {
            return MaybeHelper.empty;
        }
        return atom.molecule;
    }

    public static int JoinMoleculesAtHexes(Sim sim, Part part, HexIndex offset1, HexIndex offset2)
    {
        if (!FindMoleculeRelative(sim, part, offset1).GetOrDefault(out Molecule molecule1) || !FindMoleculeRelative(sim, part, offset2).GetOrDefault(out Molecule molecule2))
        {
            return 0;
        }
        if (molecule1 == molecule2)
        {
            return 1;
        }
        sim.molecules.Remove(molecule1);
        sim.molecules.Remove(molecule2);
        sim.molecules.Add(molecule1.MergeWith(molecule2));
        return 2;
    }

    public static void AddPartTypes()
	{
        Disjunction = new()
        {
            cost = 50, // Cost
            isFullHexCover = true, // Is a glyph
            glowTexture = disjunctionGlow, // Shadow/glow
            strokeTexture = disjunctionStroke, // Stroke/outline
            baseTexture = disjunctionIcon, // Panel icon
            hoverTexture = disjunctionHover, // Hovered panel icon
            glyphHexes = new HexIndex[]
            {
                disjunctionL,
                disjunctionR
            },
            CustomPermissionCheck = perms => perms.Contains(DisjointAlchemy.Instance.GetIdentifier("disjunction"))
        };

        QApi.AddPartTypeToPanel(Disjunction, false);
        DisjointAlchemy.Instance.AddPartType(Disjunction, "disjunction", (part, pos, editor, renderer) =>
        {
            renderer.RenderBase(disjunctionBase, Vector2.Zero, new Vector2(164f, 121f), 0f);
            renderer.RenderShadowCircle(Assets.textures.parts.animismus.ring_shadow, disjunctionL, 3f);
            renderer.RenderShadowCircle(Assets.textures.parts.animismus.ring_shadow, disjunctionR, 3f);
            renderer.RenderRotating(disjunctionBowl, disjunctionL, Vector2.Zero);
            renderer.RenderRotating(disjunctionBowl, disjunctionR, Vector2.Zero);
            renderer.RenderUpright(saltSymbol, disjunctionL, Vector2.Zero);
            renderer.RenderUpright(saltSymbol, disjunctionR, Vector2.Zero);
            renderer.RenderBase(saltPip, Vector2.Zero, new Vector2(0f, 9f), 0f);
            renderer.RenderBase(saltPip, Vector2.Zero, new Vector2(-82f, -38f), 0f);
        });
        DisjointAlchemy.Instance.AddPuzzlePermission("disjunction");


        Talma = new PartType
        {
            baseTexture = talmaIcon, // Panel icon
            hoverTexture = talmaHover, // Hovered panel icon
            cost = 30,
            isUnique = true,
            layer = PartTypeLayer.Top,
            isProgrammable = true,
            attachedAtoms = new Dictionary<HexIndex, AtomType>{},
            canRotateInSimulation = true,
            permissionCategory = PuzzlePermissions.None,
            CustomPermissionCheck = perms => perms.Contains(DisjointAlchemy.Instance.GetIdentifier("talma"))
        };

		foreach (var hex in TalmaHexes) {
			Talma.attachedAtoms.Add(hex,AtomTypes.salt);
		}

        QApi.AddPartTypeToPanel(Talma, true);
        DisjointAlchemy.Instance.AddPartType(Talma, "talma", (part, pos, editor, renderer) =>
        {
            DisjointAlchemy.PrivateMethod<Part>("SetArmLength").Invoke(part, new object[] { 2 });
        });
        DisjointAlchemy.Instance.AddPuzzlePermission("talma");

        QApi.RunAfterCycle((sim, first) =>
        {

            SolutionEditorBase seb = sim.solutionEditor;
            Dictionary<Part, PartSimState> pss = sim.simulationDict;
            List<Part> parts = seb.GetSolution().parts;

            foreach (Part part in parts)
            {
                PartType type = part.GetType();
                if (type == Disjunction)
                {
                    HexIndex relative_hex_L = part.InFrontBy(disjunctionL);
                    HexIndex relative_hex_R = part.InFrontBy(disjunctionR);
                    if (sim.FindAtomRelative(part, disjunctionL).GetOrDefault(out AtomReference s1) && s1.atomType == AtomTypes.salt &&
                        sim.FindAtomRelative(part, disjunctionR).GetOrDefault(out AtomReference s2) && s2.atomType == AtomTypes.salt)
                    {
                        if (JoinMoleculesAtHexes(sim, part, disjunctionL, disjunctionR) == 2) {
                        seb.glyphEffects.Add(new GlyphEffect(seb, (EffectTimescaleType)1, HexGrid.standardGrid.ToPixelCoords(relative_hex_L), disjunctionFlash, 30f, Vector2.Zero, 0f));
                        seb.glyphEffects.Add(new GlyphEffect(seb, (EffectTimescaleType)1, HexGrid.standardGrid.ToPixelCoords(relative_hex_R), disjunctionFlash, 30f, Vector2.Zero, 0f));
                        }
                    }
                }
                
            }
        });
        
    }

}
}

public class patch_Sim : Sim {

    [MonoModIgnore]
    private SolutionEditorBase solutionEditor;

    public extern bool orig_HasOverlap(Molecule molecule, HashSet<HexIndex> additionalHexes);

    public bool HasOverlap(Molecule molecule, HashSet<HexIndex> additionalHexes)
    {

        bool has_talma = false;
        foreach (Part part in solutionEditor.GetSolution().parts)
        {
            if (part.GetType() == Parts.Talma) {has_talma = true;}
        }
        
        if (!has_talma) {return orig_HasOverlap(molecule, additionalHexes);}

		foreach (HexIndex key in molecule.GetAtoms().Keys)
		{
			if (additionalHexes.Contains(key))
			{
				return true;
			}
		}
		foreach (Part part in solutionEditor.GetSolution().parts)
		{
			if (!part.HasAtoms())
			{
				continue;
			}
			PartSimState partSimState = simulationDict[part];
			foreach (HexIndex key2 in molecule.GetAtoms().Keys)
			{
                foreach (HexIndex key3 in part.GetType().attachedAtoms.Keys)
                {
                    HexIndex localHex = part.InFrontBy(key3);
                    if (HexIndex.Distance(key2, localHex) == 0) { return true; }
                }
			}
		}
		return false;
    }
}

public abstract class patch_SolutionEditorBase : SolutionEditorBase {

    [MonoModILInject("layer_0_RenderPartBase")]
    public static void PatchCage(MethodDefinition method, CustomAttribute attrib)
    {
        Console.WriteLine("Patching RenderPartBase atom cages");
        ILCursor gremlin = new(new ILContext(method));
        if (!gremlin.TryGotoNext(instr => instr.OpCode == OpCodes.Callvirt && ((MethodReference)instr.Operand).Name == "HasAtoms"))
        {
            Console.WriteLine("Failed to find HasAtoms predicate.");
            throw new Exception();
        }
        if (!gremlin.TryGotoNext(
            instr => instr.MatchLdcI4(1),
            instr => instr.MatchLdcI4(0),
            instr => instr.MatchNewobj(out MethodReference ctor) && ctor.DeclaringType.Name == "HexIndex"))
        {
            Console.WriteLine("Failed to find HexIndex constructor.");
            throw new Exception();
        }
        TypeDefinition partTD = method.Parameters[0].ParameterType.Resolve();
        MethodDefinition getArmLenMD = partTD.Methods.First((m) => m.Name == "GetArmLength");
        gremlin.RemoveRange(1);
        gremlin.Emit(OpCodes.Ldarg_1);
        gremlin.Emit(OpCodes.Call, getArmLenMD);
    }

    public extern void orig_RenderGlow(Part part, Vector2 pos, float alpha);
    public void RenderGlow(Part part, Vector2 pos, float alpha)
    {
        if (part.GetType() == Parts.Talma)
        {
            var cageSelectGlowTexture = Assets.textures.select.cage;
            int armLength = 2; // part.method_1165()
            IntermediatePartState ips = GetIntermState(part, pos);
            Color color = Color.White.WithAlpha(alpha);
            DisjointAlchemy.DisjointAlchemy.PrivateMethod<SolutionEditorBase>("RenderArmSelection").Invoke(((SolutionEditorBase)(object)this), new object[] { armLength, PartTypes.hexArm.armRotations, ips, color });
        }
        else
        {
            orig_RenderGlow(part, pos, alpha);
        }
    }
}