using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

namespace Raele.AtlasTextureImporter;

public class AtlasTextureSourcePair
{
	public string AtlasFilepath { get; private set; }
	public string ImageFilepath => field ??= Path.Combine(
		this.AtlasFilepath.GetBaseDir(),
		this.AtlasData["meta"].AsGodotDictionary()["image"].AsString()
	);
	public string OutputDirName => this.AtlasFilepath.GetFile().GetBaseName().GetBaseName() + ".sprites";
	public string SourcePairDirPath => this.AtlasFilepath.GetBaseDir();
	public string OutputDirPath => Path.Combine(this.SourcePairDirPath, this.OutputDirName);
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
	public Texture2D SourceTexture => field ??= ResourceLoader.Load<Texture2D>(this.ImageFilepath, "Texture2D", ResourceLoader.CacheMode.Reuse);
	private DirAccess SourcePairDirAccess => field ??= DirAccess.Open(this.SourcePairDirPath);
	private DirAccess OutputDirAccess => field ??= DirAccess.Open(this.OutputDirPath);

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

	public IEnumerable<string> SaveTexturesAsPng()
	{
		this.PrepareOutputDir();
		foreach (var frame in this.Frames)
		{
			AtlasTexture atlas = this.BuildAtlasTexture(frame);
			yield return this.SaveTextureAsPng(frame, atlas);
		}
	}

	private string SaveTextureAsPng(Frame frame, AtlasTexture atlas)
	{
		string outputFilepath = Path.Combine(this.OutputDirPath, $"{frame.Filename}.png");
		GD.PrintS("Generating texture...", new { outputFilepath });
		Error error = atlas.GetImage().SavePng(outputFilepath);
		if (error != Error.Ok)
		{
			GD.PrintErr("Failed to save AtlasTexture.", new { outputFilepath, error });
		}
		return outputFilepath;
	}

	public IEnumerable<string> SaveTexturesAsResource()
	{
		this.PrepareOutputDir();
		foreach (var frame in this.Frames)
		{
			AtlasTexture atlas = this.BuildAtlasTexture(frame);
			yield return this.SaveTextureAsResource(frame, atlas);
		}
	}

	private string SaveTextureAsResource(Frame frame, AtlasTexture atlas)
	{
		string outputFilepath = Path.Combine(this.OutputDirPath, $"{frame.Filename}.tres");
		GD.PrintS("Generating texture...", new { outputFilepath });
		Error error = ResourceSaver.Save(atlas, outputFilepath);
		if (error != Error.Ok)
		{
			GD.PrintErr("Failed to save PNG sprite.", new { outputFilepath, error });
		}
		return outputFilepath;
	}

	private AtlasTexture BuildAtlasTexture(Frame frame)
		=> new AtlasTexture()
		{
			Atlas = this.SourceTexture,
			Region = frame.FrameRect,
			Margin = frame.Margin,
		};

	private void PrepareOutputDir()
	{
		if (this.SourcePairDirAccess.DirExists(this.OutputDirName))
		{
			// Delete all existing files in the output directory
			try
			{
				this.OutputDirAccess.ListDirBegin();
				for (string? fileName; !string.IsNullOrEmpty(fileName = this.OutputDirAccess.GetNext());)
				{
					Error error = this.OutputDirAccess.Remove(fileName);
					if (error != Error.Ok)
					{
						throw new Exception($"Failed to delete existing file in output directory. {new { this.OutputDirAccess, fileName, error }}");
					}
				}
			}
			finally
			{
				this.OutputDirAccess.ListDirEnd();
			}
		}
		else
		{
			Error error = this.SourcePairDirAccess.MakeDir(this.OutputDirName);
			if (error != Error.Ok)
			{
				throw new Exception($"Failed to create output directory. {new { this.SourcePairDirPath, this.OutputDirName, error }}");
			}
		}
	}
}
