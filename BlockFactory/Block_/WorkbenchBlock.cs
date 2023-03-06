using BlockFactory.Block_.Instance;

namespace BlockFactory.Block_;

public class WorkbenchBlock : Block
{
    public override BlockInstance? CreateInstance()
    {
        return new WorkbenchInstance();
    }
}