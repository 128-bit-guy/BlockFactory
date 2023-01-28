using System.Collections.Concurrent;
using BlockFactory.Network;
using BlockFactory.World_;

namespace BlockFactory.Game
{
    public class GameInstance : IDisposable
    {
        public readonly GameKind Kind;
        private DateTime _nextTickTime;
        public INetworkHandler NetworkHandler = null!;
        public ISideHandler SideHandler = null!;
        public Thread MainThread;
        public Random Random;
        private readonly ConcurrentQueue<Action> Actions = new();
        public World World;
        public readonly string SaveLocation;
        public GameInstance(GameKind kind, Thread mainThread, int seed, string saveLocation)
        {
            Kind = kind;
            MainThread = mainThread;
            Random = new Random();
            SaveLocation = saveLocation;
            World = new World(this, seed, saveLocation);
        }

        public void Init()
        {
            _nextTickTime = DateTime.UtcNow;
        }


        private void Tick()
        {
            World.Tick();
            if (Kind.IsNetworked())
            {
                if (Kind.DoesProcessLogic())
                {
                    foreach (NetworkConnection connection in NetworkHandler.GetAllConnections())
                    {
                        connection.Flush();
                    }
                }
                else
                {
                    NetworkHandler.GetServerConnection().Flush();
                }
            }
        }

        public bool Update()
        {
            ProcessScheduled();
            if (DateTime.UtcNow >= _nextTickTime)
            {
                _nextTickTime += Constants.TickPeriod;
                Tick();
                return true;
            }
            return false;
        }

        public void Wait(Task task)
        {
            task.Wait();
        }

        public void ProcessScheduled()
        {
            int cnt = Actions.Count;
            for (int i = 0; i < cnt; ++i)
            {
                if (Actions.TryDequeue(out var action))
                {
                    action();
                }
            }
        }

        public void EnqueueWork(Action action)
        {
            Actions.Enqueue(action);
        }

        public void Dispose()
        {
            World.Dispose();
        }
    }
}
