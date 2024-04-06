using BlockFactory.Gui.Menu_;
using BlockFactory.Serialization;

namespace BlockFactory.Gui.Control;

public abstract class SynchronizedMenuControl : MenuControl, ITagSerializable
{
    private LogicalSide _logicalSide;
    private bool _logicalSideChecked;

    public abstract SynchronizedControlType Type { get; }

    public LogicalSide LogicalSide
    {
        get
        {
            if (_logicalSideChecked)
            {
                return _logicalSide;
            }

            _logicalSideChecked = true;
            if (Parent != null)
            {
                if (Parent is SynchronizedMenuControl control)
                {
                    return _logicalSide = control.LogicalSide;
                }

                return _logicalSide = LogicalSide.SinglePlayer;
            }

            if (ParentMenu is SynchronizedMenu menu)
            {
                return _logicalSide = menu.User.World!.LogicProcessor.LogicalSide;
            }

            return _logicalSide = LogicalSide.SinglePlayer;
        }
    }

    public abstract DictionaryTag SerializeToTag(SerializationReason reason);
    public abstract void DeserializeFromTag(DictionaryTag tag, SerializationReason reason);
}