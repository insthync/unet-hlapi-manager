# NetworkManagerSimple

The simple network manager based on Unity's NetworkManager without players, scenes stuffs focus on network messages callback handles and multiple server purposes
Class named NetworkManagerSimple will handles only an basic callbacks for Server connect, Server disconnect, Server error, Client connect, Client disconnect and Client error
You can inherit the class to handles anatoher messages

### Variables & Fields

**isNetworkActive** True if the server or client is active.
**useWebSockets** This makes the server listen for WebSockets connections instead of normal transport layer connections.
**networkAddress**	The network address currently in use.
**networkPort**	The network port currently in use.
**serverBindAddress**	The IP address to bind the server to.
**serverBindToIP**	Flag to tell the server whether to bind to a specific IP address.
**connectionConfig**	The network configuration to use.
**maxConnections**	The maximum number of concurrent network connections to support.
**maxDelay**	The maximum delay before sending packets on connections.

### Public Functions

**StartClient**	This starts a network client. It uses the networkAddress and networkPort properties as the address to connect to.
**StartHost**	This starts a network "host" - a server and client in the same application.
**StartServer**	This starts a new server.
**StopClient**	Stops the client that the manager is using.
**StopHost**	This stops both the client and the server that the manager is using.
**StopServer**	Stops the server that the manager is using.

### Public Overridable Functions

**OnClientConnect**	Called on the client when connected to a server.
**OnClientDisconnect**	Called on clients when disconnected from a server.
**OnClientError**	Called on clients when a network error occurs.
**OnServerConnect**	Called on the server when a new client connects.
**OnServerDisconnect**	Called on the server when a client disconnects.
**OnServerError**	Called on the server when a network error occurs for a client connection.
**OnStartClient**	This is a hook that is invoked when the client is started.
**OnStartHost**	This hook is invoked when a host is started.
**OnStartServer**	This hook is invoked when a server is started - including when a host is started.
**OnStopClient**	This hook is called when a client is stopped.
**OnStopHost**	This hook is called when a host is stopped.
**OnStopServer**	This hook is called when a server is stopped - including when a host is stopped.
