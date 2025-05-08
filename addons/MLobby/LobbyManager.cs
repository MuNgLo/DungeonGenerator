using Godot;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using Munglo.Commons;
namespace Munglo.GDLobby;
/// <summary>
/// The LobbyManager is to usher new connections in and once they are ready
/// inform the gamelogic there is a new player ready to join the game.
/// </summary>
internal partial class LobbyManager : MultiplayerSpawner
{
    // Running is an active host, connected is is a client instance of the lobby
    internal enum LOBBYSTATE { OFFLINE, LAUNCHING, RUNNING, CONNECTED }
    private LOBBYSTATE state = LOBBYSTATE.OFFLINE;
    [Export] private bool debug = true;
    private int port = 27020;
    private IPAddress ip;
    private List<Member> Members;

    //private HostConfig Config => SettingsModule.Settings.GetCachedSettings("HostConfig") as HostConfig;
    //internal EventHandler OnConnectAttempt;
    private ENetMultiplayerPeer localPeer;

    private MultiplayerApi MP => Multiplayer;
    public override void _Ready()
    {
        MP.MultiplayerPeer = null;
        Members = new();
        MP.ConnectedToServer += OnConnectedToServer;
        MP.ConnectionFailed += OnConnectionFailed;
        MP.PeerConnected += OnPeerConnected;
        MP.PeerDisconnected += OnPeerDisconnected;
        MP.ServerDisconnected += OnServerDisconnected;
        MP.PropertyListChanged += OnPropertyListChanged; // Exploratory (From GodotObject and should not be useful)
        ip = IPAddress.Any;
    }
    /// <summary>
    /// Fires on Client as it connects to a host
    /// </summary>
    private void OnConnectedToServer()
    {
        //Core.Log($"LobbyManager::OnConnectedToServer()");
    }
    /// <summary>
    /// Fires when a clients is started but timesout after ~34s
    /// Sets the MP Peer to NULL
    /// </summary>
    private void OnConnectionFailed()
    {
        //Core.Log($"LobbyManager::OnConnectionFailed()");
        GetTree().GetMultiplayer().MultiplayerPeer = null; // Remove peer.
    }
    /// <summary>
    /// Fires on Client as server drops from network
    /// </summary>
    private void OnServerDisconnected()
    {
        if (debug)
        {
            //Core.Log($"LobbyManager::OnServerDisconnected()");
            GetTree().GetMultiplayer().MultiplayerPeer.Close();
        }
    }
    /// <summary>
    /// Fires on all as a peer disconnect. Yes host too but not on the peer that disconnects
    /// </summary>
    /// <param name="id"></param>
    private void OnPeerDisconnected(long id)
    {
        //Core.Log($"LobbyManager::PeerDisconnected({id})");
        if (MP.IsServer())
        {
            Member pl = Members.Find(p => p.PeerID == id);
            Members.Remove(pl);
            //Events.Lobby.RaiseMemberDisconnected(id);
            pl.QueueFree();
        }
    }
    /// <summary>
    /// Fires once on connecting client for the host and every existing client.
    /// Fires once on Host and existing Clients for the joining client.
    /// </summary>
    /// <param name="id"></param>
    private void OnPeerConnected(long id) // Seems connecting client has id as 1 and host has the random peer ID
    {
        //Core.Log($"LobbyManager::OnPeerConnected({id})");
        //Events.NET.RaiseGameConnectedEvent();
        AddMember(id, LOBBYMEMBERSTATENUM.CONNECTING);
    }



    /// <summary>
    /// If host, stop it!
    /// </summary>
    public void StopHost()
    {
        if (MP.IsServer())
        {
            //Core.Log($"LobbyManager::StopHost()");
            MP.MultiplayerPeer.Close(); // OnServerDisconnected fires on clients. NOT on host
            MP.MultiplayerPeer = null;
            ClearAll();
            //Events.Lobby.RaiseHostClosed();
            // REMOVE Node named HOST that was added for debuggin
            GetTree().CurrentScene.GetNode("HOST").QueueFree();
        }
    }
    /// <summary>
    /// Asynchrounously checks UPNP 
    /// Only fails if adress is in use
    /// When succeded, raises Events.NET.RaiseGameConnectedEvent (disabled now)
    /// </summary>
    public async void StartHost(int maxPlayers)
    {
        if (state != LOBBYSTATE.OFFLINE) { return; }
        if (GetChildCount() > 0)
        {
            //Core.Log($"LobbyManager::StartHost() Can't host, Manager Node has [{GetChildCount()}]Children left.");
            return;
        }
        state = LOBBYSTATE.LAUNCHING;
        string lobbyKey = await TryUPNP();
        //Events.Lobby.RaiseHostKeyResolved(lobbyKey);
        localPeer = new ENetMultiplayerPeer();
        Error err = localPeer.CreateServer(port, maxPlayers);
        if (err != Error.Ok)
        {
            // Is another server running?
            //Core.Log($"LobbyManager::StartHost() Can't host, address in use.");
            state = LOBBYSTATE.OFFLINE;
            localPeer = null;
            return;
        }
        //Core.Log($"LobbyManager::StartHost() Lobby [{lobbyKey}] maxPlayers[{Config.maxPlayers}]"); 
        GetTree().GetMultiplayer().MultiplayerPeer = localPeer;
        state = LOBBYSTATE.RUNNING;
        // ADD Node named HOST for debuggin
        Node n = new() { Name = "HOST" };
        GetTree().CurrentScene.AddChild(n);
        Members = new();
        AddMember(1, LOBBYMEMBERSTATENUM.CONNECTING);
        //Events.Lobby.RaiseHostSetupReady();
    }
    public void JoinHost(string lobbyKey)
    {
        if (state != LOBBYSTATE.OFFLINE) { return; }
        state = LOBBYSTATE.LAUNCHING;
        // Uncode the key
        if (StringToAddressAndPort(lobbyKey, out IPAddress ip, out int port))
        {
            localPeer = new ENetMultiplayerPeer();
            Error err = localPeer.CreateClient(ip.ToString(), port);
            if (err != Error.Ok)
            {
                GD.Print($"LobbyManager::JoinHost({lobbyKey}) Client creation error :: {err}", true);
            }
            else
            {
                GetTree().GetMultiplayer().MultiplayerPeer = localPeer;
                if (debug)
                {
                    GD.Print($"LobbyManager::JoinHost({lobbyKey}) Connecting...");
                }
                state = LOBBYSTATE.CONNECTED;
                return;
            }
        }
        GD.PrintErr($"LobbyManager::JoinHost({lobbyKey}) Key could not be decoded!");
        state = LOBBYSTATE.OFFLINE;
    }
    /// <summary>
    /// Call this on client to leave the host
    /// Avoid calling this on a host
    /// </summary>
    internal void LeaveHost()
    {
        if (MP.HasMultiplayerPeer())
        {
            MP.MultiplayerPeer.Close();
            MP.MultiplayerPeer = null;
            state = LOBBYSTATE.OFFLINE;
            //Events.Lobby.RaiseLeavingHost();
        }
    }

    private void AddMember(long id, LOBBYMEMBERSTATENUM pState)
    {
        if (MP.IsServer())
        {
            if (Members.Exists(p => p.PeerID == id))
            {
                GD.PrintErr($"LobbyManager::AddMember({id}) peerID already exists! Handle This!");
                return;
            }
            string path = GetSpawnableScene(0);
            Member newMember = GD.Load<PackedScene>(path).Instantiate() as Member;
            newMember.SetMemberInfo(id, pState);
            Members.Add(newMember);
            newMember.Name = id.ToString();
            AddChild(newMember); // Remember this should be host authority synced over network with a networkspawner
            ValidateMember(id);
        }
    }
    /// <summary>
    /// Validate that given Peer is ready to join the gamelogic side of things
    /// host send object pool info so client cna set the pools up correctly. Once that is done client sends verification back
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="NotImplementedException"></exception>
    private async void ValidateMember(long id)
    {
        await Task.Delay(500);
        if (Members.Exists(p => p.PeerID == id))
        {
            Members.Find(p => p.PeerID == id).SetState(LOBBYMEMBERSTATENUM.CONNECTED);
        }
        //Events.Lobby.RaiseHostMemberValidated(id);
    }


    #region SingleFunction
    /// <summary>
    /// Make sure the Lobby is fully reset and any straggling memeber nodes is removed
    /// </summary>
    public void ClearAll()
    {
        if (Members is not null)
        {
            foreach (Member member in Members)
            {
                if (IsInstanceValid(member))
                {
                    member.QueueFree();
                }
            }
        }
        Members = new();
    }
    /// <summary>
    /// Tries to resolve the network with auto port forwarding and learn what outside IP the host will end up on.
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task<string> TryUPNP()
    {
        Upnp upnp = new();
        await Task.Run(() =>
        {
            try { upnp.Discover(); } catch (Exception e) { GD.Print(e.Message); }
        });
        // No UPNP device found so setting all read
        if (upnp.GetDeviceCount() < 1)
        {
            //Core.Log($"LobbyManager::TryUPNP() No UPNP device found");
            return "";
        }

        await Task.Run(() =>
        {
            if ((Upnp.UpnpResult)upnp.AddPortMapping(port) != Upnp.UpnpResult.Success)
            {
                // Failed to map Port
            }
        }
        );

        // resolve external IP
        await Task.Run(() =>
        {
            string ipString = upnp.QueryExternalAddress();

            if (IPAddress.TryParse(ipString, out IPAddress ip2))
            {
                ip = ip2;
            }
            else
            {
                // Failed get exterrnalIP
                //Core.Log($"LobbyManager::TryUPNP() Failed get exterrnalIP");
            }
        });
        return AddressAndPortToString(ip, port);
    }
    private static string AddressAndPortToString(System.Net.IPAddress anIPAddress, int port)
    {
        Debug.Assert(port <= UInt16.MaxValue && port >= 0);
        int portSize = sizeof(UInt16); // it's '2' but whatevs
        byte[] ipBytes = anIPAddress.GetAddressBytes();
        byte[] addressAndPortBytes = new byte[portSize + ipBytes.Length];
        Array.Copy(ipBytes, 0, addressAndPortBytes, portSize, ipBytes.Length);
        ipBytes = BitConverter.GetBytes((UInt16)(port & UInt16.MaxValue));
        Array.Copy(ipBytes, addressAndPortBytes, portSize);
        string encoded = Convert.ToBase64String(addressAndPortBytes);
        return encoded.TrimEnd('=');
    }
    private static bool StringToAddressAndPort(string encoded, out System.Net.IPAddress anIPAddress, out int port)
    {
        anIPAddress = null;
        port = -1;
        byte[] addressAndPortBytes = new byte[1];
        try
        {
            addressAndPortBytes = Convert.FromBase64String(encoded);
        }
        catch (Exception ex)
        {
            //Core.Log($"LobbyManager::StringToAddressAndPort() {ex}");
        }
        if (addressAndPortBytes.Length < 2) { return false; }
        port = BitConverter.ToUInt16(addressAndPortBytes, 0);
        byte[] anotherArray = new byte[addressAndPortBytes.Length - sizeof(UInt16)];
        Array.Copy(addressAndPortBytes, sizeof(UInt16), anotherArray, 0, anotherArray.Length);
        try
        {
            anIPAddress = new System.Net.IPAddress(anotherArray);
        }
        catch (Exception)
        {
            anIPAddress = null;
        }
        return !(anIPAddress is null);
    }
    #endregion
    #region Exploratory
    private void OnPropertyListChanged()
    {
        if (debug) { GD.Print($"LobbyManager::OnPropertyListChanged() NOT used so this line is to figure out when it fires"); }
    }
    #endregion
}// EOF CLASS
