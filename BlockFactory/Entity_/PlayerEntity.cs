using BlockFactory.World_;

namespace BlockFactory.Entity_;

public class PlayerEntity : Entity
{
    public PlayerChunkLoader? ChunkLoader { get; private set; }

    public override void Update()
    {
        base.Update();
        ChunkLoader!.Update();
    }

    protected override void OnRemovedFromWorld()
    {
        ChunkLoader!.Dispose();
        ChunkLoader = null;
        base.OnRemovedFromWorld();
    }

    protected override void OnAddedToWorld()
    {
        base.OnAddedToWorld();
        ChunkLoader = new PlayerChunkLoader(this);
    }
}