using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections;

public class NetworkManagerSimple : MonoBehaviour
{
    protected ErrorMessage errorMessage = new ErrorMessage();
    public NetworkServerSimple server { get; protected set; }
    public NetworkClient client { get; protected set; }
    public bool isNetworkActive { get; protected set; }

    public bool useWebSockets;
    public string networkAddress = "localhost";
    public int networkPort = 7770;
    public bool serverBindToIP;
    public string serverBindAddress;
    public ConnectionConfig connectionConfig;
    public int maxConnections = 4;
    public float maxDelay = 0.01f;
    public bool writeLog;

    protected virtual void Update()
    {
        if (server != null)
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
        if (writeLog) { Debug.Log("NetworkManagerSimple StartHost port:" + networkPort); }
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

        if (writeLog) { Debug.Log("NetworkManagerSimple StopServer"); }
        server.Stop();
        server = null;
    }

    public void StopClient()
    {
        isNetworkActive = false;

        if (client == null)
            return;

        OnStopClient();

        if (writeLog) { Debug.Log("NetworkManagerSimple StopClient"); }
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
        if (writeLog) { Debug.Log("NetworkManagerSimple:OnServerConnectCallback"); }

        netMsg.conn.SetMaxDelay(maxDelay);
        OnServerConnect(netMsg.conn);
    }

    protected virtual void OnServerDisconnectCallback(NetworkMessage netMsg)
    {
        if (writeLog) { Debug.Log("NetworkManagerSimple:OnServerDisconnectCallback"); }

        OnServerDisconnect(netMsg.conn);
    }

    protected virtual void OnServerErrorCallback(NetworkMessage netMsg)
    {
        if (writeLog) { Debug.Log("NetworkManagerSimple:OnServerErrorCallback"); }

        netMsg.ReadMessage(errorMessage);
        OnServerError(netMsg.conn, errorMessage.errorCode);
    }

    // ----------------------------- Client System Message Handlers --------------------------------

    protected virtual void OnClientConnectCallback(NetworkMessage netMsg)
    {
        if (writeLog) { Debug.Log("NetworkManagerSimple:OnClientConnectCallback"); }

        netMsg.conn.SetMaxDelay(maxDelay);
        OnClientConnect(netMsg.conn);
    }

    protected virtual void OnClientDisconnectCallback(NetworkMessage netMsg)
    {
        if (writeLog) { Debug.Log("NetworkManagerSimple:OnClientDisconnectCallback"); }

        OnClientDisconnect(netMsg.conn);
    }

    protected virtual void OnClientErrorCallback(NetworkMessage netMsg)
    {
        if (writeLog) { Debug.Log("HLNetworkManager:OnClientErrorCallback"); }

        netMsg.ReadMessage(errorMessage);
        OnClientError(netMsg.conn, errorMessage.errorCode);
    }

    // ----------------------------- Server System Callbacks --------------------------------
    /// <summary>
    /// Called on the server when a new client connects.
    /// </summary>
    /// <param name="conn"></param>
    public virtual void OnServerConnect(NetworkConnection conn)
    {
    }

    /// <summary>
    /// Called on the server when a client disconnects.
    /// </summary>
    /// <param name="conn"></param>
    public virtual void OnServerDisconnect(NetworkConnection conn)
    {

    }

    /// <summary>
    /// Called on the server when a network error occurs for a client connection.
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="errorCode"></param>
    public virtual void OnServerError(NetworkConnection conn, int errorCode)
    {
    }

    // ----------------------------- Client System Callbacks --------------------------------
    /// <summary>
    /// Called on the client when connected to a server.
    /// </summary>
    /// <param name="conn"></param>
    public virtual void OnClientConnect(NetworkConnection conn)
    {
    }

    /// <summary>
    /// Called on clients when disconnected from a server.
    /// </summary>
    /// <param name="conn"></param>
    public virtual void OnClientDisconnect(NetworkConnection conn)
    {
        StopClient();
    }

    /// <summary>
    /// Called on clients when a network error occurs.
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="errorCode"></param>
    public virtual void OnClientError(NetworkConnection conn, int errorCode)
    {
    }

    //------------------------------ Start & Stop callbacks -----------------------------------

    // Since there are multiple versions of StartServer, StartClient and StartHost, to reliably customize
    // their functionality, users would need override all the versions. Instead these callbacks are invoked
    // from all versions, so users only need to implement this one case.
    /// <summary>
    /// This hook is invoked when a host is started.
    /// </summary>
    public virtual void OnStartHost()
    {
    }

    /// <summary>
    /// This hook is invoked when a server is started - including when a host is started.
    /// </summary>
    public virtual void OnStartServer()
    {
    }

    /// <summary>
    /// This is a hook that is invoked when the client is started.
    /// </summary>
    /// <param name="client"></param>
    public virtual void OnStartClient(NetworkClient client)
    {
    }

    /// <summary>
    /// This hook is called when a server is stopped - including when a host is stopped.
    /// </summary>
    public virtual void OnStopServer()
    {
    }

    /// <summary>
    /// This hook is called when a client is stopped.
    /// </summary>
    public virtual void OnStopClient()
    {
    }

    /// <summary>
    /// This hook is called when a host is stopped.
    /// </summary>
    public virtual void OnStopHost()
    {
    }

    public bool IsServerActive()
    {
        return server != null;
    }

    public bool IsClientActive()
    {
        return client != null;
    }
}
