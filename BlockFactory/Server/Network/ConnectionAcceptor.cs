using System.Net;
using System.Net.Sockets;
using BlockFactory.Network;
using BlockFactory.Side_;

namespace BlockFactory.Server.Network;

[ExclusiveTo(Side.Server)]
public class ConnectionAcceptor : IDisposable
{
    private readonly CancellationTokenSource _tokenSource;
    public readonly Socket Socket;
    private Task? _task;

    public ConnectionAcceptor(int port)
    {
        var entry = Dns.GetHostEntry(Dns.GetHostName());
        var ipAddress = entry.AddressList[0];
        var localEndPoint = new IPEndPoint(ipAddress, port);
        Socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        Socket.Bind(localEndPoint);
        _tokenSource = new CancellationTokenSource();
        OnAccepted = _ => { };
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public event Action<NetworkConnection> OnAccepted;

    ~ConnectionAcceptor()
    {
        Dispose(false);
    }

    private void BeginAccept()
    {
        var task = Socket.AcceptAsync(_tokenSource.Token);
        _task = Task.Factory.ContinueWhenAll(new[] { task.AsTask() }, AcceptSocket);
    }

    private void AcceptSocket(Task<Socket>[] res)
    {
        var connection = new NetworkConnection(res[0].Result);
        OnAccepted(connection);
        BeginAccept();
    }

    public void Start()
    {
        Socket.Listen(10);
        BeginAccept();
    }

    public void Stop()
    {
        _tokenSource.Cancel();
    }

    private void Dispose(bool disposing)
    {
        Socket.Dispose();
        _tokenSource.Dispose();
    }
}