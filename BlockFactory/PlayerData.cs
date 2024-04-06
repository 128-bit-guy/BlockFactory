using BlockFactory.Entity_;
using BlockFactory.Serialization;

namespace BlockFactory;

public class PlayerData : ITagSerializable
{
    private readonly LogicProcessor _logicProcessor;
    private readonly Dictionary<string, string> _passwords = new();
    public readonly Dictionary<string, PlayerEntity> Players = new();

    public PlayerData(LogicProcessor logicProcessor)
    {
        _logicProcessor = logicProcessor;
    }

    public DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var res = new DictionaryTag();
        var passwords = new DictionaryTag();
        foreach (var (name, password) in _passwords) passwords.SetValue(name, password);
        res.Set("passwords", passwords);
        var players = new DictionaryTag();
        foreach (var (name, player) in Players) players.Set(name, player.SerializeToTag(reason));
        res.Set("players", players);
        return res;
    }

    public void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        var passwords = tag.Get<DictionaryTag>("passwords");
        _passwords.Clear();
        foreach (var name in passwords.Keys) _passwords.Add(name, passwords.GetValue<string>(name));

        var players = tag.Get<DictionaryTag>("players");
        Players.Clear();
        foreach (var name in players.Keys)
        {
            var playerTag = players.Get<DictionaryTag>(name);
            PlayerEntity player = _logicProcessor.LogicalSide == LogicalSide.Server
                ? new ServerPlayerEntity()
                : new ClientPlayerEntity();
            player.DeserializeFromTag(playerTag, reason);
            Players.Add(name, player);
        }
    }

    public bool AttemptLogin(Credentials credentials)
    {
        if (_passwords.TryGetValue(credentials.Name, out var savedPassword))
            return savedPassword == credentials.Password;

        _passwords.Add(credentials.Name, credentials.Password);
        return true;
    }
}