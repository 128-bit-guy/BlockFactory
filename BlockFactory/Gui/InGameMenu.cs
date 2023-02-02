using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Gui.Widget;
using BlockFactory.Init;
using BlockFactory.Inventory_;
using BlockFactory.Item_;
using BlockFactory.Network;
using BlockFactory.Util;
using OpenTK.Mathematics;

namespace BlockFactory.Gui;

public abstract class InGameMenu
{
    public readonly SimpleInventory MovedStackInventory;
    public readonly InGameMenuType Type;
    public PlayerEntity Owner = null!;
    public List<InGameMenuWidget> Widgets = new();

    public InGameMenu(InGameMenuType type, BinaryReader reader)
    {
        Type = type;
        Size = NetworkUtils.ReadVector2i(reader);
        var cnt = reader.Read7BitEncodedInt();
        for (var i = 0; i < cnt; ++i)
        {
            var id = reader.Read7BitEncodedInt();
            AddWidget(InGameMenuWidgetTypes.Registry[id].Loader(reader));
        }

        MovedStackInventory = new SimpleInventory(1);
        MovedStackInventory.OnSlotContentChanged += OnMovedStackSlotContentChanged;
    }

    public InGameMenu(InGameMenuType type)
    {
        Type = type;
        MovedStackInventory = new SimpleInventory(1);
        MovedStackInventory.OnSlotContentChanged += OnMovedStackSlotContentChanged;
    }

    public Vector2i Size { get; protected init; }

    public void AddWidget(InGameMenuWidget widget)
    {
        widget.Index = Widgets.Count;
        widget.Menu = this;
        Widgets.Add(widget);
    }

    public void Write(BinaryWriter writer)
    {
        Size.Write(writer);
        writer.Write7BitEncodedInt(Widgets.Count);
        foreach (var widget in Widgets)
        {
            writer.Write7BitEncodedInt(widget.Type.Id);
            widget.Write(writer);
        }
    }

    public virtual void OnOpen()
    {
    }

    public virtual void OnClose()
    {
    }

    public virtual void Update()
    {
        foreach (var widget in Widgets) widget.Update();
    }

    public virtual void WriteUpdateData(BinaryWriter writer)
    {
        MovedStackInventory[0].Write(writer);
    }

    public virtual void ReadUpdateData(BinaryReader reader)
    {
        MovedStackInventory[0] = new ItemStack(reader);
    }

    protected void SendUpdate()
    {
        if (Owner.GameInstance!.Kind.IsNetworked() && Owner.GameInstance!.Kind.DoesProcessLogic())
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            WriteUpdateData(writer);
            Owner.GameInstance.NetworkHandler.GetPlayerConnection(Owner).SendPacket(
                new WidgetUpdatePacket(Widgets.Count, stream.ToArray()));
        }
    }

    private void OnMovedStackSlotContentChanged(int slot, ItemStack previous, ItemStack current)
    {
        SendUpdate();
    }
}