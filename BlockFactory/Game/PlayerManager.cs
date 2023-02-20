using BlockFactory.Client.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Serialization.Serializable;
using BlockFactory.Serialization.Tag;
using BlockFactory.Server.Entity_;
using BlockFactory.Side_;

namespace BlockFactory.Game;

public class PlayerManager : ITagSerializable
{
    private readonly GameInstance _game;
    private readonly Dictionary<string, PlayerInfo> _playerInfos;

    public PlayerManager(GameInstance game)
    {
        _playerInfos = new Dictionary<string, PlayerInfo>();
        _game = game;
    }

    public DictionaryTag SerializeToTag()
    {
        var res = new DictionaryTag();
        var tag = new ListTag(0, TagType.Dictionary);
        foreach (var info in _playerInfos.Values) tag.Add(((ITagSerializable)info.Player).SerializeToTag());

        res.Set("players", tag);
        return res;
    }

    public void DeserializeFromTag(DictionaryTag tag)
    {
        _playerInfos.Clear();
        var list = tag.Get<ListTag>("players");
        foreach (var infoTag in list.GetEnumerable<DictionaryTag>())
        {
            var p = NewPlayer(new PlayerInfo(new Credentials()));
            ((ITagSerializable)p).DeserializeFromTag(infoTag);
            _playerInfos.Add(p.PlayerInfo!.Credentials.Name, p.PlayerInfo);
        }
    }

    private PlayerEntity NewPlayer(PlayerInfo info)
    {
        if (_game.Kind.GetPhysicalSide() == Side.Server)
            return new ServerPlayerEntity(info);
        return new ClientPlayerEntity(info);
    }

    public PlayerEntity? GetOrCreatePlayer(Credentials c, out bool created)
    {
        if (_playerInfos.TryGetValue(c.Name, out var info))
        {
            created = false;
            return c.Password == info.Credentials.Password || _game.Kind == GameKind.Singleplayer
                ? info.Player
                : null;
        }

        if (_game.Kind == GameKind.Singleplayer)
        {
            var c2 = new Credentials
            {
                Name = c.Name,
                Password = "Admin"
            };
            c = c2;
        }
        info = new PlayerInfo(c);
        var p = NewPlayer(info);
        _playerInfos.Add(c.Name, info);
        created = true;
        return p;
    }
}