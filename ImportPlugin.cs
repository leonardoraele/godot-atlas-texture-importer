using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

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
	public override Array<Dictionary> _GetImportOptions(string path, int presetIndex) => [];
	public override bool _GetOptionVisibility(string path, StringName optionName, Dictionary options) => true;
	public override int _GetImportOrder() => 200;
	public override float _GetPriority() => 1f;

	public override Error _Import(string sourcePath, string savePath, Dictionary options, Array<string> platformVariants, Array<string> genFiles)
	{
		GD.PrintS("Importing atlas texture: ", sourcePath);

		AtlasTextureSourcePair atlasPair = new(sourcePath.GetBaseName().GetBaseName());
		IEnumerable<string> outputFiles = atlasPair.GenerateAtlasTextures().Select(pair => pair.Item2);
		genFiles.AddRange(outputFiles);

		GD.PrintS("Generated ", genFiles.Count, " atlas textures.");

		EditorInterface.Singleton.Call("get_resource_filesystem").AsGodotObject().Call("scan_sources");

		return ResourceSaver.Save(new Resource(), savePath + "." + this._GetSaveExtension());
	}
}
