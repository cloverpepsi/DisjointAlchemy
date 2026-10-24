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

public class patch_OptionsScreen : OptionsScreen {

    [MonoModIgnore]
    public CampaignItemType itemType;

    public extern void orig_RenderFrame(float timeDelta);
	public void RenderFrame(float timeDelta)
	{
		if (Solitaire.currentCampaignIsDisjoint())
		{
			var screen_dyn = new DynamicData((OptionsScreen)(object)this);
			var currentStoryPanel = screen_dyn.Get<StoryPanel>("storyPanel");
			var stringArray = new DynamicData(currentStoryPanel).Get<string[]>("parsedText");
			if (!stringArray.Any(x => x.Contains("Serena") || x.Contains("Jerin") || x.Contains("Talma")))
			{
				var myVignette = new LocalizedVignette("options-Disjoint");
				myVignette.vignettePath = "options-Disjoint";
				screen_dyn.Set("storyPanel", new StoryPanel((Maybe<LocalizedVignette>)myVignette, false));
			}
		}
		orig_RenderFrame(timeDelta);
	}
}

public class patch_StoryPanel : StoryPanel {
    public patch_StoryPanel(Maybe<LocalizedVignette> vignette, bool b) : base(vignette, b){}
    public extern void orig_RenderFrame(float timeDelta, Vector2 pos, int index, Tuple<int, LocString>[] tuple);
	public void RenderFrame(float timeDelta, Vector2 pos, int index, Tuple<int, LocString>[] tuple)
	{

		if (Solitaire.currentCampaignIsDisjoint() && tuple.Length == 2 && tuple[0].Item2.ToString() == "Complete the prologue")
		{
			// then we're doing the options code while in the Disjoint campaign
			// hijack the inputs so we draw it our way
			bool flag = DisjointAlchemy.DisjointAlchemy.optionsUnlock.IsComplete();
			index = flag ? 1 : 0;
		}

		orig_RenderFrame(timeDelta, pos, index, tuple);
	}
}

public class patch_JournalScreen : JournalScreen {
    public patch_JournalScreen(bool showDifficultyWarningIfNeeded) : base(showDifficultyWarningIfNeeded) {}

    private extern void orig_RenderPuzzleSelect(Puzzle puzzle, Vector2 pos, bool isLarge);
    private void RenderPuzzleSelect(Puzzle puzzle, Vector2 pos, bool isLarge) {

        Texture puzzleBg = isLarge ? Assets.textures.journal.puzzle_large : Assets.textures.journal.puzzle_small;
        Bounds2 bounds = Bounds2.WithSize(pos, puzzleBg.size.ToVector2());
        bool hover = bounds.Contains(InputManager.MousePos());

        if (puzzle.puzzleId == "production-sigmar")
        {
            Texture tick = true /* TODO: count wins */ ? Assets.textures.puzzle_select.list_checked : Assets.textures.puzzle_select.list_unchecked;
            Texture divider = isLarge ? Assets.textures.journal.divider_large : Assets.textures.journal.divider_small;
            TextureRenderer.RenderText("Sigmar's Garden", pos + new Vector2(9, -19), Assets.fonts.crimson_15, UIConsts.lightTextColor, 0, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, new Color(), null, int.MaxValue, false, true);
            UI.DrawTexture(tick, pos + new Vector2(puzzleBg.size.X - 27, -23f));
            UI.DrawTexture(puzzleBg, pos);
            UI.DrawTexture(divider, pos + new Vector2(7f, -34f));
            UI.DrawTexture(hover ? Solitaire.sigmarHoverSprite : Solitaire.sigmarSprite, bounds.Min + new Vector2(13f, 13f));
            if (hover && InputManager.IsClickPressed(MouseButtonType.LeftClick))
            {
                var solitaireScreen = new SolitaireScreen(SolitaireType.Normal);
                UI.OpenScreen(solitaireScreen);
                Assets.sounds.click_button.PlaySound(1f);
            }
        }

        else if (puzzle.puzzleId == "Disjoint-j01-polarized-salts")
        {

            bool num = GameLogic.instance.saveData.IsComplete(puzzle);
            Texture texture = isLarge ? Assets.textures.journal.puzzle_large : Assets.textures.journal.puzzle_small;
            Texture texture2 = isLarge ? Assets.textures.journal.divider_large : Assets.textures.journal.divider_small;
            Texture texture3 = num ? Assets.textures.puzzle_select.list_checked : Assets.textures.puzzle_select.list_unchecked;
            float num2 = isLarge ? 468 : 294;
            string text = puzzle.journalAuthor.HasValue() ? $"{puzzle.puzzleName} ({puzzle.journalAuthor.GetValue()})" : ((string)puzzle.puzzleName);
            if (TextureRenderer.RenderText(text, pos + new Vector2(9f, -19f), Assets.fonts.crimson_15, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, float.MaxValue, 0, default(Color), null, int.MaxValue, calculateBounds: true, doRender: false).Width < num2)
            {
                num2 = float.MaxValue;
            }
            TextureRenderer.RenderText(text, pos + new Vector2(9f, -19f), Assets.fonts.crimson_15, UIConsts.lightTextColor, TextAlignment.Left, 1f, 0.6f, float.MaxValue, num2, 0, default(Color), null, int.MaxValue, calculateBounds: false, doRender: true);
            TextureRenderer.Render(texture3, pos + new Vector2(texture.size.X - 27, -23f));
            TextureRenderer.Render(texture2, pos + new Vector2(isLarge ? 7 : 7, -34f));
            TextureRenderer.Render(texture, pos);
			TextureRenderer.Render(Editor.RenderMoleculeForDisplay(new MoleculeBuilder()
                .Push(-1, 0)
                .Atom(AtomTypes.vitae)
                .Bond(1, 0)
                .Atom(AtomTypes.salt)
				.Bond(1, -1)
				.Atom(AtomTypes.mors)
				.Pop()
                .Push(-1,2)
				.Atom(AtomTypes.vitae)
				.Bond(1, 0)
				.Atom(AtomTypes.salt)
				.Bond(1, 0)
				.Atom(AtomTypes.mors)
                .Pop()
                .Push(2, -2)
                .Atom(AtomTypes.vitae)
                .Bond(1, 0)
                .Atom(AtomTypes.salt)
                .Bond(0,-1)
                .Atom(AtomTypes.mors), isInput: false, hover, new Vector2(500f, 500f), isLarge, 0.65f).GetTarget().renderedTexture, bounds.Min + new Vector2(17f, 8f));

            if (hover && InputManager.IsClickPressed(MouseButtonType.LeftClick))
            {
                Tuple<MusicTrack, Sound> tuple = PuzzleEditorScreen.RandomTrack(puzzle);
                GameLogic.instance.PushScreen(new PuzzleInfoScreen(puzzle, tuple.Item1, tuple.Item2, MaybeHelper.empty));
                Volume.PlaySound(Assets.sounds.click_button, 1f);
            }
        }

        else { orig_RenderPuzzleSelect(puzzle, pos, isLarge); }
    }
}