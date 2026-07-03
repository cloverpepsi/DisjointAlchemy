using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
//using Quintessential.Settings;
//using SDL2;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
//using System.Reflection;

namespace DisjointAlchemy;

using AtomTypes = class_175;
using Permissions = enum_149;
using PartType = class_139;
using PartTypes = class_191;
using Texture = class_256;

public static class Parts
{
	public static PartType Talma;

    public static readonly AtomType salt = class_175.field_1675;
    public static Texture GetTexture(string path = "Quintessential/missing") => class_235.method_615(path);

    public static PartType Disjunction;

    public static Texture disjunctionBase = GetTexture("textures/parts/disjunction_base");
    public static Texture disjunctionGlow = GetTexture("textures/parts/disjunction_glow");
    public static Texture disjunctionStroke = GetTexture("textures/parts/disjunction_stroke");
    public static Texture saltSymbol = GetTexture("textures/parts/symbol_salt");
    public static Texture saltPip = GetTexture("textures/parts/symbol_pip");

    public static Texture disjunctionIcon = GetTexture("textures/parts/disjunction_icon");
    public static Texture disjunctionHover = GetTexture("textures/parts/disjunction_icon_hover");

	public static Texture[] disjunctionFlash = new Texture[10]
		{
			GetTexture("textures/parts/disjunction_flash/calcify_glyph_0001"),
			GetTexture("textures/parts/disjunction_flash/calcify_glyph_0002"),
			GetTexture("textures/parts/disjunction_flash/calcify_glyph_0003"),
			GetTexture("textures/parts/disjunction_flash/calcify_glyph_0004"),
			GetTexture("textures/parts/disjunction_flash/calcify_glyph_0005"),
			GetTexture("textures/parts/disjunction_flash/calcify_glyph_0006"),
			GetTexture("textures/parts/disjunction_flash/calcify_glyph_0007"),
			GetTexture("textures/parts/disjunction_flash/calcify_glyph_0008"),
			GetTexture("textures/parts/disjunction_flash/calcify_glyph_0009"),
			GetTexture("textures/parts/disjunction_flash/calcify_glyph_0010")
		};

    public static Texture disjunctionBowl = class_238.field_1989.field_90.field_170;

    public static readonly HexIndex disjunctionL = new(0, 0);
    public static readonly HexIndex disjunctionR = new(1, 1);

    // these two methods are taken from Brimstone API
    public static Maybe<Molecule> FindMoleculeRelative(Sim sim, Part part, HexIndex offset)
    {
        if (!sim.FindAtom(part.method_1184(offset)).method_99(out AtomReference atom))
        {
            return struct_18.field_1431;
        }
        return atom.field_2277;
    }

    public static int JoinMoleculesAtHexes(Sim sim, Part part, HexIndex offset1, HexIndex offset2)
    {
        if (!FindMoleculeRelative(sim, part, offset1).method_99(out Molecule molecule1) || !FindMoleculeRelative(sim, part, offset2).method_99(out Molecule molecule2))
        {
            return 0;
        }
        if (molecule1 == molecule2)
        {
            return 1;
        }
        sim.field_3823.Remove(molecule1);
        sim.field_3823.Remove(molecule2);
        sim.field_3823.Add(molecule1.method_1119(molecule2));
        return 2;
    }

    public static void AddPartTypes()
	{
        Disjunction = new()
        {
            field_1528 = "disjoint-disjunction", // ID
            field_1529 = class_134.method_253("Glyph of Disjunction", string.Empty), // Name
            field_1530 = class_134.method_253("The glyph of disjunction combines two molecules together into a disjoint molecule via two salt atoms.", string.Empty), // Description
            field_1531 = 50, // Cost
            field_1539 = true, // Is a glyph
            field_1549 = disjunctionGlow, // Shadow/glow
            field_1550 = disjunctionStroke, // Stroke/outline
            field_1547 = disjunctionIcon, // Panel icon
            field_1548 = disjunctionHover, // Hovered panel icon
            field_1540 = new HexIndex[]
            {
                disjunctionL,
                disjunctionR
            },
            field_1551 = Permissions.None,
            CustomPermissionCheck = perms => perms.Contains("DisjointAlchemy:disjunction")
        };

        QApi.AddPartTypeToPanel(Disjunction, false);

        QApi.AddPartType(Disjunction, static (part, pos, editor, renderer) =>
        {
            renderer.method_523(disjunctionBase, Vector2.Zero, new Vector2(164f, 121f), 0f);
            renderer.method_530(class_238.field_1989.field_90.field_228.field_273, disjunctionL, 3f);
            renderer.method_530(class_238.field_1989.field_90.field_228.field_273, disjunctionR, 3f);
            renderer.method_528(disjunctionBowl, disjunctionL, Vector2.Zero);
            renderer.method_528(disjunctionBowl, disjunctionR, Vector2.Zero);
            renderer.method_529(saltSymbol, disjunctionL, Vector2.Zero);
            renderer.method_529(saltSymbol, disjunctionR, Vector2.Zero);
            renderer.method_523(saltPip, Vector2.Zero, new Vector2(0f, 9f), 0f);
            renderer.method_523(saltPip, Vector2.Zero, new Vector2(-82f, -38f), 0f);
        });

        QApi.RunAfterCycle((sim, first) =>
        {
            SolutionEditorBase seb = sim.field_3818;
            Dictionary<Part, PartSimState> pss = sim.field_3821;
            List<Part> parts = seb.method_502().field_3919;

            foreach (Part part in parts)
            {
                PartType type = part.method_1159();
                if (type == Disjunction)
                {
                    HexIndex relative_hex_L = part.method_1184(disjunctionL);
                    HexIndex relative_hex_R = part.method_1184(disjunctionR);
                    if (sim.FindAtomRelative(part, disjunctionL).method_99(out AtomReference s1) && s1.field_2280 == salt &&
                        sim.FindAtomRelative(part, disjunctionR).method_99(out AtomReference s2) && s2.field_2280 == salt)
                    {
                        if (JoinMoleculesAtHexes(sim, part, disjunctionL, disjunctionR) == 2) {
                        seb.field_3935.Add(new class_228(seb, (enum_7)1, class_187.field_1742.method_492(relative_hex_L), disjunctionFlash, 30f, Vector2.Zero, 0f));
                        seb.field_3935.Add(new class_228(seb, (enum_7)1, class_187.field_1742.method_492(relative_hex_R), disjunctionFlash, 30f, Vector2.Zero, 0f));
                        }
                    }
                }
                
            }
        });
    }

}