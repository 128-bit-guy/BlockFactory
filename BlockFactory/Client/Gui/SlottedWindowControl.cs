﻿using Silk.NET.Maths;

namespace BlockFactory.Client.Gui;

public class SlottedWindowControl : WindowControl
{
    private const float SlotSize = 64;
    private const float NormalMargin = 5;
    private const float DivisionMargin = 24;
    private Vector2D<int> _slotCounts;
    private readonly int[] _divisionsX;
    private readonly int[] _divisionsY;

    private readonly List<(IMenuControl control, Box2D<int> slots)> _children = new();

    public SlottedWindowControl(Vector2D<int> slotCounts, int[] divisionsX, int[] divisionsY)
    {
        _slotCounts = slotCounts;
        _divisionsX = divisionsX;
        _divisionsY = divisionsY;
        Array.Sort(_divisionsX);
        Array.Sort(_divisionsY);
    }

    private Vector2D<float> GetMinPosForSlot(Box2D<float> contentBox, Vector2D<int> slot)
    {
        var divCntX = 0;
        while (divCntX < _divisionsX.Length && _divisionsX[divCntX] < slot.X)
        {
            ++divCntX;
        }
        var divCntY = 0;
        while (divCntY < _divisionsY.Length && _divisionsY[divCntX] < slot.Y)
        {
            ++divCntY;
        }

        return contentBox.Min + slot.As<float>() * (SlotSize + NormalMargin) +
               new Vector2D<float>(divCntX, divCntY) * (DivisionMargin - NormalMargin);
    }

    private Vector2D<float> GetMaxPosForSlot(Box2D<float> contentBox, Vector2D<int> slot)
    {
        return GetMinPosForSlot(contentBox, slot) + new Vector2D<float>(SlotSize);
    }

    public SlottedWindowControl With(Box2D<int> slots, IMenuControl control)
    {
        _children.Add((control, slots));
        return this;
    }

    protected override Vector2D<float> GetSize()
    {
        return _slotCounts.As<float>() * (SlotSize + NormalMargin) +
               new Vector2D<float>(_divisionsX.Length, _divisionsY.Length) * (DivisionMargin - NormalMargin)
               - new Vector2D<float>(NormalMargin);
    }

    protected override void SetChildrenAreas(Box2D<float> contentBox)
    {
        foreach (var (control, slots) in _children)
        {
            var min = GetMinPosForSlot(contentBox, slots.Min);
            var max = GetMaxPosForSlot(contentBox, slots.Max);
            control.SetWorkingArea(new Box2D<float>(min, max));
        }
    }

    protected override void UpdateAndRenderChildren(float z)
    {
        foreach (var (control, _) in _children)
        {
            control.UpdateAndRender(z);
        }
    }
}