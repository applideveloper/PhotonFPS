// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerRemote.cs" company="Exit Games GmbH">
// modified by Exit Games GmbH. 
// </copyright>
// <summary>
// PlayerRemote is the local representation holding values concerning the 3D model of your opponent (i.e. rotation, position, etc.). 
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using ExitGames.Client.Photon;
using UnityEngine;
using ExitGames.Client.Photon.LoadBalancing;

public class PlayerRemote : MonoBehaviour
{
    #region Fields

	public Player player;
    private float lastUpdateTime = -1;
    private Vector3 realPos;
    public GameObject DeadSpalsh;
	// predicted!
    public Vector3 pos = Vector3.zero;
    public Quaternion rot = Quaternion.identity;
    public Vector3 targetpos = Vector3.zero;

	
    public bool PlayerIsLocal = false;
    public bool leftright;

	public bool Fire = false;
    public bool Dead = false;

    public byte currentWeapon = 0;

    public bool CrouchSatus = false;
    public bool Aim = false;
    public bool InAir = false;
    public bool Walk = true;
    public bool Reload = false;
    public KeyState keyState = KeyState.Still;

    public string Name = string.Empty;
    private GameObject _DeadSplash;

    #endregion
	
	public PlayerRemote()
	{			
	}
	
    void Start()
    {
        DeadSpalsh = GameObject.Find("DeadSoldier");
    }

    #region Player Control        

    internal void SetProperties(Hashtable properties)
    {
		if (properties.ContainsKey("N"))
            this.Name = (string)properties["N"];
		
		if (properties.ContainsKey("W"))			
			this.currentWeapon = (byte)properties["W"];

        if (properties.ContainsKey("C"))
            this.CrouchSatus = (bool) properties["C"];

        if (properties.ContainsKey("R"))
            this.InAir = (bool)properties["R"];
       
		if (properties.ContainsKey("A"))			
			this.Aim = (bool)properties["A"];  
    }
	
	public void SetCrouch(bool crouch)
	{
	    this.CrouchSatus = crouch;
	}

    public void SetInAir(bool air)
    {
        this.InAir = air;
    }

    public void SetAim(bool aim)
    {
        this.Aim = aim;
    }

	public static Vector3 GetPosition(float[] result)
	{
	    Vector3 position = new Vector3(result[0], result[1], result[2]);
	    return position;
	}
	
	public static Quaternion GetRotation(float[] result)
	{
	    Quaternion rotation = new Quaternion(result[0], result[1], result[2], result[3]);
	    return rotation;
	}
	
	internal void SetSpawnPosition(Hashtable evData)
	{
	    transform.eulerAngles = new Vector3(90, transform.rotation.y, 0);
	
	    _DeadSplash = (GameObject)Instantiate(DeadSpalsh, transform.position + new Vector3(0, 1f, 0), transform.rotation);
	
	    Destroy(_DeadSplash, 8);
	
	    Vector3 p = GetPosition((float[])evData[Constants.STATUS_PLAYER_POS]);
	    Quaternion r = GetRotation((float[])evData[Constants.STATUS_PLAYER_ROT]);
		this.pos = p;	
		this.transform.position = p;	
		this.rot = r;				
		this.realPos = p;
		this.lastUpdateTime = UnityEngine.Time.time;
		targetpos = GetPosition((float[])evData[Constants.STATUS_TARGET_POS]);
		SendMessage("SetTargetPos", targetpos);
	    
	    transform.position = pos;
	    transform.localRotation = rot;
	
	    Dead = false;		
	}
	
	// updates a (remote player's) position. directly gets the new position from the received event
	internal void SetPosition(Hashtable evData)
	{
		
	    Vector3 p = GetPosition((float[])evData[Constants.STATUS_PLAYER_POS]);
	    Quaternion r = GetRotation((float[])evData[Constants.STATUS_PLAYER_ROT]);
		
		float diff =  UnityEngine.Time.time - this.lastUpdateTime;
		if (diff > 0.2f)
		{
			// old info, walk to newest available
			this.pos = p;				
		}
		else 
		{
			// predict that he continues to walk into same direction with same speed
			this.pos = p + p - realPos;				
		}
	
		this.rot = r;				
		this.realPos = p;
		this.lastUpdateTime = UnityEngine.Time.time;
	
		targetpos = GetPosition((float[])evData[Constants.STATUS_TARGET_POS]);
		SendMessage("SetTargetPos", targetpos, SendMessageOptions.RequireReceiver);		
	}
	
	void Update()
	{
	    if (this.lastUpdateTime > 1f)
		{
            
			// move to real pos in case prediction was wrong
			this.pos = this.realPos;
			this.lastUpdateTime = UnityEngine.Time.time;
		}
	}
	
	internal void SetAnim(Hashtable evData)
	{
	    keyState = (KeyState)evData[Constants.STATUS_PLAYER_KEYSTATE];
	}
	
	internal void SetFire(Hashtable evData)
	{  
		Hashtable h = new Hashtable();
        if (evData.Contains("reload"))
        {
            this.Reload = (bool)evData["reload"];
        }
        else 
        {
            this.Reload = false;
        }
        this.Fire = (bool)evData[Constants.STATUS_PLAYER_FIRE];
		h.Add("Fire", this.Fire);
		h.Add("Weapon", this.currentWeapon);
        h.Add("reload", this.Reload);
        float[] POINT = new float[3];
        POINT[0] = (float)evData[Constants.STATUS_PLAYER_POINTX];
        POINT[1] = (float)evData[Constants.STATUS_PLAYER_POINTY];
        POINT[2] = (float)evData[Constants.STATUS_PLAYER_POINTZ];

        Vector3 point = GetPosition(POINT);
		h.Add("Point", point);

        if (evData.Contains(Constants.STATUS_PLAYER_OUTPUTPOINTX))
        {
            float[] OUTPUTPOINT = new float[3];
            OUTPUTPOINT[0] = (float)evData[Constants.STATUS_PLAYER_OUTPUTPOINTX];
            OUTPUTPOINT[1] = (float)evData[Constants.STATUS_PLAYER_OUTPUTPOINTY];
            OUTPUTPOINT[2] = (float)evData[Constants.STATUS_PLAYER_OUTPUTPOINTZ];
            Vector3 outputpoint = GetPosition(OUTPUTPOINT);
			h.Add("OutPutPoint", outputpoint);
        }
        if (this.player.playerTransform != null)
        {
			Transform _TGunManger = this.player.playerTransform.FindChild("GunManager");                                      
			_TGunManger.SendMessage("FireSimulate", h);     
		}
	}		
	
    #endregion
}