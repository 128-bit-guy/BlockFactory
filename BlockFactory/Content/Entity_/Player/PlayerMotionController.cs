using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Network.Packet_;
using Silk.NET.Maths;

namespace BlockFactory.Content.Entity_.Player;

public class PlayerMotionController
{
    private readonly PlayerEntity _player;
    private readonly LinkedList<ClientControlledPlayerState> _states;
    [ExclusiveTo(Side.Client)] private ServerControlledPlayerState _currentState;
    [ExclusiveTo(Side.Client)] private ServerControlledPlayerState _nextState;
    private ServerControlledPlayerState _previousPredictedState;

    [ExclusiveTo(Side.Client)] private DateTime _serverStateSetTime;
    public ClientControlledPlayerState ClientState;
    public bool PredictingState;

    public PlayerMotionController(PlayerEntity player)
    {
        _player = player;
        _states = new LinkedList<ClientControlledPlayerState>();
    }

    [ExclusiveTo(Side.Client)]
    private void UpdateClient()
    {
        _previousPredictedState = Predict();
        
        ++ClientState.MotionTick;
        ClientState.HeadRotation = _player.HeadRotation;
        if (_player.World!.LogicProcessor.LogicalSide != LogicalSide.SinglePlayer)
            _player.World.LogicProcessor.NetworkHandler.SendPacket(null, new PlayerControlPacket(ClientState));
        
        _states.AddLast(ClientState);

        _currentState = _nextState;

        while (_states.Count > 0 && _states.First!.Value.MotionTick < _currentState.MotionTick)
        {
            _states.RemoveFirst();
        }
    }

    [ExclusiveTo(Side.Client)]
    public void SetServerState(ServerControlledPlayerState newServerState)
    {
        _nextState = newServerState;
    }

    [ExclusiveTo(Side.Client)]
    private ServerControlledPlayerState Predict()
    {
        var normalState = LoadPlayerState();
        ClientState.HeadRotation = _player.HeadRotation;
        var normalCState = ClientState;
        SetPlayerState(_currentState);
        foreach (var state in _states)
        {
            ClientState = state;
            _player.HeadRotation = state.HeadRotation;
            _player.UpdateMotion();
        }

        ClientState = normalCState;
        _player.HeadRotation = normalCState.HeadRotation;
        _player.UpdateMotion();
        var res = LoadPlayerState();
        if (BlockFactoryClient.LogicProcessor!.LogicalSide != LogicalSide.Client)
        {
            SetPlayerState(normalState);
        }

        return res;
    }

    [ExclusiveTo(Side.Client)]
    private void SetPlayerState(ServerControlledPlayerState state)
    {
        _player.Pos = state.Pos;
        _player.Velocity = state.Velocity;
        _player.IsStandingOnGround = state.IsStandingOnGround;
    }

    private ServerControlledPlayerState LoadPlayerState()
    {
        ServerControlledPlayerState res = default;
        res.Pos = _player.Pos;
        res.Velocity = _player.Velocity;
        res.IsStandingOnGround = _player.IsStandingOnGround;
        res.MotionTick = ClientState.MotionTick;
        return res;
    }

    [ExclusiveTo(Side.Client)]
    public Vector3D<double> GetSmoothPos(double partialTicks)
    {
        return Vector3D.Lerp(_previousPredictedState.Pos, Predict().Pos, partialTicks);
    }

    private void UpdateServer()
    {
        _player.HeadRotation = ClientState.HeadRotation;
        var newServerState = LoadPlayerState();
        
        if (_player.World!.LogicProcessor.LogicalSide == LogicalSide.SinglePlayer)
            SetServerState(newServerState);
        else
            _player.World!.LogicProcessor.NetworkHandler.SendPacket(_player, new PlayerPosPacket(newServerState));
    }

    public void Update()
    {
        if (PredictingState) return;
        if (_player.World!.LogicProcessor.LogicalSide != LogicalSide.Server) UpdateClient();

        if (_player.World!.LogicProcessor.LogicalSide != LogicalSide.Client) UpdateServer();
    }
}