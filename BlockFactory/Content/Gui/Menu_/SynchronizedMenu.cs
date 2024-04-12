using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Gui;
using BlockFactory.Content.Entity_;
using BlockFactory.Content.Gui.Control;
using BlockFactory.Network.Packet_;
using BlockFactory.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.Content.Gui.Menu_;

public class SynchronizedMenu : Menu, ITagSerializable
{
    public readonly PlayerEntity User;

    public SynchronizedMenu(PlayerEntity user)
    {
        User = user;
    }

    public override void EscapePressed()
    {
        if (User.World!.LogicProcessor.LogicalSide == LogicalSide.Client)
        {
            User.World.LogicProcessor.NetworkHandler.SendPacket(null, new CloseMenuRequestPacket());
            return;
        }
        base.EscapePressed();
    }

    public DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var res = new DictionaryTag();
        if (Root != null && reason != SerializationReason.NetworkUpdate)
        {
            res.SetValue("root_type", ((SynchronizedMenuControl)Root).Type.Id);
            res.Set("root_tag", ((SynchronizedMenuControl)Root).SerializeToTag(reason));
        }

        return res;
    }

    public void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        if(reason == SerializationReason.NetworkUpdate){ return;}
        if (tag.Keys.Contains("root_type"))
        {
            var type = SynchronizedControls.Registry[tag.GetValue<int>("root_type")];
            Root = type!.Creator();
            ((SynchronizedMenuControl)Root).DeserializeFromTag(tag.Get<DictionaryTag>("root_tag"), reason);
        }
        else
        {
            Root = null;
        }
    }

    [ExclusiveTo(Side.Client)]
    public override void UpdateAndRender(Box2D<float> workingArea)
    {
        base.UpdateAndRender(workingArea);
        var mousePos = BlockFactoryClient.InputContext.Mice[0].Position;
        BfRendering.Matrices.Push();
        BfRendering.Matrices.Translate(mousePos.X, mousePos.Y, 10f);
        GuiRenderHelper.RenderStack(User.StackInMenuHand);
        BfRendering.Matrices.Pop();
    }

    public virtual void UpdateLogic()
    {
        ((SynchronizedMenuControl?)Root)?.UpdateLogic();
    }
}