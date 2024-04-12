using BlockFactory.Client;
using BlockFactory.Content.Gui.Menu_;
using BlockFactory.Network.Packet_;
using BlockFactory.Serialization;

namespace BlockFactory.Content.Gui.Control;

public abstract class SynchronizedMenuControl : MenuControl, ITagSerializable
{
    private LogicalSide _logicalSide;
    private SynchronizedMenu? _menu;
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

    public SynchronizedMenu SyncMenu
    {
        get
        {
            if (_menu != null)
            {
                return _menu;
            }

            if (ParentMenu != null)
            {
                return _menu = (SynchronizedMenu)ParentMenu;
            }

            return _menu = ((SynchronizedMenuControl)Parent!).SyncMenu;
        }
    }

    public abstract DictionaryTag SerializeToTag(SerializationReason reason);
    public abstract void DeserializeFromTag(DictionaryTag tag, SerializationReason reason);

    public virtual int GetChildIndex(MenuControl control)
    {
        return -1;
    }

    public virtual MenuControl? GetChild(int index)
    {
        return null;
    }

    public List<int> GetPathFromMenuManager()
    {
        var res = new List<int>();
        var cur = this;
        while (cur.Parent != null)
        {
            res.Add(((SynchronizedMenuControl)cur.Parent).GetChildIndex(cur));
            cur = (SynchronizedMenuControl)cur.Parent!;
        }

        res.Add(cur.ParentMenu!.MenuManager.GetMenuIndex(cur.ParentMenu));
        res.Reverse();
        return res;
    }

    public void DoAction(int action)
    {
        ProcessAction(action);
        if (LogicalSide == LogicalSide.Client)
        {
            BlockFactoryClient.LogicProcessor!.NetworkHandler.SendPacket(
                null,
                new ControlActionPacket(this, action)
                );
        }
    }

    protected virtual void ProcessAction(int action)
    {
        
    }

    public virtual void UpdateLogic()
    {
        
    }

    public void SendUpdateFromServer()
    {
        if (LogicalSide == LogicalSide.Server)
        {
            SyncMenu.User.World!.LogicProcessor.NetworkHandler.SendPacket(SyncMenu.User, new ControlUpdatePacket(this));
        }
    }
}