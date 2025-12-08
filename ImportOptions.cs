using System;
using System.Linq;
using Godot;
using GodotDictionary = Godot.Collections.Dictionary;

namespace Raele.AtlasTextureImporter;

public record ImportOptions
{
	public enum ModeEnum : int
	{
		ImportAsAtlasTextures = 0,
		ImportAsPng = 1,
	}

	public static Godot.Collections.Array<GodotDictionary> _GetImportOptions(string path, int presetIndex) => [
		new GodotDictionary
		{
			{ "name", "mode" },
			{ "default_value", (long) ModeEnum.ImportAsAtlasTextures },
			{ "property_hint", (long) PropertyHint.Enum },
			{ "hint_string", Enum.GetValues<ModeEnum>().Select(value => $"{value}:{Enum.Format(typeof(ModeEnum), value, "d")}").ToArray().Join(",") },
			{ "usage", (int) PropertyUsageFlags.Default | (int) PropertyUsageFlags.UpdateAllIfModified },
		},
		new GodotDictionary
		{
			{ "name", "remove_margins" },
			{ "default_value", false },
			{ "usage", (int) PropertyUsageFlags.Default | (int) PropertyUsageFlags.Checkable },
		},
	];

	public static bool _GetOptionVisibility(string path, StringName optionName, GodotDictionary options) => true;

	public ModeEnum Mode { get; private init; }
	public bool RemoveMargins { get; private init; }

	public ImportOptions(Godot.Collections.Dictionary options)
	{
		this.Mode = (ModeEnum) options["mode"].AsInt32();
		this.RemoveMargins = options["remove_margins"].AsBool();
	}
}
