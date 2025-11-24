#if TOOLS

namespace Raele.AtlasTextureImporter;

[Godot.Tool]
public partial class Plugin : Godot.EditorPlugin
{
	private ImportPlugin importPlugin => field != null ? field : field = new ImportPlugin();

	public override void _EnterTree()
	{
		this.AddImportPlugin(importPlugin);
	}

	public override void _ExitTree()
	{
		this.RemoveImportPlugin(importPlugin);
	}
}
#endif
