using BlockFactory.Serialization.Automatic.Serializable;

namespace BlockFactory.Game;

public class Credentials : AutoSerializable
{
    public string Name = string.Empty;
    public string Password = string.Empty;
}