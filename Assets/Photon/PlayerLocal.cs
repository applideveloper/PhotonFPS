// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerLocal.cs" company="Exit Games GmbH">
// modified by Exit Games GmbH. 
// </copyright>
// <summary>
// PlayerLocal is the local representation holding values concerning the 3D model of your player (i.e. rotation, position, etc.). 
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using System.Reflection;
using ExitGames.Client.Photon.LoadBalancing;

using UnityEngine;
using System.Collections;
using Enums;
using System;

public class PlayerLocal : MonoBehaviour
{
    private Game GameInstance;
    private Vector3 Point;
    private float lastKeyPress;
    private Transform SoilderDamage;

    private float nextMoveTime;
    private GUIText nameText;
    private KeyState lastKeyState;
    private KeyState keyState = KeyState.Still;
    public byte currentWeapon = 0;

    public static bool isSendReliable;

    public bool Walk = true;
    public string Name = string.Empty;
    public Transform SoldierTarget;

    private bool crouch;
    private bool jump;
    private bool aim;

    private bool lastJump;
    private bool lastCrouch;
    private bool lastAim;

    private bool lastFire;
    private bool lastReload;

    internal Hashtable GetProperties()
    {
        Hashtable evInfo = new Hashtable();

        // name
        evInfo.Add("N", this.Name);

        // weapon
        evInfo.Add("W", currentWeapon);

        // crouch
        evInfo.Add("C", crouch);

        // aim
        evInfo.Add("A", aim);

        // in air
        evInfo.Add("R", jump);        

        return evInfo;
    }

    public static float[] GetPosition(Vector3 position)
    {
        float[] result = new float[3];
        result[0] = position.x;
        result[1] = position.y;
        result[2] = position.z;
        return result;
    }

    public static float[] GetRotation(Quaternion rotation)
    {
        float[] result = new float[4];
        result[0] = rotation.x;
        result[1] = rotation.y;
        result[2] = rotation.z;
        result[3] = rotation.w;
        return result;
    }

    public void DownLifeHitSoldier(string Hit)
    {
        SoilderDamage.SendMessage("HitSoldier", Hit);
    }

    public void DownLifeHitSoldierGranade(string Hit)
    {
        SoilderDamage.SendMessage("HitSoldierGranade", Hit);
    }

    public void Initialize(Game engine)
    {
        this.nextMoveTime = 0;
        this.GameInstance = engine;
        this.nameText = (GUIText)GameObject.Find("PlayerNamePrefab").GetComponent("GUIText");
        SoilderDamage = transform.FindChild("Pelvis/Spine1/Spine2/Spine3/EnemiesRef");
        this.Name = this.GameInstance.LocalPlayer.Name;

        Hashtable DataActor = new Hashtable();
        DataActor.Add("actornr", this.GameInstance.LocalPlayer.ID);
        DataActor.Add("actorname", Name);
        SoilderDamage.SendMessage("SetActorNr", DataActor);

        SoldierTarget = GameObject.Find("SoldierTarget").transform;
    }

    public void Update()
    {
        try
        {
            if (this.GameInstance != null)
            {
                this.ReadKeyboardInput();
                this.nameText.text = this.GameInstance.LocalPlayer.Name;
                
                if (this.GameInstance.State == ClientState.Joined)
                {
                    // these methods will send something in-room. call it only after being joined
                    this.Move();
                    this.Anim();
                    this.States();
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    void ReadKeyboardInput()
    {
        if (ChatScreen.chat)
        {
            keyState = KeyState.Still;
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            this.crouch = !this.crouch;
            this.SetCrouch(this.crouch);
        }

        if (Input.GetMouseButton(1))
        {
            this.aim = true;
        }
        else
        {
            this.aim = false;
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.jump = true;
        }

        if (Input.GetAxis("Horizontal") > 0.1f)
        {
            if (Walk)
            {
                keyState = KeyState.WalkStrafeRight;
            }
            else
            {
                keyState = KeyState.RunStrafeRight;
            }
        }
        else if (Input.GetAxis("Horizontal") < -0.1f)
        {
            if (Walk)
            {
                keyState = KeyState.WalkStrafeLeft;
            }
            else
            {
                keyState = KeyState.RunStrafeLeft;
            }
        }
        else if (Input.GetAxis("Vertical") > 0.1)
        {
            if (Walk)
            {
                keyState = KeyState.Walking;
            }
            else
            {
                keyState = KeyState.Runing;
            }
        }
        else if (Input.GetAxis("Vertical") < -0.1)
        {
            if (Walk)
            {
                keyState = KeyState.WalkBack;
            }
            else
            {
                keyState = KeyState.RuningBack;
            }
        }
        else
            keyState = KeyState.Still;
    }

    private void Anim()
    {
        if (keyState != lastKeyState)
        {
            Hashtable evInfo = new Hashtable();
            evInfo.Add(Constants.STATUS_PLAYER_KEYSTATE, (byte)keyState);
            this.GameInstance.loadBalancingPeer.OpRaiseEvent(Constants.EV_ANIM, evInfo, true, 0);
            lastKeyState = keyState;
        }
    }

    private void States()
    {
        Hashtable evInfo = new Hashtable();
        if (crouch != lastCrouch)
        {
            evInfo.Add(Constants.STATUS_PLAYER_CROUCH, crouch);
            
            lastCrouch = crouch;
        }  
        
        if (jump != lastJump)
        {
            evInfo.Add(Constants.STATUS_PLAYER_INAIR, jump);
            lastJump = jump;
        }

        if (aim != lastAim)
        {
            evInfo.Add(Constants.STATUS_PLAYER_AIM, aim);
            lastAim = aim;
        }

        if (evInfo.Keys.Count > 0)
        {
            this.GameInstance.loadBalancingPeer.OpRaiseEvent(Constants.EV_STATES, evInfo, true, 0);
        }

    }

    int num = 0;
    int numr = 0;
    public void Fire(Hashtable _hastable)
    {
        bool fire = false;
        bool reload = true;

        Hashtable evInfo = new Hashtable();
        if (_hastable.Contains("reload"))
        {
            reload = (bool)_hastable["reload"];
            evInfo.Add("reload", reload);
        }
        if (_hastable.Contains(2))
        {
            fire = (bool)_hastable[(int)2];
            evInfo.Add(Constants.STATUS_PLAYER_FIRE, fire);
        }
        if (_hastable.Contains(1))
        {
            Vector3 point = (Vector3)_hastable[(int)1];

            evInfo.Add(Constants.STATUS_PLAYER_POINTX, point.x);
            evInfo.Add(Constants.STATUS_PLAYER_POINTY, point.y);
            evInfo.Add(Constants.STATUS_PLAYER_POINTZ, point.z);
        }
        if (_hastable.Contains(3))
        {
            Vector3 outputpoint = (Vector3)_hastable[(int)3];
            evInfo.Add(Constants.STATUS_PLAYER_OUTPUTPOINTX, outputpoint.x);
            evInfo.Add(Constants.STATUS_PLAYER_OUTPUTPOINTY, outputpoint.y);
            evInfo.Add(Constants.STATUS_PLAYER_OUTPUTPOINTZ, outputpoint.z);
        }

        // send reliable for granades
        if (reload != lastReload && _hastable.Contains("reload"))
        {
            num++;
            //Debug.Log("reload" + num);
            lastReload = reload;
            this.GameInstance.loadBalancingPeer.OpRaiseEvent(Constants.EV_FIRE, evInfo, true, 0);
        }
        if (fire != lastFire && _hastable.Contains(2))
        {
            numr++;
            //Debug.Log("fire" + numr);
            lastFire = fire;
            this.GameInstance.loadBalancingPeer.OpRaiseEvent(Constants.EV_FIRE, evInfo, true, 0);
        }

    }

    private void SetGunInfoRPC(byte Gun)
    {
        currentWeapon = Gun;

        Hashtable evInfo = new Hashtable();
        evInfo.Add("W", (byte)Gun);
        this.GameInstance.OpSetPropertiesOfActor(this.GameInstance.LocalPlayer.ID, evInfo);
    }

    private void Move()
    {
        if (Time.time > this.nextMoveTime)
        {
            this.MoveOp(Constants.EV_MOVE, false);
        }
    }

    public void MoveOp(byte operationCode, bool reliable)
    {

        float[] Position = GetPosition(this.transform.position);
        float[] Rotation = GetRotation(this.transform.rotation);
        float[] TargetPosition = GetPosition(this.SoldierTarget.position);

        Hashtable evInfo = new Hashtable();

        evInfo.Add(Constants.STATUS_PLAYER_POS, (float[])Position);
        evInfo.Add(Constants.STATUS_PLAYER_ROT, (float[])Rotation);
        evInfo.Add(Constants.STATUS_TARGET_POS, (float[])TargetPosition);

        //print ("raising event for move " + Position.ToString());

        this.GameInstance.loadBalancingPeer.OpRaiseEvent(operationCode, evInfo, reliable, 0);

        // up to 10 times per second
        this.nextMoveTime = Time.time + 0.1f;
    }

    public void SetAim(bool _Flag)
    {
        Hashtable evInfo = new Hashtable();
        evInfo.Add("A", (bool)_Flag);
        this.GameInstance.OpSetPropertiesOfActor(this.GameInstance.LocalPlayer.ID, evInfo);
    }

    public void SetCrouch(bool _Flag)
    {
        Hashtable evInfo = new Hashtable();
        evInfo.Add("C", (bool)_Flag);
        this.GameInstance.OpSetPropertiesOfActor(this.GameInstance.LocalPlayer.ID, evInfo);
        
    }

    public void SetInAir(bool _Flag)
    {  
        Hashtable evInfo = new Hashtable();
        evInfo.Add("R", (bool)_Flag);
   
        if (this.jump)
        {
            this.GameInstance.loadBalancingPeer.OpRaiseEvent(Player.EV_PLAYER_INFO, evInfo, false, 0);
        }
    }

    public void SetWalk(bool _Flag)
    {
        Walk = _Flag;
    }
}
