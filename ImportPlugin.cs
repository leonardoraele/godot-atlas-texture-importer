using System.Collections.Generic;
using System.Linq;
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
	public override Godot.Collections.Array<GodotDictionary> _GetImportOptions(string path, int presetIndex) => [
		new GodotDictionary
		{
			{ "name", "save_as_png" },
			{ "default_value", false },
			// { "property_hint", PropertyHint.None },
			// { "hint_string", "" },
			// { "usage", PropertyUsageFlags.Default },
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
		GD.PrintS("Importing atlas texture: ", sourcePath);

		AtlasTextureSourcePair atlasPair = new(sourcePath);

		IEnumerable<string> outputFilepaths = options["save_as_png"].AsBool()
			? atlasPair.SaveTexturesAsPng()
			: atlasPair.SaveTexturesAsResource();

		genFiles.AddRange(outputFilepaths);

		GD.PrintS("Generated ", genFiles.Count, " atlas textures.");

		EditorInterface.Singleton.Call("get_resource_filesystem").AsGodotObject().Call("scan_sources");

		return ResourceSaver.Save(new Resource(), savePath + "." + this._GetSaveExtension());
	}
}
