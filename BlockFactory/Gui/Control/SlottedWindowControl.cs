using BlockFactory.Base;
using BlockFactory.Serialization;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace BlockFactory.Gui.Control;

public class SlottedWindowControl : WindowControl
{
    [ExclusiveTo(Side.Client)] private const float SlotSize = 64;
    [ExclusiveTo(Side.Client)] private const float NormalMargin = 5;
    [ExclusiveTo(Side.Client)] private const float DivisionMargin = 24;

    private readonly List<(MenuControl control, Box2D<int> slots)> _children = new();
    private int[] _divisionsX;
    private int[] _divisionsY;
    private Vector2D<int> _slotCounts;

    public SlottedWindowControl(Vector2D<int> slotCounts, int[] divisionsX, int[] divisionsY)
    {
        _slotCounts = slotCounts;
        _divisionsX = divisionsX;
        _divisionsY = divisionsY;
        Array.Sort(_divisionsX);
        Array.Sort(_divisionsY);
    }

    [ExclusiveTo(Side.Client)]
    private Vector2D<float> GetMinPosForSlot(Box2D<float> contentBox, Vector2D<int> slot)
    {
        var divCntX = 0;
        while (divCntX < _divisionsX.Length && _divisionsX[divCntX] < slot.X) ++divCntX;

        var divCntY = 0;
        while (divCntY < _divisionsY.Length && _divisionsY[divCntY] < slot.Y) ++divCntY;

        return contentBox.Min + slot.As<float>() * (SlotSize + NormalMargin) +
               new Vector2D<float>(divCntX, divCntY) * (DivisionMargin - NormalMargin);
    }

    [ExclusiveTo(Side.Client)]
    private Vector2D<float> GetMaxPosForSlot(Box2D<float> contentBox, Vector2D<int> slot)
    {
        return GetMinPosForSlot(contentBox, slot) + new Vector2D<float>(SlotSize);
    }

    public SlottedWindowControl With(Box2D<int> slots, MenuControl control)
    {
        _children.Add((control, slots));
        control.Parent = this;
        return this;
    }

    public SlottedWindowControl With(Vector2D<int> pos, MenuControl control)
    {
        return With(new Box2D<int>(pos, pos), control);
    }

    public SlottedWindowControl With(int minX, int minY, int maxX, int maxY, MenuControl control)
    {
        return With(new Box2D<int>(minX, minY, maxX, maxY), control);
    }

    public SlottedWindowControl With(int x, int y, MenuControl control)
    {
        return With(new Vector2D<int>(x, y), control);
    }

    [ExclusiveTo(Side.Client)]
    protected override Vector2D<float> GetSize()
    {
        return _slotCounts.As<float>() * (SlotSize + NormalMargin) +
               new Vector2D<float>(_divisionsX.Length, _divisionsY.Length) * (DivisionMargin - NormalMargin)
               - new Vector2D<float>(NormalMargin);
    }

    [ExclusiveTo(Side.Client)]
    protected override void SetChildrenAreas(Box2D<float> contentBox)
    {
        foreach (var (control, slots) in _children)
        {
            var min = GetMinPosForSlot(contentBox, slots.Min);
            var max = GetMaxPosForSlot(contentBox, slots.Max);
            control.SetWorkingArea(new Box2D<float>(min, max));
        }
    }

    [ExclusiveTo(Side.Client)]
    protected override void UpdateAndRenderChildren(float z)
    {
        foreach (var (control, _) in _children) control.UpdateAndRender(z);
    }

    [ExclusiveTo(Side.Client)]
    public override void MouseDown(MouseButton button)
    {
        base.MouseDown(button);
        foreach (var (control, _) in _children) control.MouseDown(button);
    }

    [ExclusiveTo(Side.Client)]
    public override void MouseUp(MouseButton button)
    {
        base.MouseUp(button);
        foreach (var (control, _) in _children) control.MouseUp(button);
    }

    [ExclusiveTo(Side.Client)]
    public override void KeyDown(Key key, int a)
    {
        base.KeyDown(key, a);
        foreach (var (control, _) in _children) control.KeyDown(key, a);
    }

    [ExclusiveTo(Side.Client)]
    public override void KeyChar(char c)
    {
        base.KeyChar(c);
        foreach (var (control, _) in _children) control.KeyChar(c);
    }

    public override DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var res = new DictionaryTag();
        res.SetVector2D("slot_counts", _slotCounts);
        res.SetValue("divisions_x", _divisionsX);
        res.SetValue("divisions_y", _divisionsY);
        if (reason == SerializationReason.NetworkUpdate) return res;

        var children = new ListTag(_children.Count, TagType.Dictionary);
        for (var i = 0; i < _children.Count; ++i)
        {
            var control = (SynchronizedMenuControl)_children[i].control;
            var childTag = new DictionaryTag();
            childTag.SetVector2D("min", _children[i].slots.Min);
            childTag.SetVector2D("max", _children[i].slots.Max);
            childTag.SetValue("type", control.Type.Id);
            childTag.Set("tag", control.SerializeToTag(reason));
            children.Set(i, childTag);
        }

        res.Set("children", children);

        return res;
    }

    public override void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        _slotCounts = tag.GetVector2D<int>("slot_counts");
        _divisionsX = tag.GetValue<int[]>("divisions_x");
        _divisionsY = tag.GetValue<int[]>("divisions_y");

        if (reason == SerializationReason.NetworkUpdate) return;

        var children = tag.Get<ListTag>("children");
        _children.Clear();
        foreach (var childTag in children.GetEnumerable<DictionaryTag>())
        {
            var min = childTag.GetVector2D<int>("min");
            var max = childTag.GetVector2D<int>("max");
            var type = SynchronizedControls.Registry[childTag.GetValue<int>("type")];
            var control = type!.Creator();
            control.DeserializeFromTag(childTag.Get<DictionaryTag>("tag"), reason);
            With(new Box2D<int>(min, max), control);
        } 
    }

    public override int GetChildIndex(MenuControl control)
    {
        return _children.FindIndex(c => c.control == control);
    }

    public override MenuControl? GetChild(int index)
    {
        return _children[index].control;
    }

    public override SynchronizedControlType Type => SynchronizedControls.SlottedWindow;
}