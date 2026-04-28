//using Mono.Cecil.Cil;
//using MonoMod.Cil;
//using MonoMod.RuntimeDetour;
//using MonoMod.Utils;
using Quintessential;
//using Quintessential.Serialization;
//using Quintessential.Settings;
//using SDL2;
using System;
using System.IO;
//using System.Linq;
using System.Collections.Generic;
using System.Globalization;
//using System.Reflection;

namespace DisjointAlchemy {

// all this code is taken from True Animismus Campaign and pared down, which I believe is taken from Reductive Metallurgy and pared down.

//using PartType = class_139;
//using Permissions = enum_149;
//using BondType = enum_126;
//using BondSite = class_222;
//using AtomTypes = class_175;
//using PartTypes = class_191;
using Texture = class_256;
//using Song = class_186;
using Tip = class_215;
//using Font = class_1;

/////////////////////////////////////////////////////////////////////////////////////////////////
// advanced.yaml

public static class ModelHelpersDisjoint
{
    static NumberStyles style = NumberStyles.Any;
    static NumberFormatInfo format = CultureInfo.InvariantCulture.NumberFormat;

    public static float FloatFromString(string str, float defaulF = 0f)
    {
        if (!string.IsNullOrEmpty(str))
        {
            return float.Parse(str, style, format);
        }
        else
        {
            return defaulF;
        }
    }

    public static Vector2 Vector2FromString(string pos, float defaultX = 0f, float defaultY = 0f)
    {
        float x = FloatFromString(pos?.Split(',')[0], defaultX);
        float y = FloatFromString(pos?.Split(',')[1], defaultY);
        return new Vector2(x, y);
    }

    public static Color HexColor(int hex)
    {
        return Color.FromHex(hex);
    }

    public static Color ColorWhite => Color.White;
}

public class CampaignModelDisjoint
{
    public CreditsModelDisjoint Credits;
    public List<int> SigmarStoryUnlocks;
    public List<string> SigmarsGardens;
    public List<CutsceneModelDisjoint> Cutscenes;

}
public class CreditsModelDisjoint
{
    public string PositionOffset;
    public List<List<string>> Texts;
}
public class CutsceneModelDisjoint
{
    public string ID, Location, Background, Music;
}
//////////////////////////////////////////////////
public class TipModelDisjoint
{
    public string ID, Title, Description, Solution, Texture, SolutionOffset;
    Texture loadedTexture;

    public Tip FromModel()
    {
        Maybe<Texture> image = (Maybe<Texture>)struct_18.field_1431;

        if (!string.IsNullOrEmpty(this.Texture))
        {
            if(this.loadedTexture is null) this.loadedTexture = class_235.method_615(this.Texture); // if null, load the texture
            image = this.loadedTexture;
        }

        return new Tip()
        {
            field_1899 = this.ID,
            field_1900 = class_134.method_253(this.Title ?? "<Untitled Tip>", string.Empty),
            field_1901 = class_134.method_253(this.Description ?? "<Description Missing>", string.Empty),
            field_1902 = this.Solution ?? "speedbonder",
            field_1903 = image,
            field_1904 = ModelHelpersDisjoint.Vector2FromString(this.SolutionOffset),
        };
    }
}
}