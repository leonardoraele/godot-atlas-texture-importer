using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotDictionary = Godot.Collections.Dictionary;

namespace Raele.AtlasTextureImporter;

[Tool]
public partial class ImportPlugin : EditorImportPlugin
{
	private enum ModeEnum : int
	{
		ImportAsAtlasTextures = 0,
		ImportAsPng = 1,
	}
	public override string _GetImporterName() => typeof(ImportPlugin).FullName!;
	public override string _GetVisibleName() => "Atlas Texture Importer";
	public override string[] _GetRecognizedExtensions() => [".atlas.json"];
	public override string _GetSaveExtension() => "res";
	public override string _GetResourceType() => "Resource";
	public override int _GetPresetCount() => 1;
	public override string _GetPresetName(int presetIndex) => "Default";
	public override Godot.Collections.Array<GodotDictionary> _GetImportOptions(string path, int presetIndex) => [
		new GodotDictionary
		{
			{ "name", "mode" },
			{ "default_value", (long) ModeEnum.ImportAsAtlasTextures },
			{ "property_hint", (long) PropertyHint.Enum },
			{ "hint_string", Enum.GetValues<ModeEnum>().Select(value => $"{value}:{Enum.Format(typeof(ModeEnum), value, "d")}").ToArray().Join(",") },
			{ "usage", (int) PropertyUsageFlags.Default | (int) PropertyUsageFlags.UpdateAllIfModified },
		}
	];
	public override bool _GetOptionVisibility(string path, StringName optionName, GodotDictionary options) => true;
	public override int _GetImportOrder() => 200;
	public override float _GetPriority() => 1f;

	public override Error _Import(
		string sourcePath,
		string savePath,
		GodotDictionary options,
		Godot.Collections.Array<string> platformVariants,
		Godot.Collections.Array<string> genFiles
	)
	{
		GD.PrintS("Importing atlas texture...", new { sourcePath });

		AtlasTextureSourcePair atlasPair = new(sourcePath);

		IEnumerable<string> outputFilepaths = (ModeEnum) options["mode"].AsInt32() switch
		{
			ModeEnum.ImportAsAtlasTextures => atlasPair.SaveTexturesAsResource(),
			ModeEnum.ImportAsPng => atlasPair.SaveTexturesAsPng(),
			_ => throw new NotImplementedException("Failed to import texture atlas. Cause: Unhandled import mode: " + options["mode"]),
		};

		genFiles.AddRange(outputFilepaths);

		GD.PrintS("Import complete. Generated ", genFiles.Count, " files.");

		// Refresh the FileSystem editor interface so the generated files appear to the user. This must be done manually.
		EditorInterface.Singleton.GetResourceFilesystem().ScanSources();

		return ResourceSaver.Save(new Resource(), savePath + "." + this._GetSaveExtension());
	}
}
