using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Quintessential;
//using Quintessential.Settings;
//using SDL2;
using System;
//using System.IO;
using System.Linq;
using System.Collections.Generic;
//using System.Reflection;
using MonoMod.Cil;
using MonoMod;
using Quintessential.Serialization;
using YamlDotNet.Core;


namespace DisjointAlchemy;

using AtomTypes = class_175;

using PartType = class_139;
using PartTypes = class_191;
using Texture = class_256;

public static class Wheel
{
	public static PartType Talma;

	const float sixtyDegrees = 60f * (float)Math.PI / 180f;
	const string TalmaWheelAtomsField = "DisjointAlchemy_TalmaWheelAtoms";
    private static Hook change_talma_description;

	public static AtomType[] other_atomTypes => new AtomType[9] {
		AtomTypes.field_1675, // salt
		AtomTypes.field_1676, // air
		AtomTypes.field_1677, // earth
		AtomTypes.field_1678, // fire
		AtomTypes.field_1679, // water
		AtomTypes.field_1687, // vitae
		AtomTypes.field_1688, // mors
		AtomTypes.field_1689, // repeat
		AtomTypes.field_1690, // quintessence
	};

	static class_126 atomCageLighting => class_238.field_1989.field_90.field_232;
	static PartType Berlo => PartTypes.field_1771;
	static HexRotation[] HexArmRotations => PartTypes.field_1767.field_1534;

	static HexIndex[] TalmaHexes = { new HexIndex(0, 2), new HexIndex(2, 0), new HexIndex(2, -2), new HexIndex(0, -2), new HexIndex(-2, 0), new HexIndex(-2, 2), new HexIndex(2, -1), new HexIndex(-1, 2), new HexIndex(1, -2), new HexIndex(-2, 1)};
	static Molecule TalmaMolecule()
	{
		Molecule molecule = new Molecule();
		molecule.method_1105(new Atom(other_atomTypes[0]), new HexIndex(0, 2)); //1
		molecule.method_1105(new Atom(other_atomTypes[0]), new HexIndex(2, 0)); //2
		molecule.method_1105(new Atom(other_atomTypes[0]), new HexIndex(2, -2)); //0
		molecule.method_1105(new Atom(other_atomTypes[0]), new HexIndex(0, -2)); //1
		molecule.method_1105(new Atom(other_atomTypes[0]), new HexIndex(-2, 0)); //2
		molecule.method_1105(new Atom(other_atomTypes[0]), new HexIndex(-2, 2)); //0
		molecule.method_1105(new Atom(other_atomTypes[0]), new HexIndex(2, -1)); //4
		molecule.method_1105(new Atom(other_atomTypes[0]), new HexIndex(-1, 2)); //3
		molecule.method_1105(new Atom(other_atomTypes[0]), new HexIndex(1, -2)); //3
		molecule.method_1105(new Atom(other_atomTypes[0]), new HexIndex(-2, 1)); //4
		return molecule;
	}
	// ============================= //
	// public methods called by main
	public static void drawSelectionGlow(SolutionEditorBase seb_self, Part part, Vector2 pos, float alpha)
	{
		var cageSelectGlowTexture = class_238.field_1989.field_97.field_367;
		int armLength = 2; // part.method_1165()
		class_236 class236 = seb_self.method_1989(part, pos);
		Color color = Color.White.WithAlpha(alpha);

		DisjointAlchemy.PrivateMethod<SolutionEditorBase>("method_2006").Invoke(seb_self, new object[] { armLength, HexArmRotations, class236, color });
	}

	public static void drawTalmaAtoms(SolutionEditorBase seb_self, Part part, Vector2 pos)
	{
		if (part.method_1159() != Talma) return;

		PartSimState partSimState = seb_self.method_507().method_481(part);

		class_236 class236 = seb_self.method_1989(part, pos);
		Editor.method_925(GetTalmaWheelAtoms(partSimState), class236.field_1984, new HexIndex(0,0), class236.field_1985, 1f, 1f, 1f, false, seb_self);
	}

	public static Maybe<AtomReference> maybeFindTalmaWheelAtom(Sim sim_self, Part part, HexIndex offset)
	{

		DisjointAlchemy.PrivateMethod<Part>("method_1166").Invoke(part, [2]);

		var SEB = sim_self.field_3818;
		var solution = SEB.method_502();
		var partList = solution.field_3919;
		var partSimStates = sim_self.field_3821;

		HexIndex key = part.method_1184(offset);
		foreach (var Talma in partList.Where(x => x.method_1159() == Talma))
		{
			var partSimState = partSimStates[Talma];
			Molecule TalmaAtoms = GetTalmaWheelAtoms(partSimState);
			var hexIndex = partSimState.field_2724;
			var rotation = partSimState.field_2726;
			var hexKey = (key - hexIndex).Rotated(rotation.Negative());

			Atom atom;
			if (TalmaAtoms.method_1100().TryGetValue(hexKey, out atom))
			{
				return (Maybe<AtomReference>)new AtomReference(TalmaAtoms, hexKey, atom.field_2275, atom, true);
			}
		}
		return (Maybe<AtomReference>)struct_18.field_1431;
	}

	private static bool ContentLoaded = false;

	public static void LoadPuzzleContent()
	{
		// hook from Iris. Thanks Iris
		change_talma_description = new Hook(
        typeof(PuzzleInfoScreen).GetMethod("method_1275", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance),
        (Action<PuzzleInfoScreen, Solution> orig,
        PuzzleInfoScreen self,
        Solution param_5012) => {
        var puzzle = param_5012.method_1934();
        if (puzzle.CustomPermissions.Contains("DisjointAlchemy:disjunction")) {
            Talma.field_1530 = class_134.method_253("By using Talma's wheel with the glyph of disjunction, you can accomplish absolutely nothing.", string.Empty);
        }
        else {
            Talma.field_1530 = class_134.method_253("By using Talma's wheel with the glyph of disjuncti- oh, that doesn't exist? I guess it doesn't do anything.", string.Empty);
        }
        orig(self, param_5012);
        }
    	);
	}
	public static void LoadContent()
	{
		if (ContentLoaded) return;
		ContentLoaded = true;
		//=========================//
		string iconpath = "textures/parts/talma";
		Talma = new PartType()
		{
			/*ID*/field_1528 = "disjoint-alchemy-Talma",
			/*Name*/field_1529 = class_134.method_253("Talma's Wheel", string.Empty),
			/*Desc*/field_1530 = class_134.method_253("By using Talma's wheel with the glyph of disjuncti- oh, that doesn't exist? I guess it doesn't do anything.", string.Empty),
			/*Cost*/field_1531 = 50,
			/*Type*/field_1532 = (enum_2) 1,
			/*Programmable?*/field_1533 = true,
			/*Force-rotatable*/field_1536 = true,
			/*Berlo Atoms*/field_1544 = new Dictionary<HexIndex, AtomType>(),
			/*Icon*/field_1547 = class_235.method_615(iconpath),
			/*Hover Icon*/field_1548 = class_235.method_615(iconpath + "_hover"),
			/*Only One Allowed?*/field_1552 = true,
			CustomPermissionCheck = perms => perms.Contains("DisjointAlchemy:talma")
		};
		foreach (var hex in TalmaHexes) {
			Talma.field_1544.Add(hex,other_atomTypes[0]);
		}
		QApi.AddPartTypeToPanel(Talma, Berlo);
		QApi.AddPartType(Talma, DrawTalmaPart);
	}

	// private methods
	private static void SetTalmaWheelData<T>(PartSimState state, string field, T data) => new DynamicData(state).Set(field, data);
	private static T GetTalmaWheelData<T>(PartSimState state, string field, T initial)
	{
		var data = new DynamicData(state).Get(field);
		if (data == null)
		{
			SetTalmaWheelData(state, field, initial);
			return initial;
		}
		else
		{
			return (T)data;
		}
	}
	private static Molecule GetTalmaWheelAtoms(PartSimState state) => GetTalmaWheelData(state, TalmaWheelAtomsField, TalmaMolecule());
	
	static void DrawTalmaPart (Part part, Vector2 pos, SolutionEditorBase editor, class_195 renderer)
	{

		DisjointAlchemy.PrivateMethod<Part>("method_1166").Invoke(part, [2]);

		// draw atoms, if the simulation is stopped - otherwise, the running simulation will draw them
		if (editor.method_503() == enum_128.Stopped)
		{
			drawTalmaAtoms(editor, part, pos);
		}

		// draw arm stubs
		class_236 class236 = editor.method_1989(part, pos);
		DisjointAlchemy.PrivateMethod<SolutionEditorBase>("method_2005").Invoke(editor, new object[] { part.method_1165(), HexArmRotations, class236 });
		// draw cages
		PartSimState partSimState = editor.method_507().method_481(part);
		for (int i = 0; i < 6; i++)
		{
			float radians = renderer.field_1798 + (i * sixtyDegrees);
			Vector2 vector2_9 = renderer.field_1797 + DisjointAlchemy.hexGraphicalOffset(new HexIndex(2, 0)).Rotated(radians);
			DisjointAlchemy.PrivateMethod<SolutionEditorBase>("method_2003").Invoke(editor, new object[] { atomCageLighting, vector2_9, new Vector2(39f, 33f), radians });
		}
	}

}