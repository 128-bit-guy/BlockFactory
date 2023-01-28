using BlockFactory.Network;
using BlockFactory.Util;
using OpenTK.Mathematics;
using BlockFactory.Game;

namespace BlockFactory.Gui.Widget;

public class InGameMenuWidget
{
    public readonly InGameMenuWidgetType Type;
    public Box2i InclusiveBox;
    public InGameMenu Menu = null!;
    public int Index = -1;

    public InGameMenuWidget(InGameMenuWidgetType type, Box2i inclusiveBox)
    {
        Type = type;
        InclusiveBox = inclusiveBox;
    }

    public InGameMenuWidget(InGameMenuWidgetType type, BinaryReader reader)
    {
        Type = type;
        InclusiveBox = default;
        InclusiveBox.Min = NetworkUtils.ReadVector2i(reader);
        InclusiveBox.Max = NetworkUtils.ReadVector2i(reader);
    }

    public virtual void Write(BinaryWriter writer)
    {
        InclusiveBox.Min.Write(writer);
        InclusiveBox.Max.Write(writer);
    }

    public virtual void Update()
    {
        
    }

    public virtual void WriteUpdateData(BinaryWriter writer)
    {
    }

    public virtual void ReadUpdateData(BinaryReader reader)
    {
        
    }

    protected void SendUpdate()
    {
        if (Menu.Owner.GameInstance!.Kind.IsNetworked() && Menu.Owner.GameInstance!.Kind.DoesProcessLogic())
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);
            WriteUpdateData(writer);
            Menu.Owner.GameInstance.NetworkHandler.GetPlayerConnection(Menu.Owner).SendPacket(
                new WidgetUpdatePacket(Index, stream.ToArray()));
        }
    }

    public void ProcessAction(int actionNumber)
    {
        if (Menu.Owner.GameInstance!.Kind.DoesProcessLogic())
        {
            OnAction(actionNumber);
        } else if (Menu.Owner.GameInstance!.Kind.IsNetworked())
        {
            SendAction(actionNumber);
        }
    }

    protected void SendAction(int actionNumber)
    {
        Menu.Owner.GameInstance!.NetworkHandler.GetServerConnection().SendPacket(
            new WidgetActionPacket(Index, actionNumber));
    }

    protected virtual void OnAction(int actionNumber)
    {
        
    }
}