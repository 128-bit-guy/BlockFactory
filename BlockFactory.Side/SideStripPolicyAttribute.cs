namespace BlockFactory.Side;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
public class SideStripPolicyAttribute : Attribute
{
    public readonly SideStripPolicy Policy;

    public SideStripPolicyAttribute(SideStripPolicy policy)
    {
        Policy = policy;
    }
}