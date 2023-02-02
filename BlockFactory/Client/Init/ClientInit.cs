using BlockFactory.Client.Gui.InGame;
using BlockFactory.Client.Gui.InGame.Widget_;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Block_;
using BlockFactory.Client.Render.Mesh;
using BlockFactory.Client.Render.Shader;
using BlockFactory.Side_;

namespace BlockFactory.Client.Init;

[ExclusiveTo(Side.Client)]
public class ClientInit
{
    public static void Init()
    {
        PacketHandlers.Init();
        BlockMeshing.Init();
        Shaders.Init();
        VertexFormats.Init();
        Textures.Init();
        InGameMenuClientWidgets.Init();
        InGameMenuScreens.Init();
    }
}