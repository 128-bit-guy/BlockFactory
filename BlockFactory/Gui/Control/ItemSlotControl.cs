using System.Drawing;
using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Gui;
using BlockFactory.Item_;
using BlockFactory.Item_.Inventory_;
using BlockFactory.Serialization;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace BlockFactory.Gui.Control;

public class ItemSlotControl : SynchronizedMenuControl
{
    [ExclusiveTo(Side.Client)]
    private const float Padding = 8f;
    [ExclusiveTo(Side.Client)]
    private Box2D<float> _controlBox;

    private readonly IInventory? _inventory;
    private readonly int _slot;
    private ItemStack _stack;

    public ItemSlotControl()
    {
        _stack = new ItemStack();
    }

    public ItemSlotControl(IInventory inv, int slot)
    {
        _inventory = inv;
        _slot = slot;
        _stack = (ItemStack)_inventory[slot].Clone();
    }

    [ExclusiveTo(Side.Client)]
    public override void SetWorkingArea(Box2D<float> box)
    {
        _controlBox = box;
    }

    [ExclusiveTo(Side.Client)]
    public override void UpdateAndRender(float z)
    {
        base.UpdateAndRender(z);
        BfRendering.Matrices.Push();
        BfRendering.Matrices.Translate(0, 0, z);
        var mouseOver = IsMouseOver;
        GuiRenderHelper.RenderQuadWithBorder(7, _controlBox, Padding, 1 / 8.0f);
        BfRendering.Matrices.Translate(_controlBox.Center.X, _controlBox.Center.Y, 4);
        GuiRenderHelper.RenderStack(_stack);
        BfRendering.Matrices.Pop();
    }

    [ExclusiveTo(Side.Client)]
    public override Box2D<float> GetControlBox()
    {
        return _controlBox;
    }

    [ExclusiveTo(Side.Client)]
    public override void MouseDown(MouseButton button)
    {
        base.MouseDown(button);
        if (IsMouseOver)
        {
            if (button == MouseButton.Left)
            {
                DoAction(0);
            } else if (button == MouseButton.Right)
            {
                DoAction(1);
            }
        } 
    }

    protected override void ProcessAction(int action)
    {
        base.ProcessAction(action);
        if(LogicalSide == LogicalSide.Client) return;
        var handInv = SyncMenu.User.MenuHand;
        if (action == 0)
        {
            if (handInv[0].ItemInstance.IsEmpty())
            {
                InventoryUtils.Transfer(_inventory!, _slot, handInv, 0);
            } else if(ItemUtils.CanStack(handInv[0], _inventory![_slot]))
            {
                InventoryUtils.Transfer(handInv, 0, _inventory!, _slot);
            }
            else
            {
                InventoryUtils.Swap(handInv, 0, _inventory!, _slot);
            }
        } else if (action == 1)
        {
            if (handInv[0].ItemInstance.IsEmpty())
            {
                InventoryUtils.Transfer(_inventory!, _slot, handInv, 0, 
                    (_inventory![_slot].Count + 1) / 2);
            } else if(ItemUtils.CanStack(handInv[0], _inventory![_slot]))
            {
                InventoryUtils.Transfer(handInv, 0, _inventory!, _slot, 1);
            }
            else
            {
                InventoryUtils.Swap(handInv, 0, _inventory!, _slot);
            }
        }
    }

    public override SynchronizedControlType Type => SynchronizedControls.ItemSlot;

    public override DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var res = new DictionaryTag();
        res.Set("stack", _stack.SerializeToTag(reason));
        return res;
    }

    public override void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        _stack = new ItemStack();
        _stack.DeserializeFromTag(tag.Get<DictionaryTag>("stack"), reason);
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        if(_inventory == null) return;
        if (!_stack.Equals(_inventory![_slot]))
        {
            _stack = (ItemStack)_inventory[_slot].Clone();
            SendUpdateFromServer();
        }
    }
}