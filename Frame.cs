using Godot;

namespace Raele.AtlasTextureImporter;

public partial record Frame
{
	public string Filename;
	public Rect2 FrameRect;
	public bool Rotated;
	public bool Trimmed;
	public Rect2 SpriteSourceSize;
	public Vector2I SourceSize;
	public Rect2 Margin => new Rect2(this.SpriteSourceSize.Position, this.SpriteSourceSize.Size - this.FrameRect.Size);

	public Frame(Variant data, string? filename = null)
	{
		var dataDict = data.AsGodotDictionary();
		this.Filename = filename ?? dataDict["filename"].ToString();
		var frameDict = dataDict["frame"].AsGodotDictionary();
		this.FrameRect = new Rect2(
			(float) frameDict["x"].AsDouble(),
			(float) frameDict["y"].AsDouble(),
			(float) frameDict["w"].AsDouble(),
			(float) frameDict["h"].AsDouble()
		);
		this.Rotated = dataDict["rotated"].AsBool();
		this.Trimmed = dataDict["trimmed"].AsBool();
		var spriteSourceSizeDict = dataDict["spriteSourceSize"].AsGodotDictionary();
		this.SpriteSourceSize = new Rect2(
			(float) spriteSourceSizeDict["x"].AsDouble(),
			(float) spriteSourceSizeDict["y"].AsDouble(),
			(float) spriteSourceSizeDict["w"].AsDouble(),
			(float) spriteSourceSizeDict["h"].AsDouble()
		);
		var sourceSizeDict = dataDict["sourceSize"].AsGodotDictionary();
		this.SourceSize = new Vector2I(sourceSizeDict["w"].AsInt32(), sourceSizeDict["h"].AsInt32());
	}
}
