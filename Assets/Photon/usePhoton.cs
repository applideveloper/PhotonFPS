// --------------------------------------------------------------------------------------------------------------------
// <copyright file="usePhoton.cs" company="Exit Games GmbH">
// Copyright (C) 2011 Exit Games GmbH
// </copyright>
// <summary>
// This is the "main" script of the Photon Bootcamp Loadbalancing Demo for Unity.
// This sample should show how to get a player's position across to other players in the same room/game.
// Attach to a camera (or GameObject) to update the game and players.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// This class is the intersection between the game logic (in class game) and Unity. Attached to a GameObject (cam) 
// this will update the game and process input. 
// This sample should show how to get a player's position across to other players in the same room.
// Each running instance will connect to PhotonCloud (with a local player / peer), go into the Lobby and 
// show the user the available rooms/games. 
//
// The actual handling of Photon is done in the Game and Player classes.

#region usings
#if UNITY_EDITOR
using ExitGames.Client.Photon;
using UnityEditor;
#endif

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ExitGames.Client.Photon.LoadBalancing;
#endregion

public class usePhoton : MonoBehaviour
{
    #region Fields
    public GUISkin SkinPhoton;
    public Transform playerModel;
    public Transform Player;

    private bool Pause;
    public string namePlayer;
    public Texture2D windowBackground;

    // the GameInstance is where Photon is actually handled
    private Game GameInstance;

    public GameObject MainMenuScreen;
    public GameObject GameManager;
    public ChatScreen chatScreen;
    internal const byte CHAT_MESSAGE = 64;

    GUIStyle titleStyle;
    GUIStyle createStyle;
    GUIStyle textFieldStyle;
    GUIStyle boxStyle;

    internal const float runSpeed = 4.6f;
    internal const float runStrafeSpeed = 3.07f;
    internal const float walkSpeed = 1.22f;
    internal const float walkStrafeSpeed = 1.22f;
    internal const float crouchRunSpeed = 5f;
    internal const float crouchRunStrafeSpeed = 5f;
    internal const float crouchWalkSpeed = 1.8f;
    internal const float crouchWalkStrafeSpeed = 1.8f;
    Vector3 lastPosition;

    private bool useCustomServer = false;
    
    public bool MainStart = true;
    private bool nameEntered; // used to show logos and name-input

    public bool logo1 = true;
    public bool logo2 = false;

    public Texture2D PhotonLogo;
    public Texture2D KingsMoundLogo;
    private float opacity = 1;
    private bool isVisible = false;
    private bool isHiding = false;
    private bool isShowing = false;

    public string PhotonCloudAppId;                 // your Photon Cloud AppID. See readme.txt on how to get one
    public string CustomServerAddress;              // if empty or null, the default Photon Cloud ServerAddress is used (see LoadbalancingClient)
    public string GameVersion = "v1.0";             // version of this demo. users of different versions are separated in the Cloud

    private bool UsingCloudServerWithoutAppId;       // if true, a warning will be shown to insert a "Photon Cloud AppID" in the inspector

    private bool UsingCustomAddressWithoutPort;

    private bool AppIdWasNotAuthenticated;

    private bool GameServerAddressIsLocalhost;
    #endregion

    #region Start and update routines
    
    /// <summary>
    /// Doing initial setup for the application.
    /// </summary>
    void Start()
    {
        titleStyle = SkinPhoton.GetStyle("Titles");
        createStyle = SkinPhoton.GetStyle("Create");
        textFieldStyle = SkinPhoton.GetStyle("textField");
        boxStyle = SkinPhoton.GetStyle("box");

        Application.targetFrameRate = 30;
        Application.runInBackground = true;     // this is essential to keep connections without having focus.
        StartCoroutine(this.DisplayLogoRutin());

        this.StartGame();
    }
 
    /// <summary>
    /// Starts the game.
    /// If a custom server address is set in unity editor, is will be used instead of the default one. 
    /// If no AppId is setup, a popup will be shown. 
    /// </summary>
    public void StartGame()
    {
        // The GameInstance handles Photon and encapsulates the game "logic". 
        // This way we can use it on different DotNet platforms.
        this.GameInstance = new Game(this.DebugReturn, this);

        // if the Photon Cloud is used, check if the appID is applied in the Inspector
        if (string.IsNullOrEmpty(this.CustomServerAddress) || this.GameInstance.MasterServerAddress.Equals(this.CustomServerAddress))
        {
            try
            {
                // the appIDs are always a valid GUID. if this fails, the appID is wrong
                new Guid(this.PhotonCloudAppId);
            }
            catch (Exception)
            {
                this.UsingCloudServerWithoutAppId = true;
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(this.CustomServerAddress) && !this.CustomServerAddress.Contains(":"))
            {
                // if we have a custom server address (self-hosted) but it does not contain a ":port"-part, we show a hint that connects would fail
                this.UsingCustomAddressWithoutPort = true;
            }
            else if (!string.IsNullOrEmpty(this.CustomServerAddress) && this.CustomServerAddress.Contains(":"))
            {
                // if a custom address and port is given
                this.useCustomServer = true;
            }
        }

        if (string.IsNullOrEmpty(this.namePlayer))
        {
            this.namePlayer = "Player " + (Environment.TickCount % 4);
        }

        this.GameInstance.LocalPlayer.Name = this.namePlayer;

        // connect only to Cloud if a appID is setup OR if we connect to a custom server
        if (this.UsingCloudServerWithoutAppId == false)
        {
            if (this.useCustomServer)
            {
                this.GameInstance.MasterServerAddress = this.CustomServerAddress;
            }

            this.GameInstance.ConnectToMaster(this.PhotonCloudAppId, this.GameVersion, this.namePlayer);
        }
    }
 
    /// <summary>
    /// When the application is disabled, it will disconnect from the server.
    /// </summary>
    void OnDisable()
    {
        if (GameInstance == null)
        {
            return;
        }

        GameInstance.Disconnect();
    }
 
    /// <summary>
    /// Called periodically from unity.
    /// updating the game instance and players positions. 
    /// </summary>   
    public void Update()
    {
        if (this.GameInstance == null)
        {
            return;
        }

        this.GameInstance.Update();

        this.UpdatePositions();
    }
    
    public void UpdatePositions()
    {
        // only while "joined" (being in a proper room): update remote positions and send ours. otherwise: return here
        if (this.GameInstance == null || this.GameInstance.State != ClientState.Joined)
        {
            return;
        }

        foreach (Player player in this.GameInstance.CurrentRoom.Players.Values)
        {
            Transform t;

            if (!player.IsLocal)
            {

                // was dead
                if (player.playerTransform == null)
                {
                    AddRemotePlayer(player);
                }

                t = player.playerTransform;

                if (!player.playerRemote.Dead)
                {
                    // set this Transform position
                    t.transform.position =
                       Vector3.Lerp(t.transform.position, player.playerRemote.pos, Time.deltaTime * 7.0f);

                    // set this Transform rotation
                    t.transform.localRotation =
                        Quaternion.Slerp(t.transform.localRotation, player.playerRemote.rot, Time.deltaTime * 7.0f);
                }
                // get the corresponding TextMesh (used to display the name per player)
                Hashtable DataActor = new Hashtable();
                DataActor.Add("actornr", player.ID);
                DataActor.Add("actorname", player.playerRemote.Name);
                t.FindChild("Pelvis/EnemiesRef").SendMessage("SetActorNr", DataActor);
            }
        }
    }
    #endregion
 
    #region Photon wrappermethods
    
    /// <summary>
    /// Tell the game instance to join the given room.
    /// </summary>
    void JoinRoom(string room)
    {
        this.GameInstance.JoinRoom(room);
    }
 
    /// <summary>
    /// Tell the game instance to leave the current room and 
    /// switch back to the lobby on the masterserver.
    /// </summary>
    void ToLobby()
    {
        if (this.GameInstance.State == ClientState.Joined)
        {
            foreach (Player player in this.GameInstance.CurrentRoom.Players.Values)
            {
                this.RemoveRemotePlayer(player);
            }

            this.GameInstance.OpLeaveRoom();
            Debug.Log ("ToLobbyLeave");
        }
    }

    /// <summary>
    /// Tell the game instance to create a new room/game with the given name.
    /// </summary>
    void CreateGame(string name)
    {
        if (this.GameInstance.State == ClientState.JoinedLobby)
        {
            // make up some custom properties (key is a string for those)
            Hashtable customGameProperties = new Hashtable(); // { { "map", "blue" }, { "units", 2 }, { "gamegridwidth", width }, { "gamegridheight", height } };
            //Hashtable playerProperties;

            if (!Player.gameObject.GetComponent<PlayerLocal>())
            {
                // create a player Transform instance 
                GameInstance.PlayerLocal = Player.gameObject.AddComponent<PlayerLocal>();
                GameInstance.PlayerLocal.Initialize(GameInstance);
                GameInstance.PlayerLocal.name = GameInstance.LocalPlayer.Name + GameInstance.LocalPlayer.ID;

                GameInstance.chatScreen = chatScreen;
                GameInstance.chatScreen.NameSender = GameInstance.LocalPlayer.Name;
				
            }


            //playerProperties = GameInstance.PlayerLocal.GetProperties();

            // tells the master to create the room and pass on our locally set properties of "this" player
            //            this.GameInstance.OpCreateRoom(name, true, true, 4, customGameProperties, this.GameInstance.LocalPlayer.CustomProperties, new string[] {}); // "map", "gamegridwidth", "gamegridheight" });
            this.GameInstance.OpCreateRoom(name, true, true, 4, customGameProperties, new string[] {}); // "map", "gamegridwidth", "gamegridheight" });

            this.GameInstance.chatScreen.Initialize(this.GameInstance);
        }
        else
        {
            DebugReturn("unable to create room: " + name);
            DebugReturn("GameInstance state is: " + this.GameInstance.State);
        }
    }
    #endregion
    
    #region Helpermethods
    public void AddRemotePlayer(Player player)
    {
		Debug.Log("AddRemotePlayer: " + player);
        Transform t = (Transform)Instantiate(playerModel, player.playerRemote == null ? new Vector3(0, -100, 0) : player.playerRemote.pos, Quaternion.identity);
        PlayerRemote playerRemote = t.gameObject.AddComponent<PlayerRemote>();
        playerRemote.player = player;
        player.playerRemote = playerRemote;
        player.playerTransform = t;
        t.position = player.playerRemote.pos;
        ActorAnimator ActAnimator = t.GetComponent<ActorAnimator>();
        ActAnimator.playerRemote = player.playerRemote;
	}

    public void RemoveRemotePlayer(Player player)
    {
        if (player == null || player.playerTransform == null) return;
        int playerId = player.ID;

        Destroy(player.playerTransform.gameObject);

        Destroy(GameObject.Find(playerId.ToString()));

        player.playerTransform = null;
    }

    public void DebugReturn(string debug)
    {
        Debug.Log("[DebugReturn] " + debug);
    }

    Int32 ColorToInt(Color c)
    {
        return (((int)c.r) << 16) + (((int)c.g) << 8) + ((int)c.b);
    }

    Color IntToColor(Int32 col)
    {
        if (col == 0)
        {
            return new Color(0f, 0f, 0f, 0.3f);
        }

        return new Color((float)(((double)(col & 0X00FF0000) / 256 / 256) / 256), (float)(((double)(col & 0X0000FF00) / 256) / 256), (float)(((double)(col & 0x000000FF)) / 256));
    }

    public static float[] GetPosition(Vector3 position)
    {
        float[] result = new float[3];
        result[0] = position.x;
        result[1] = position.y;
        result[2] = position.z;
        return result;
    }


    void Dead(Vector3 spawnPosition)
    {
        GameInstance.PlayerLocal.transform.position = spawnPosition;
        GameInstance.PlayerLocal.MoveOp(Constants.EV_FRAG, true);
    }

    void DownLife(int actorNr)
    {
        Hashtable evInfo = new Hashtable();
        evInfo["T"] = actorNr;
        evInfo["H"] = (byte)0;
        GameInstance.loadBalancingPeer.OpRaiseEvent(Constants.EV_HIT, evInfo, true, 0);
    }

    void DownLifeGranade(int actorNr)
    {
        Hashtable evInfo = new Hashtable();
        evInfo["T"] = actorNr;
        evInfo["H"] = (byte)1;
        GameInstance.loadBalancingPeer.OpRaiseEvent(Constants.EV_HIT, evInfo, true, 0);
    }

    void StatusPause(bool _pause)
    {
        Pause = _pause;        
    }
    #endregion

    #region UI controlings
    public void ToggleAppIdWasNotAuthenticated()
    {
        this.AppIdWasNotAuthenticated = !this.AppIdWasNotAuthenticated;
    }

    public void ToggleGameServerAddressIsLocalhost()
    {
        this.GameServerAddressIsLocalhost = !this.GameServerAddressIsLocalhost;
    }

    IEnumerator DisplayLogoRutin()
    {
        if (!MainStart)
        {
            Show();
            yield return new WaitForSeconds(3);
            Hide();
            yield return new WaitForSeconds(1);
            logo1 = false;

            yield return new WaitForSeconds(1);
            logo2 = true;
            Show();
            yield return new WaitForSeconds(3);

            Hide();
            yield return new WaitForSeconds(1);
            logo2 = false;

            MainStart = true;
        }
    }

    public void DisplayLogo(Texture2D Logo)
    {
        GUI.DrawTexture(new Rect((int)((Screen.width / 2) - (Logo.width / 2)), (int)((Screen.height / 2) - (Logo.height / 2)), Logo.width, Logo.height), Logo);
    }

    public void DisplayLogo(Texture2D Logo, float height, float width)
    {
        GUI.DrawTexture(new Rect((int)((Screen.width / 2) - (width / 2)), (int)((Screen.height / 2) - (height / 2)), width, height), Logo);
    }

    public void OnGUI()
    {
        if (this.GameServerAddressIsLocalhost)
        {
            this.DisplayGameServerAddressIsLocalhost();
            return;
        }

        if (this.AppIdWasNotAuthenticated)
        {
            this.DisplayCloudAppIdRejectedHint();
            return;
        }

        if (this.UsingCloudServerWithoutAppId)
        {
            this.DisplayCloudAppIdHint();
            return;
        }

        if (this.UsingCustomAddressWithoutPort)
        {
            this.DisplayPortInAddressHint();
            return;
        }

        if (!this.nameEntered)
        {
            Color oldColor;
            Color auxColor;
            oldColor = auxColor = GUI.color;

            if (logo1)
            {
                auxColor.a = opacity;
                GUI.color = auxColor;
                DisplayLogo(PhotonLogo, 343, 280);
            }

            if (logo2)
            {
                auxColor.a = opacity;
                GUI.color = auxColor;
                DisplayLogo(KingsMoundLogo, 211, 487);
            }
            GUI.color = oldColor;

            if (MainStart)
            {
                GUI.skin = SkinPhoton;
                GUI.DrawTexture(new Rect((int)((Screen.width / 2) - (windowBackground.width / 2)), (int)((Screen.height / 2) - (windowBackground.height / 2)), windowBackground.width, windowBackground.height), windowBackground);
                GUILayout.BeginArea(new Rect((int)(((Screen.width / 2) - (windowBackground.width / 2)) + 30), (int)(((Screen.height / 2) - (windowBackground.height / 2)) + 20), windowBackground.width - 30, windowBackground.height - 20));

                GUILayout.Label("Register", titleStyle);

                GUILayout.Space(40);


                GUILayout.BeginArea(new Rect((windowBackground.width / 2) - (windowBackground.height / 2) - 30, (windowBackground.height / 2) - 50, windowBackground.width, windowBackground.height));


                GUILayout.Label("Name:", createStyle);

                GUI.SetNextControlName("registerName");
                namePlayer = GUILayout.TextField(namePlayer, textFieldStyle, GUILayout.Width((int)((windowBackground.width / 2))));
                if (GUILayout.Button("OK", createStyle) || Input.GetKeyUp(KeyCode.KeypadEnter) || Input.GetKeyUp(KeyCode.Return))
                {
                    if (namePlayer != "")
                    {
                        // if OK is pressed do not display this RegisterBox anymore
                        //MainStart = false;


                        GameInstance.LocalPlayer.Name = namePlayer;
                        nameEntered = true;
                        MainMenuScreen.SendMessage("ShowMainConection");
                        //                        StartGame();
                    }
                }
                GUILayout.EndArea();
                GUILayout.EndArea();

                GUI.FocusControl("registerName");
            }
            return;
        }

        if (GameInstance != null)
        {
            Hashtable tmpRoomList = new Hashtable();
            foreach (KeyValuePair<string, RoomInfo> room in GameInstance.RoomInfoList)
            {
                bool open = room.Value.IsOpen;
                bool visible = room.Value.IsVisible;
                int maxP = room.Value.MaxPlayers;
                string name = room.Value.Name;
                int pCount = room.Value.PlayerCount;

                // adding roomProps as a long string, which is later split up to get the values
                // see line 763 in MainMenuScreen.js
                tmpRoomList.Add(room.Key, name + ";" + open + ";" + visible + ";" + maxP + ";" + pCount);
            }

            MainMenuScreen.SendMessage("PushList", tmpRoomList);
        }

        if (!Pause)
        {
            GUI.skin = SkinPhoton;

            GUILayout.BeginArea(new Rect(0, (int)(Screen.height - 90), 380, 90));

            GUILayout.BeginHorizontal(boxStyle, GUILayout.Width(380));
            GUILayout.Label("roundTripTime in ms: " + GameInstance.loadBalancingPeer.RoundTripTime);
            GUILayout.Label("roundTripTimeVariance in ms: " + GameInstance.loadBalancingPeer.RoundTripTimeVariance);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }

    private void DisplayPortInAddressHint()
    {
        GUI.skin = this.SkinPhoton;
        GUILayout.BeginArea(new Rect(Screen.width / 3, Screen.height / 3, Screen.width / 3, Screen.height / 2));

        GUILayout.Box("Custom Server Address and Port");
        GUILayout.TextField("The 'Custom Server Address' you entered (" + this.CustomServerAddress + ") must include a port after a colon:\n<ip or hostname>:<port number>\nPlease edit the UsePhoton values accordingly and build/restart.");

    #if UNITY_EDITOR
        if (Application.isEditor && GUILayout.Button("Select Settings Object"))
        {
            Selection.activeGameObject = this.gameObject;
        }

        if (Application.isEditor && GUILayout.Button("Stop"))
        {
            EditorApplication.ExecuteMenuItem("Edit/Play");
        }
    #endif

        GUILayout.EndArea();
    }

    private void DisplayCloudAppIdHint()
    {
        GUI.skin = this.SkinPhoton;
        GUILayout.BeginArea(new Rect(Screen.width / 3, Screen.height / 3, Screen.width / 3, Screen.height / 2));

        GUILayout.Box("Missing AppID");
        GUILayout.TextField("For the Photon Cloud, an AppID is essential. You find it on your account dashboard.\nThe script 'UsePhoton' has a field 'Photon Cloud App Id' for it. Please edit accordingly and build/restart.");

        if (GUILayout.Button("Open Dashboard"))
        {
            Application.OpenURL("https://cloud.exitgames.com/Dashboard");
        }

    #if UNITY_EDITOR
        if (Application.isEditor && GUILayout.Button("Select Settings Object"))
        {
            Selection.activeGameObject = this.gameObject;
        }

        if (Application.isEditor && GUILayout.Button("Stop"))
        {
            EditorApplication.ExecuteMenuItem("Edit/Play");
        }
    #endif

        GUILayout.EndArea();
    }

    private void DisplayCloudAppIdRejectedHint()
    {
        GUI.skin = this.SkinPhoton;
        GUILayout.BeginArea(new Rect(Screen.width / 3, Screen.height / 3, Screen.width / 3, Screen.height / 2));

        GUILayout.Box("Your AppID could not be authenticated!");
        GUILayout.TextField("For connecting to the Photon Cloud, your AppID has to be active. You find it on your account dashboard.\nThe script 'UsePhoton' has a field 'Photon Cloud App Id' for it. Please edit accordingly and build/restart.");

    #if UNITY_EDITOR
        if (Application.isEditor && GUILayout.Button("Stop"))
        {
            EditorApplication.ExecuteMenuItem("Edit/Play");
        }
    #else
        if (GUILayout.Button("Stop"))
        {
            // somehow exit the app
            Application.CancelQuit();
        }
    #endif

        GUILayout.EndArea();
    }

    private void DisplayGameServerAddressIsLocalhost()
    {
        GUI.skin = this.SkinPhoton;
        GUILayout.BeginArea(new Rect(Screen.width / 3, Screen.height / 3, (Screen.width / 3) + 50, Screen.height / 2));

        GUILayout.Box("Game server address is localhost (127.0.0.1)");
        GUILayout.TextField("The address for the game-server which was returned by the master-server points to your machine, is this intended? Please edit your game-server config accordingly and build/restart.");

        if (GUILayout.Button("Ignore"))
        {
            ToggleGameServerAddressIsLocalhost();
            this.GameManager.SendMessage("initGame");
            GameInstance.ClearGSaddrIsLocal();
        }
        
    #if UNITY_EDITOR
        if (Application.isEditor && GUILayout.Button("Stop"))
        {
            EditorApplication.ExecuteMenuItem("Edit/Play");
            this.GameManager.SendMessage("initGame");
            GameInstance.ClearGSaddrIsLocal();
        }
    #else
        if (Application.isEditor && GUILayout.Button("Stop"))
        {
            Application.CancelQuit();
            this.GameManager.SendMessage("initGame");
            GameInstance.ClearGSaddrIsLocal();
        }        
    #endif

        GUILayout.EndArea();
    }

    public void Show()
    {
        if (isVisible == false)
        {
            StartCoroutine(ShowBySlideFromBottom());
        }
    }

    public void Hide()
    {
        if (isVisible && !isHiding)
        {
            StartCoroutine(HideByFading());
        }
    }

    private IEnumerator ShowBySlideFromBottom()
    {
        if (!isShowing || !isHiding)
        {
            isShowing = isVisible = true; opacity = 0;
            while (opacity < 1)
            {
                opacity += Time.deltaTime * 2f;
                yield return new WaitForEndOfFrame();
            }
            opacity = 1; isShowing = false;
        }
    }

    private IEnumerator HideByFading()
    {
        if (!isShowing || !isHiding)
        {
            isHiding = true;

            while (opacity > 0)
            {
                opacity -= Time.deltaTime * 3;
                yield return new WaitForEndOfFrame();
            }
            if (opacity <= 0)
            {
                if (logo1) logo1 = false;
                if (logo2) logo2 = false;
            }
            isHiding = isVisible = false; opacity = 1;
        }
    }
    #endregion
}