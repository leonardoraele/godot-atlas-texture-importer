using System;
using System.Collections.Generic;
using Godot;
using GodotDictionary = Godot.Collections.Dictionary;

namespace Raele.AtlasTextureImporter;

[Tool]
public partial class ImportPlugin : EditorImportPlugin
{
	public override string _GetImporterName() => typeof(ImportPlugin).FullName!;
	public override string _GetVisibleName() => "Atlas Texture Importer";
	public override string[] _GetRecognizedExtensions() => [".atlas.json"];
	public override string _GetSaveExtension() => "res";
	public override string _GetResourceType() => "Resource";
	public override int _GetPresetCount() => 1;
	public override string _GetPresetName(int presetIndex) => "Default";
	public override Godot.Collections.Array<GodotDictionary> _GetImportOptions(string path, int presetIndex)
		=> ImportOptions._GetImportOptions(path, presetIndex);
	public override bool _GetOptionVisibility(string path, StringName optionName, GodotDictionary options)
		=> ImportOptions._GetOptionVisibility(path, optionName, options);
	public override int _GetImportOrder() => 200;
	public override float _GetPriority() => 1f;

	public override Error _Import(
		string sourcePath,
		string savePath,
		GodotDictionary optionsDict,
		Godot.Collections.Array<string> platformVariants,
		Godot.Collections.Array<string> genFiles
	)
	{
		GD.PrintS("Importing atlas texture...", new { sourcePath });

		AtlasTextureSourcePair atlasPair = new(sourcePath);
		ImportOptions options = new(optionsDict);

		IEnumerable<string> outputFilepaths = options.Mode switch
		{
			ImportOptions.ModeEnum.ImportAsAtlasTextures => atlasPair.SaveTexturesAsResource(options),
			ImportOptions.ModeEnum.ImportAsPng => atlasPair.SaveTexturesAsPng(options),
			_ => throw new NotImplementedException("Failed to import texture atlas. Cause: Unhandled import mode: " + options.Mode),
		};

		genFiles.AddRange(outputFilepaths);

		GD.PrintS("Import complete. Generated ", genFiles.Count, " files.");

		// Refresh the FileSystem editor interface so the generated files appear to the user. This must be done manually.
		EditorInterface.Singleton.GetResourceFilesystem().ScanSources();

		return ResourceSaver.Save(new Resource(), savePath + "." + this._GetSaveExtension());
	}
}
