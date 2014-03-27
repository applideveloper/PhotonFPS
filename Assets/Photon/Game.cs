// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Game.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   The Game class wraps up usage of a LoadBalancginClient, event handling and simple game logic. To make it work,
//   it must be integrated in a "gameloop", which regularly calls Game.Update().
//
//   This sample should show how to get a player's position across to other players in the same room/game.
//   Each running instance will connect to PhotonCloud (with a local player / peer), go into the lobby and
//   list the available rooms/games.
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using ExitGames.Client.Photon.LoadBalancing;
using ExitGames.Client.Photon;

public class Game : LoadBalancingClient
{
    #region Members

    public PlayerLocal PlayerLocal; // references the 3D model of the player

    public ChatScreen chatScreen;

    public delegate void DebugOutputDelegate(string debug);

    public DebugOutputDelegate DebugListeners;

    private readonly usePhoton usePhoton;
 
    private bool localGSAddr = false;
    
    #endregion

    #region Constructor and Gameloop

    public Game(DebugOutputDelegate debugDelegate, usePhoton photon)
        : base()
    {
        this.usePhoton = photon;
        this.DebugListeners = debugDelegate;
    }

    /// <summary>
    /// Update must be called by a gameloop (a single thread), so it can handle
    /// automatic movement and networking.
    /// </summary>
    public void Update()
    {
        this.Service();
    }

    #endregion

    #region IPhotonPeerListener Members

    /// <summary>
    /// Sends this players info, when someone joins the game/room. 
    /// </summary>
    private void SendPlayerInfo()
    {
        var data = this.PlayerLocal.GetProperties();
        this.loadBalancingPeer.OpRaiseEvent(Player.EV_PLAYER_INFO, data, false, 0);
    }
 
    /// <summary>
    /// Called by Photon, when any event occurs. 
    /// </summary>
    public override void OnEvent(EventData eventData)
    {
        // not all events have a actorNumber. if there's none, we most likely don't need it and -1 indicates this
        int actorNr = -1;
        if (eventData.Parameters.ContainsKey(ParameterCode.ActorNr))
        {
            actorNr = (int)eventData[ParameterCode.ActorNr];
        }

        // this player is potentially no longer in the players list, after base.OnEvent(). take care  :)
        Player playerOfEvent = null;
        if (this.CurrentRoom != null)
        {
            playerOfEvent = this.CurrentRoom.GetPlayer(actorNr);
        }

        // let the LoadbalancingClient handle it's players and room, etc.
        base.OnEvent(eventData);

        // if the player wasn't in the players list before, it might be now, after base.OnEvent(). check again.
        if (playerOfEvent == null && this.CurrentRoom != null)
        {
            playerOfEvent = this.CurrentRoom.GetPlayer(actorNr);
        }

        switch (eventData.Code)
        {
            case EventCode.Join:
                {
                    if (playerOfEvent.playerRemote == null && playerOfEvent != LocalPlayer)
                    {
                        usePhoton.AddRemotePlayer(playerOfEvent);
                        var properties = (Hashtable)eventData[ParameterCode.PlayerProperties];
                        playerOfEvent.playerRemote.SetProperties(properties);
                    }

                    this.SendPlayerInfo();
                    // send my player information
                    this.PrintPlayers();
                }
                break;

            case EventCode.Leave:
                {
                    this.usePhoton.RemoveRemotePlayer(playerOfEvent);
                }
                break;

            case Player.EV_PLAYER_INFO:
            {
                    if (playerOfEvent != null)
                    {
                        playerOfEvent.playerRemote.SetProperties((Hashtable)eventData[ParameterCode.Data]);    
                    }
                }
                break;

            case Constants.EV_HIT:
                {
                    if (playerOfEvent != null)
                    {
                        Hashtable data = (Hashtable)eventData[(byte)ParameterCode.Data];
                        int target = (int)data["T"];
                        if (target == this.LocalPlayer.ID)
                        {
                            switch ((byte)data["H"])
                            {
                                case 0:
                                    PlayerLocal.DownLifeHitSoldier(playerOfEvent.playerRemote.Name);
                                    break;
                                case 1:
                                    PlayerLocal.DownLifeHitSoldierGranade(playerOfEvent.playerRemote.Name);
                                    break;
                            }
                        }
                    }
                }
                break;
/*
            case Constants.STATUS_PLAYER_CROUCH:
                {
                    
                    if (playerOfEvent != null)
                    {
                        Hashtable table = (Hashtable)eventData[(byte) ParameterCode.Data];
                        if (table.ContainsKey("crouch"))
                        {
                            if (playerOfEvent.playerRemote != null) playerOfEvent.playerRemote.SetCrouch((bool)table["crouch"]);
                        }
                    } 
                }
                break; 
 */
                /*
            case Constants.STATUS_PLAYER_INAIR:
                {
                    this.SendPlayerInfo();
                    break;
                }   */

    
                 
            case Constants.EV_MOVE:
            {
                    if (playerOfEvent != null)
                    {
                        if (playerOfEvent.playerRemote != null) playerOfEvent.playerRemote.SetPosition((Hashtable)eventData[(byte)ParameterCode.Data]);
                    }
                }
                break;

            case Constants.EV_STATES:
            {
                if (playerOfEvent != null)
                {
                    Hashtable data = (Hashtable) eventData[(byte) ParameterCode.Data];
                    if (playerOfEvent.playerRemote != null && data.Keys.Count != 0)
                    {
                       
                        if (data.ContainsKey(Constants.STATUS_PLAYER_CROUCH))
                            playerOfEvent.playerRemote.SetCrouch((bool)data[Constants.STATUS_PLAYER_CROUCH]); 
                        if (data.ContainsKey(Constants.STATUS_PLAYER_INAIR))
                            playerOfEvent.playerRemote.SetInAir((bool)data[Constants.STATUS_PLAYER_INAIR]);
                        if (data.ContainsKey(Constants.STATUS_PLAYER_AIM))
                            playerOfEvent.playerRemote.SetAim((bool)data[Constants.STATUS_PLAYER_AIM]); 
                    }
                }
                break;
            }

            case Constants.EV_ANIM:
                {
                    if (playerOfEvent != null)
                    {
                        playerOfEvent.playerRemote.SetAnim((Hashtable)eventData[(byte)ParameterCode.Data]);
                    }
                }
                break;

            case Constants.EV_FRAG:
                {
                    playerOfEvent = this.CurrentRoom.GetPlayer(actorNr);
                    if (playerOfEvent != null)
                    {
                        playerOfEvent.playerRemote.Dead = true;
                        playerOfEvent.playerRemote.SetSpawnPosition((Hashtable)eventData[(byte)ParameterCode.Data]);
                    }
                }
                break;

            case Constants.EV_FIRE:
            {
                    actorNr = (int)eventData[ParameterCode.ActorNr];
                    playerOfEvent = this.CurrentRoom.GetPlayer(actorNr);
                    if (playerOfEvent != null)
                    {
                        playerOfEvent.playerRemote.SetFire((Hashtable)eventData[(byte)ParameterCode.Data]);
                    }
                }
                break;

            case Constants.EV_CHAT:
                {
                    chatScreen.IncomingMessage((Hashtable)eventData[(byte)ParameterCode.Data]);
                }
                break;

        }
    }
 
    /// <summary>
    /// Called by Photon as a result on any operation called by this client.
    /// </summary>
    public override void OnOperationResponse(OperationResponse operationResponse)
    {
        base.OnOperationResponse(operationResponse);

        switch (operationResponse.OperationCode)
        {
            case OperationCode.Authenticate:
                {
                    if (operationResponse.ReturnCode == ErrorCode.InvalidAuthentication)
                    {
                        this.usePhoton.ToggleAppIdWasNotAuthenticated();
                    }
                    break;
                }
   
            case OperationCode.JoinLobby:
            {
                //DebugReturn("deactivating soldierCam");
                GameObject soldierCam = GameObject.Find ("Soldier Camera");
                soldierCam.camera.enabled = false;
            
                this.usePhoton.GameManager.SendMessage("SetInLobby", true);
            
                break;
            }
            case OperationCode.JoinRandomGame:
            case OperationCode.CreateGame:
            case OperationCode.JoinGame:
                {
                    if (operationResponse[ParameterCode.Address] != null)
                    {
                        var gameServerAddress = (string)operationResponse[ParameterCode.Address];
                        string[] parts = gameServerAddress.Split(':');
                        if (parts.Length == 2 && parts[0].Equals("127.0.0.1"))
                        {
                            if (this.usePhoton == null)
                            {
                                DebugReturn("usePhoton is null...");
                            }
                            else
                            {
                                this.usePhoton.SendMessage("ToggleGameServerAddressIsLocalhost");
                                localGSAddr = true;
                            }
                            break;
                        }
                    }

                    if (this.State == ClientState.Joined)
                    {
                        //DebugReturn("activating soldierCam");
                        GameObject soldierCam = GameObject.Find ("Soldier Camera");
                        soldierCam.camera.enabled = true; 
                        
                
                
                        int actorNrReturnedForOpJoin = (int)operationResponse[ParameterCode.ActorNr];

                        foreach (Player oldPlayer in this.CurrentRoom.Players.Values)
                        {
                            if (oldPlayer.IsLocal == false)
                            {
                                usePhoton.RemoveRemotePlayer(oldPlayer);
                            }
                        }

                        foreach (KeyValuePair<int, Player> player in this.CurrentRoom.Players)
                        {
                            if (((Player)player.Value).ID == actorNrReturnedForOpJoin)
                            {
                                continue;
                            }

                            this.usePhoton.AddRemotePlayer((Player)player.Value);
                            var props = operationResponse[ParameterCode.PlayerProperties];

                            ((Player)player.Value).playerRemote.SetProperties((Hashtable)props);
                        }
                    }

                    usePhoton.GameManager.SendMessage("SetInLobby", false);
                    if (!localGSAddr) this.usePhoton.GameManager.SendMessage("initGame");
                    break;
                }
        }
    }

   
    
    #endregion

    #region Helpermethods
 
    /// <summary>
    /// For Debuggin purposes.
    /// </summary>
    public override void DebugReturn(DebugLevel level, string message)
    {
        base.DebugReturn(level, message);
        this.usePhoton.DebugReturn(message);
    }

    /// <summary>
    /// This is only used by the game/application, not by the Photon library
    /// </summary>
    public void DebugReturn(string debug)
    {
        this.DebugListeners(debug);
    }
    
    /// <summary>
    /// Joins the room/game listed on the lobby.
    /// </summary>
    public void JoinRoom(string room)
    {
        if (!this.usePhoton.Player.gameObject.GetComponent<PlayerLocal>())
        {
            // create a player Transform instance 
            this.PlayerLocal = this.usePhoton.Player.gameObject.AddComponent<PlayerLocal>();
            this.PlayerLocal.Initialize(this);
            this.PlayerLocal.name = this.LocalPlayer.Name + this.LocalPlayer.ID;

            this.chatScreen = this.usePhoton.chatScreen;
            this.chatScreen.NameSender = this.LocalPlayer.Name;
        }

        this.OpJoinRoom(room);

        this.chatScreen.Initialize(this);
    }

    /// <summary>
    /// Simple "help" function to print current list of players.
    /// As this function uses the players list, make sure it's not called while 
    /// peer.DispatchIncomingCommands() might modify the list!! (e.g. by lock(this))
    /// </summary>
    public void PrintPlayers()
    {
        if (this.CurrentRoom == null) return;
        string players = "Players: ";
        foreach (Player p in this.CurrentRoom.Players.Values)
        {
            players += p.ToString() + ", ";
        }

        this.DebugReturn(players);
    }

    /// <summary>
    /// Resets the flag if this.localGSAddr was previously set to true.
    /// This is only needed for this demo -> its not crucial.
    /// </summary>
    public void ClearGSaddrIsLocal()
    {
        this.localGSAddr = false;
    }

    #endregion
}
