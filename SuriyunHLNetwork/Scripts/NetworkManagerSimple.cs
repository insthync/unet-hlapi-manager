using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections;

public class NetworkManagerSimple : MonoBehaviour
{
    protected ErrorMessage errorMessage = new ErrorMessage();
    protected NetworkServerSimple server = null;
    protected NetworkClient client = null;
    public bool isNetworkActive { get; protected set; }
    public bool isServerConnected { get; protected set; }

    public bool useWebSockets;
    public string networkAddress = "localhost";
    public int networkPort = 7770;
    public bool serverBindToIP;
    public string serverBindAddress;
    public ConnectionConfig connectionConfig;
    public int maxConnections = 4;
    public float maxDelay = 0.01f;

    protected virtual void Update()
    {
        if (server == null || !isNetworkActive)
            return;
        server.Update();
    }

    public virtual bool StartServer()
    {
        if (server != null)
            return true;

        OnStartServer();
        server = new NetworkServerSimple();
        RegisterServerMessages();
        server.useWebSockets = useWebSockets;
        server.Configure(connectionConfig, maxConnections);
        if (serverBindToIP && !string.IsNullOrEmpty(serverBindAddress))
            isNetworkActive = server.Listen(serverBindAddress, networkPort);
        else
            isNetworkActive = server.Listen(networkPort);

        return isNetworkActive;
    }

    public virtual NetworkClient StartClient()
    {
        if (client != null)
            return client;

        client = new NetworkClient();
        RegisterClientMessages(client);
        client.Configure(connectionConfig, maxConnections);
        client.Connect(networkAddress, networkPort);
        isNetworkActive = true;
        OnStartClient(client);
        return client;
    }

    public virtual NetworkClient StartHost()
    {
        OnStartHost();
        if (StartServer())
        {
            var localClient = ConnectLocalClient();
            OnStartClient(localClient);
            return localClient;
        }
        return null;
    }

    NetworkClient ConnectLocalClient()
    {
        if (LogFilter.logDebug) { Debug.Log("NetworkManagerSimple StartHost port:" + networkPort); }
        networkAddress = "localhost";
        return StartClient();
    }

    public void StopHost()
    {
        OnStopHost();

        StopServer();
        StopClient();
    }

    public void StopServer()
    {
        isNetworkActive = false;

        if (server == null)
            return;

        OnStopServer();

        if (LogFilter.logDebug) { Debug.Log("NetworkManagerSimple StopServer"); }
        server.Stop();
        server = null;
    }

    public void StopClient()
    {
        isNetworkActive = false;

        if (client == null)
            return;

        OnStopClient();

        if (LogFilter.logDebug) { Debug.Log("NetworkManagerSimple StopClient"); }
        // only shutdown this client, not ALL clients.
        client.Disconnect();
        client.Shutdown();
        client = null;
    }

    // ----------------------------- Message Registration --------------------------------

    protected virtual void RegisterServerMessages()
    {
        server.RegisterHandler(MsgType.Connect, OnServerConnectCallback);
        server.RegisterHandler(MsgType.Disconnect, OnServerDisconnectCallback);
        server.RegisterHandler(MsgType.Error, OnServerErrorCallback);
    }

    protected virtual void RegisterClientMessages(NetworkClient client)
    {
        client.RegisterHandler(MsgType.Connect, OnClientConnectCallback);
        client.RegisterHandler(MsgType.Disconnect, OnClientDisconnectCallback);
        client.RegisterHandler(MsgType.Error, OnClientErrorCallback);
    }

    // ----------------------------- Server System Message Handlers --------------------------------

    protected virtual void OnServerConnectCallback(NetworkMessage netMsg)
    {
        if (LogFilter.logDebug) { Debug.Log("NetworkManagerSimple:OnServerConnectCallback"); }
        if (isServerConnected)
            return;
        isServerConnected = true;
        netMsg.conn.SetMaxDelay(maxDelay);
        OnServerConnect(netMsg.conn);
    }

    protected virtual void OnServerDisconnectCallback(NetworkMessage netMsg)
    {
        if (LogFilter.logDebug) { Debug.Log("NetworkManagerSimple:OnServerDisconnectCallback"); }
        if (!isServerConnected)
            return;
        isServerConnected = false;
        OnServerDisconnect(netMsg.conn);
    }

    protected virtual void OnServerErrorCallback(NetworkMessage netMsg)
    {
        if (LogFilter.logDebug) { Debug.Log("NetworkManagerSimple:OnServerErrorCallback"); }

        netMsg.ReadMessage(errorMessage);
        OnServerError(netMsg.conn, errorMessage.errorCode);
    }

    // ----------------------------- Client System Message Handlers --------------------------------

    protected virtual void OnClientConnectCallback(NetworkMessage netMsg)
    {
        if (LogFilter.logDebug) { Debug.Log("NetworkManagerSimple:OnClientConnectCallback"); }

        netMsg.conn.SetMaxDelay(maxDelay);
        OnClientConnect(netMsg.conn);
    }

    protected virtual void OnClientDisconnectCallback(NetworkMessage netMsg)
    {
        if (LogFilter.logDebug) { Debug.Log("NetworkManagerSimple:OnClientDisconnectCallback"); }

        OnClientDisconnect(netMsg.conn);
    }

    protected virtual void OnClientErrorCallback(NetworkMessage netMsg)
    {
        if (LogFilter.logDebug) { Debug.Log("HLNetworkManager:OnClientErrorCallback"); }

        netMsg.ReadMessage(errorMessage);
        OnClientError(netMsg.conn, errorMessage.errorCode);
    }

    // ----------------------------- Server System Callbacks --------------------------------

    public virtual void OnServerConnect(NetworkConnection conn)
    {
    }

    public virtual void OnServerDisconnect(NetworkConnection conn)
    {

    }

    public virtual void OnServerError(NetworkConnection conn, int errorCode)
    {
    }

    // ----------------------------- Client System Callbacks --------------------------------

    public virtual void OnClientConnect(NetworkConnection conn)
    {
    }

    public virtual void OnClientDisconnect(NetworkConnection conn)
    {
        StopClient();
    }

    public virtual void OnClientError(NetworkConnection conn, int errorCode)
    {
    }

    //------------------------------ Start & Stop callbacks -----------------------------------

    // Since there are multiple versions of StartServer, StartClient and StartHost, to reliably customize
    // their functionality, users would need override all the versions. Instead these callbacks are invoked
    // from all versions, so users only need to implement this one case.

    public virtual void OnStartHost()
    {
    }

    public virtual void OnStartServer()
    {
    }

    public virtual void OnStartClient(NetworkClient client)
    {
    }

    public virtual void OnStopServer()
    {
    }

    public virtual void OnStopClient()
    {
    }

    public virtual void OnStopHost()
    {
    }
}
