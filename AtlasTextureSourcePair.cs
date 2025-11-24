using System;
using System.IO;
using System.Linq;
using Godot;

namespace Raele.AtlasTextureImporter;

public class AtlasTextureSourcePair
{
	public string AtlasFilepath { get; private set; }
	public string ImageFilepath => field ??= Path.GetRelativePath(
		this.AtlasFilepath.GetBaseDir(),
		this.AtlasData["meta"].AsGodotDictionary()["image"].AsString()
	);
	public string OutputDirPath => this.ImageFilepath.GetBaseName();
	public string OutputDirName => this.OutputDirPath.GetFile();
	public string SourcePairDirPath => this.ImageFilepath.GetBaseDir();
	public Godot.Collections.Dictionary AtlasData
		=> field ??= Json.ParseString(this.ReadAtlasFile()).AsGodotDictionary();
	public Frame[] Frames
		=> field ??= (
			this.AtlasData["frames"].VariantType == Variant.Type.Dictionary
				? this.AtlasData["frames"].AsGodotDictionary()
					.Keys
					.Select(key => new Frame(this.AtlasData["frames"].AsGodotDictionary()[key], key.AsString()))
					.ToArray()
			: this.AtlasData["frames"].VariantType == Variant.Type.Array
				? this.AtlasData["frames"].AsGodotArray()
					.Select(frame => new Frame(frame))
					.ToArray()
			: throw new Exception("Invalid frames format in atlas JSON.")
		);
	public Texture2D SourceTexture => field ??= ResourceLoader.Load<Texture2D>(this.ImageFilepath, "Texture2D", ResourceLoader.CacheMode.Ignore);

	private DirAccess SourcePairDirAccess => field ??= DirAccess.Open(this.SourcePairDirPath);

	public AtlasTextureSourcePair(string filepath)
	{
		this.AtlasFilepath = filepath;
	}

	private string ReadAtlasFile()
	{
		Godot.FileAccess file = Godot.FileAccess.Open(this.AtlasFilepath, Godot.FileAccess.ModeFlags.Read);
		string contents = file.GetAsText();
		file.Close();
		return contents;
	}

	public (AtlasTexture, string)[] GenerateAtlasTextures()
		=> this.Frames.Select(this.GenerateAtlasTexture).ToArray();

	private (AtlasTexture, string) GenerateAtlasTexture(Frame frame)
	{
		this.AssertOutputDirExists();
		string outputFilepath = frame.BuildOutputFilepath(this.OutputDirPath);
		GD.PrintS("Generating texture...", new { outputFilepath });
		AtlasTexture atlas = this.BuildAtlasTexture(frame);
		Error error = ResourceSaver.Save(atlas, outputFilepath);
		if (error != Error.Ok)
		{
			GD.PrintErr("Failed to save AtlasTexture.", new { outputFilepath, error });
		}
		return (atlas, outputFilepath);
	}

	private void AssertOutputDirExists()
	{
		if (!this.SourcePairDirAccess.DirExists(this.OutputDirName))
		{
			Error error = this.SourcePairDirAccess.MakeDir(this.OutputDirName);
			if (error != Error.Ok)
			{
				throw new Exception($"Failed to create output directory. {new { this.SourcePairDirPath, this.OutputDirName, error }}");
			}
		}
	}

	private AtlasTexture BuildAtlasTexture(Frame frame)
		=> frame.BuildAtlasTexture(this.SourceTexture);
}
