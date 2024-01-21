using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Network.Packet_;
using Silk.NET.Maths;

namespace BlockFactory.Entity_;

public class PlayerMotionController
{
    public ClientControlledPlayerState ClientState;
    private readonly PlayerEntity _player;
    private readonly LinkedList<ClientControlledPlayerState> _states;
    [ExclusiveTo(Side.Client)] private ServerControlledPlayerState _serverState;
    [ExclusiveTo(Side.Client)] private ServerControlledPlayerState _predictedServerState;

    [ExclusiveTo(Side.Client)] private DateTime _serverStateSetTime;
    public bool PredictingState = false;

    public PlayerMotionController(PlayerEntity player)
    {
        _player = player;
        _states = new LinkedList<ClientControlledPlayerState>();
    }

    [ExclusiveTo(Side.Client)]
    private void UpdateClient()
    {
        ++ClientState.MotionTick;
        ClientState.HeadRotation = _player.HeadRotation;
        if (_player.World!.LogicProcessor.LogicalSide != LogicalSide.SinglePlayer)
        {
            _player.World.LogicProcessor.NetworkHandler.SendPacket(null, new PlayerControlPacket(ClientState));
        }

        _states.AddLast(ClientState);
    }

    [ExclusiveTo(Side.Client)]
    public void SetServerState(ServerControlledPlayerState newServerState)
    {
        if (_serverState.MotionTick == newServerState.MotionTick) return;
        var predictedState = PredictServerStateForTick(newServerState.MotionTick);
        while (_states.Count > 0 && _states.First!.Value.MotionTick < newServerState.MotionTick)
        {
            _states.RemoveFirst();
        }

        _predictedServerState = predictedState;
        _serverState = newServerState;
        _serverStateSetTime = BlockFactoryClient.LogicProcessor.LastUpdateTime;
    }

    [ExclusiveTo(Side.Client)]
    public ServerControlledPlayerState PredictServerStateForTick(long motionTick)
    {
        var lastUpdate = BlockFactoryClient.LogicProcessor.LastUpdateTime;
        var motionLerpVal = (lastUpdate - _serverStateSetTime).TotalSeconds * 10;
        motionLerpVal = Math.Clamp(motionLerpVal, 0, 1);
        ServerControlledPlayerState interpolatedBaseState = default;
        interpolatedBaseState.Velocity =
            Vector3D.Lerp(_predictedServerState.Velocity, _serverState.Velocity, motionLerpVal);
        interpolatedBaseState.Pos =
            Vector3D.Lerp(_predictedServerState.Pos, _serverState.Pos, motionLerpVal);
        interpolatedBaseState.MotionTick = _serverState.MotionTick;
        var oldPos = _player.Pos;
        _player.Pos = interpolatedBaseState.Pos;
        var oldHeadRotation = _player.HeadRotation;
        var oldCState = ClientState;
        PredictingState = true;
        foreach (var cState in _states)
        {
            if (cState.MotionTick >= motionTick) break;
            _player.HeadRotation = cState.HeadRotation;
            ClientState = cState;
            _player.UpdateMotion();
        }

        var resState = new ServerControlledPlayerState();

        resState.MotionTick = motionTick;
        resState.Pos = _player.Pos;
        //TODO set velocity

        PredictingState = false;
        ClientState = oldCState;
        _player.HeadRotation = oldHeadRotation;
        if (_player.World!.LogicProcessor.LogicalSide != LogicalSide.Client)
        {
            //TODO set velocity
            _player.Pos = oldPos;
        }

        return resState;
    }

    private void PreUpdateServer()
    {
        _player.HeadRotation = ClientState.HeadRotation;
        var newServerState = new ServerControlledPlayerState();
        newServerState.MotionTick = ClientState.MotionTick;
        newServerState.Pos = _player.Pos;
        //TODO Set velocity when physics are added
        newServerState.Velocity = Vector3D<double>.Zero;
        if (_player.World!.LogicProcessor.LogicalSide == LogicalSide.SinglePlayer)
        {
            SetServerState(newServerState);
        }
        else
        {
            _player.World!.LogicProcessor.NetworkHandler.SendPacket(_player, new PlayerPosPacket(newServerState));
        }
    }

    public void Update()
    {
        if (PredictingState) return;
        if (_player.World!.LogicProcessor.LogicalSide != LogicalSide.Server)
        {
            UpdateClient();
        }

        if (_player.World!.LogicProcessor.LogicalSide != LogicalSide.Client)
        {
            PreUpdateServer();
        }
    }
}