using UnityEngine;
using System.Collections;

public class ActorAnimator : MonoBehaviour
{
    public PlayerRemote playerRemote;
    public string currentWeaponName;
    public float jumpLandCrouchAmount = 1.6f;


    public bool aim;
    public bool fire;
    public bool walk;
    public bool crouch;

    public bool reloading;
    public int currentWeapon;
    public bool inAir;

    public Vector3 aimTarget;
    public Transform aimPivot;
    private float aimAngleY = 0.0f;

    private float groundedWeight = 1f;
	private float crouchWeight = 0f;
    //private float relaxedWeight = 1f;
	private float aimWeight = 0f;
	private float fireWeight = 0f;

    void OnEnable()
    {
        SetAnimationProperties();
    }

    void SetAnimationProperties()
	{
        if (animation.GetClip("ReloadM4") == null && animation.GetClip("ReloadM203") == null)
        {
            //  Set all animations to loop

            animation.wrapMode = WrapMode.Loop;
            animation["RunJump"].wrapMode = WrapMode.Clamp;
            animation["StandingJump"].wrapMode = WrapMode.ClampForever;
            animation["RunJump"].layer = 1;
            animation["StandingJump"].layer = 1;


            animation.AddClip(animation["StandingReloadM4"].clip, "ReloadM4");
            animation["ReloadM4"].AddMixingTransform(transform.Find("Pelvis/Spine1/Spine2"));
            animation["ReloadM4"].wrapMode = WrapMode.Clamp;
            animation["ReloadM4"].layer = 3;
            animation["ReloadM4"].time = 0;
            animation["ReloadM4"].speed = 1.0f;

            animation.AddClip(animation["StandingReloadRPG1"].clip, "ReloadM203");
            animation["ReloadM203"].AddMixingTransform(transform.Find("Pelvis/Spine1/Spine2"));
            animation["ReloadM203"].wrapMode = WrapMode.Clamp;
            animation["ReloadM203"].layer = 3;
            animation["ReloadM203"].time = 0;
            animation["ReloadM203"].speed = 1.0f;
        }
    	SetupAdditiveAiming("StandingAimUp");
    	SetupAdditiveAiming("StandingAimDown");
    	SetupAdditiveAiming("CrouchAimUp");
    	SetupAdditiveAiming("CrouchAimDown");
	}
	
	void SetupAdditiveAiming (string anim)
	{
		animation[anim].blendMode = AnimationBlendMode.Additive;
    	animation[anim].enabled = true;
    	animation[anim].weight = 1;
    	animation[anim].layer = 4;
    	animation[anim].time = 0;
    	animation[anim].speed = 0;
	}



    void CheckSoldierState()
    {
        aim = playerRemote.Aim;
        fire = playerRemote.Fire;
        walk = playerRemote.Walk;
        crouch = playerRemote.CrouchSatus;
        inAir = playerRemote.InAir;

        //Todo Send Reload
        reloading = playerRemote.Reload;
        currentWeapon = playerRemote.currentWeapon;

        currentWeaponName = playerRemote.currentWeapon == 0 ? "M4" : "M203";

        aimTarget = playerRemote.targetpos;
    }
    
    float CrossFadeUp (float weight, float fadeTime)
    {
		return Mathf.Clamp01(weight + Time.deltaTime / fadeTime);
	}
	
	float CrossFadeDown (float weight, float fadeTime) 
    {
		return Mathf.Clamp01(weight - Time.deltaTime / fadeTime);
	}

    void Update()
	{
        SetAnimationProperties();
        CheckSoldierState();
        
        if (crouch)
            crouchWeight = CrossFadeUp(crouchWeight, 0.4f);
        else if (inAir && jumpLandCrouchAmount > 0)
            crouchWeight = CrossFadeUp(crouchWeight, 1 / jumpLandCrouchAmount);
        else
            crouchWeight = CrossFadeDown(crouchWeight, 0.45f);
        float uprightWeight = 1 - crouchWeight;

        if (fire)
        {
            aimWeight = CrossFadeUp(aimWeight, 0.2f);
            fireWeight = CrossFadeUp(fireWeight, 0.2f);
        }
        else if (aim)
        {
            aimWeight = CrossFadeUp(aimWeight, 0.3f);
            fireWeight = CrossFadeDown(fireWeight, 0.3f);
        }
        else
        {
            aimWeight = CrossFadeDown(aimWeight, 0.5f);
            fireWeight = CrossFadeDown(fireWeight, 0.5f);
        }
        float nonAimWeight = (1 - aimWeight);
        //float aimButNotFireWeight = aimWeight - fireWeight;

        if (inAir)
        {
            groundedWeight = CrossFadeDown(groundedWeight, 0.1f);
        }
            
        else
            groundedWeight = CrossFadeUp(groundedWeight, 0.2f);
		
        // Aiming up/down
        Vector3 aimDir = (aimTarget - aimPivot.position).normalized;
        float targetAngle = Mathf.Asin(aimDir.y) * Mathf.Rad2Deg;
        aimAngleY = Mathf.Lerp(aimAngleY, targetAngle, Time.deltaTime * 8);

        // Use HeadLookController when not aiming/firing
        SendMessage("SetEffect",nonAimWeight);
        
        // Use additive animations for aiming when aiming and firing
        animation["StandingAimUp"].weight = uprightWeight * aimWeight;
        animation["StandingAimDown"].weight = uprightWeight * aimWeight;
        animation["CrouchAimUp"].weight = crouchWeight * aimWeight;
        animation["CrouchAimDown"].weight = crouchWeight * aimWeight;

        // Set time of animations according to current vertical aiming angle
        animation["StandingAimUp"].time = Mathf.Clamp01(aimAngleY / 90);
        animation["StandingAimDown"].time = Mathf.Clamp01(-aimAngleY / 90);
        animation["CrouchAimUp"].time = Mathf.Clamp01(aimAngleY / 90);
        animation["CrouchAimDown"].time = Mathf.Clamp01(-aimAngleY / 90);

        if (reloading)
        {
            animation.CrossFade("Reload" + currentWeaponName, 0.1f);
        }
		
        if (!inAir)
        {
            if (crouch)
            {
                Crouch();
            }
            else
            {
                Idle();
            }
        }
        else
        {
            Jump();
        }

        if (playerRemote.keyState == Enums.KeyState.Walking) { WalkFoward(); }
        if (playerRemote.keyState == Enums.KeyState.WalkBack) { WalkBack(); }
        if (playerRemote.keyState == Enums.KeyState.Runing) { Run(); }
        if (playerRemote.keyState == Enums.KeyState.RuningBack) { RunBack(); }

        if (playerRemote.keyState == Enums.KeyState.WalkStrafeLeft) { WalkStrafeLeft(); }
        if (playerRemote.keyState == Enums.KeyState.WalkStrafeRight) { WalkStrafeRight(); }
        if (playerRemote.keyState == Enums.KeyState.RunStrafeLeft) { RunStrafeLeft(); }
        if (playerRemote.keyState == Enums.KeyState.RunStrafeRight) { RunStrafeRight(); }
        if (playerRemote.keyState == Enums.KeyState.Jump) { Jump();}
        

        if (playerRemote.Fire)
        {
            Fire(currentWeapon);
        }

	}

    public void Crouch()
    {
        if (animation.IsPlaying("Walk"))
            animation.Blend("Walk", 0.0F, 0.1F);

        if (animation.IsPlaying("Run"))
            animation.Blend("Run", 0.0F, 0.1F);

        if (animation.IsPlaying("Idle"))
            animation.Blend("Idle", 0.0F, 0.1F);

        if (animation.IsPlaying("StandingJump"))
            animation.Blend("StandingJump", 0.0F, 0.1F);

        if (aim)
        {
            animation["CrouchAim"].speed = 1.0F;
            animation.CrossFade("CrouchAim");
        }
        else
        {
            animation["Crouch"].speed = 0.2F;
            animation.CrossFade("Crouch", 0.3F, PlayMode.StopAll);
        }
    }

    public void Fire(int Gun)
    {
        if(animation.IsPlaying("Walk"))
            animation.Blend("Walk", 0.0F, 0.1F);

        if(animation.IsPlaying("WalkAim"))
            animation.Blend("WalkAim", 0.0F, 0.1F);

        if(animation.IsPlaying("CrouchWalkAim"))
            animation.Blend("CrouchWalkAim", 0.0F, 0.1F);

        if(animation.IsPlaying("CrouchWalk"))
            animation.Blend("CrouchWalk", 0.0F, 0.1F);

        if(animation.IsPlaying("RunAim"))
            animation.Blend("RunAim", 0.0F, 0.1F);

        if(animation.IsPlaying("Run"))
            animation.Blend("Run", 0.0F, 0.1F);
        
        if(animation.IsPlaying("StandingJump"))
            animation.Blend("StandingJump", 0.0F, 0.1F);

        if (crouch)
        {
            if(animation.IsPlaying("CrouchWalk") || animation.IsPlaying("CrouchWalkAim"))
            {
                    animation["CrouchWalkFire"].speed = 1.0F;
                    animation.CrossFade("CrouchWalkFire");
            }
            else if(animation.IsPlaying("CrouchWalkBackwards") || animation.IsPlaying("CrouchWalkBackwardsAim"))
            {
                animation["CrouchWalkBackwardsFire"].speed = 1.0F;
                animation.CrossFade("CrouchWalkBackwardsFire");
            }
            else
            {
                if(Gun == 0)
                {
                    animation["CrouchFire"].speed = 3.0F;
                    animation.CrossFade("CrouchFire"); 
                }
                else 
                {
                    animation["CrouchFireRPG"].speed = 1.0F;
                    animation.CrossFade("CrouchFireRPG");
                }
            }
        }
        else 
        {
            if(animation.IsPlaying("Run") || animation.IsPlaying("RunAim"))
            {
                if(Gun == 0)
                {
                    animation["RunFire"].speed = 1.0F;
                    animation.CrossFade("RunFire");
                }
                else 
                {
                    animation["RunFireRPG"].speed = 1.0F;
                    animation.CrossFade("RunFireRPG");
                }
            }
            else if(animation.IsPlaying("Walk") || animation.IsPlaying("WalkAim"))
            {
                if(Gun == 0)
                {
                    animation["WalkFire"].speed = 1.0F;
                    animation.CrossFade("WalkFire");

                }
                else 
                {
                    animation["WalkFireRPG"].speed = 1.0F;
                    animation.CrossFade("WalkFireRPG");
                }
            }
            else if(animation.IsPlaying("RunBackwards") || animation.IsPlaying("RunBackwardsAim"))
            {
                if(Gun == 0)
                {
                    animation["RunBackwardsFire"].speed = 1.0F;
                    animation.CrossFade("RunBackwardsFire");
                }
                else
                {
                    animation["RunFireRPG"].speed = -1.0F;
                    animation.CrossFade("RunFireRPG");
                }

            }
            else if(animation.IsPlaying("WalkBackwards") || animation.IsPlaying("WalkBackwardsAim"))
            {
                if(Gun == 0)
                {
                    animation["WalkBackwardsFire"].speed = 1.0F;
                    animation.CrossFade("WalkBackwardsFire");
                }
                else
                {
                    animation["WalkFireRPG"].speed = -1.0F;
                    animation.CrossFade("WalkFireRPG");
                }

            }
            else if (animation.IsPlaying("CrouchStrafeWalkLeft") || animation.IsPlaying("CrouchStrafeWalkLeftAim"))
            {
                animation["CrouchStrafeWalkLeftFire"].speed = 1.0F;
                animation.CrossFade("CrouchStrafeWalkLeftFire");
            }
            else if (animation.IsPlaying("StrafeWalkLeft") || animation.IsPlaying("StrafeWalkLeftAim"))
            {
                animation["StrafeWalkLeftFire"].speed = 1.0F;
                animation.CrossFade("StrafeWalkLeftFire");
            }
            else if (animation.IsPlaying("CrouchStrafeWalkRight") || animation.IsPlaying("CrouchStrafeWalkRightAim"))
            {
                animation["CrouchStrafeWalkRightFire"].speed = 1.0F;
                animation.CrossFade("CrouchStrafeWalkRightFire");
            }
            else if (animation.IsPlaying("CrouchStrafeRunLeft") || animation.IsPlaying("CrouchStrafeRunLeftAim"))
            {
                animation["CrouchStrafeRunLeftFire"].speed = 1.0F;
                animation.CrossFade("CrouchStrafeRunLeftFire");
            }
            else if (animation.IsPlaying("StrafeRunLeft") || animation.IsPlaying("StrafeRunLeftAim"))
            {
                animation["StrafeRunLeftFire"].speed = 1.0F;
                animation.CrossFade("StrafeRunLeftFire");
            }
            else if (animation.IsPlaying("CrouchStrafeRunRight") || animation.IsPlaying("CrouchStrafeRunRightAim"))
            {
                animation["CrouchStrafeRunRightFire"].speed = 1.0F;
                animation.CrossFade("CrouchStrafeRunRightFire");
            }
            else if (animation.IsPlaying("StrafeRunRight") || animation.IsPlaying("StrafeRunRightAim"))
            {
                animation["StrafeRunRightFire"].speed = 1.0F;
                animation.CrossFade("StrafeRunRightFire");
            }
            else
            {
                if(Gun == 0)
                {
                    animation["StandingFire"].speed = 1.0F;
                    animation.Play("StandingFire");
                }
                else 
                {
                    animation["StandingFireRPG"].speed = 1.0F;
                    animation.Play("StandingFireRPG");
                }
            }
        }
    }

    public void Idle()
    {
        if (animation.IsPlaying("Walk"))
            animation.Blend("Walk", 0.0F, 0.1F);

        if (animation.IsPlaying("Run"))
            animation.Blend("Run", 0.0F, 0.1F);

        if (animation.IsPlaying("StandingJump"))
            animation.Blend("StandingJump", 0.0F, 0.1F);

        if (aim)
        {
            animation["StandingAim"].speed = 1.0F;
            animation.CrossFade("StandingAim");
        }
        else
        {
            animation.CrossFade("Standing");
        }
    }

    public void Run()
    {
  
            if (animation.IsPlaying("Walk"))
                animation.Blend("Walk", 0.0F, 0.1F);

            if (animation.IsPlaying("Run"))
                animation.Blend("Run", 0.0F, 0.1F);

            if (animation.IsPlaying("Idle"))
                animation.Blend("Idle", 0.0F, 0.1F);

            if (animation.IsPlaying("StandingJump"))
                animation.Blend("StandingJump", 0.0F, 0.1F);

            if (aim)
            {
                animation["RunAim"].speed = 1.0F;
                animation.CrossFade("RunAim");
            }
            else
            {
                animation["Run"].speed = 1.0F;
                animation.CrossFade("Run");
            }
    }

    public void RunBack()
    {

            if (animation.IsPlaying("Walk"))
                animation.Blend("Walk", 0.0F, 0.1F);

            if (animation.IsPlaying("Run"))
                animation.Blend("Run", 0.0F, 0.1F);

            if (animation.IsPlaying("Idle"))
                animation.Blend("Idle", 0.0F, 0.1F);

            if (animation.IsPlaying("StandingJump"))
                animation.Blend("StandingJump", 0.0F, 0.1F);


            if (aim)
            {
                animation["RunBackwardsAim"].speed = 1.0F;
                animation.CrossFade("RunBackwardsAim");
            }
            else
            {
                animation["RunBackwards"].speed = 1.0F;
                animation.CrossFade("RunBackwards");
            }
    }

    public void WalkFoward()
    {
        if (crouch)
            {
                if (aim)
                {
                    animation["CrouchWalkAim"].speed = 1.0F;
                    animation.CrossFade("CrouchWalkAim");
                }
                else
                {
                    animation["CrouchWalk"].speed = 1.0F;
                    animation.CrossFade("CrouchWalk");
                }
            }
            else 
            {
                if (aim)
                {
                    animation["WalkAim"].speed = 1.0F;
                    animation.CrossFade("WalkAim");
                }
                else
                {
                    animation["Walk"].speed = 1.0F;
                    animation.CrossFade("Walk");
                }
            }

    }

    public void WalkBack()
    {

            if (animation.IsPlaying("Walk"))
                animation.Blend("Walk", 0.0F, 0.1F);

            if (animation.IsPlaying("CrouchWalkAim"))
                animation.Blend("CrouchWalkAim", 0.0F, 0.1F);

            if (crouch)
            {
                if (aim)
                {
                    animation["CrouchWalkBackwardsAim"].speed = 1.0F;
                    animation.CrossFade("CrouchWalkBackwardsAim");
                }
                else
                {
                    animation["CrouchWalkBackwards"].speed = 1.0F;
                    animation.CrossFade("CrouchWalkBackwards");
                }
            }
            else
            {
                if (aim)
                {
                    animation["WalkBackwardsAim"].speed = 1.0F;
                    animation.CrossFade("WalkBackwardsAim");
                }
                else
                {
                    animation["WalkBackwards"].speed = 1.0F;
                    animation.CrossFade("WalkBackwards");
                }
            }
    }
    
    public void WalkStrafeLeft()
    {
        if (crouch)
        {
            if (aim)
            {
                animation["CrouchStrafeWalkLeftAim"].speed = 1.0F;
                animation.CrossFade("CrouchStrafeWalkLeftAim");
            }
            else
            {
                animation["CrouchStrafeWalkLeft"].speed = 1.0F;
                animation.CrossFade("CrouchStrafeWalkLeft");
            }
        }
        else
        {
            if (aim)
            {
                animation["StrafeWalkLeftAim"].speed = 1.0F;
                animation.CrossFade("StrafeWalkLeftAim");
            }
            else
            {
                animation["StrafeWalkLeft"].speed = 1.0F;
                animation.CrossFade("StrafeWalkLeft");
            }
        }
    }

    public void WalkStrafeRight()
    {
        if (crouch)
        {
            if (aim)
            {
                animation["CrouchStrafeWalkRightAim"].speed = 1.0F;
                animation.CrossFade("CrouchStrafeWalkRightAim");
            }
            else
            {
                animation["CrouchStrafeWalkRight"].speed = 1.0F;
                animation.CrossFade("CrouchStrafeWalkRight");
            }
        }
        else
        {
            if (aim)
            {
                animation["StrafeWalkRightAim"].speed = 1.0F;
                animation.CrossFade("StrafeWalkRightAim");
            }
            else
            {
                animation["StrafeWalkRight"].speed = 1.0F;
                animation.CrossFade("StrafeWalkRight");
            }
        }
    }

    public void RunStrafeLeft()
    {
        if (crouch)
        {
            if (aim)
            {
                animation["CrouchStrafeRunLeftAim"].speed = 1.0F;
                animation.CrossFade("CrouchStrafeRunLeftAim");
            }
            else
            {
                animation["CrouchStrafeRunLeft"].speed = 1.0F;
                animation.CrossFade("CrouchStrafeRunLeft");
            }
        }
        else
        {
            if (aim)
            {
                animation["StrafeRunLeftAim"].speed = 1.0F;
                animation.CrossFade("StrafeRunLeftAim");
            }
            else
            {
                animation["StrafeRunLeft"].speed = 1.0F;
                animation.CrossFade("StrafeRunLeft");
            }
        }
    }

    public void RunStrafeRight()
    {
        if (crouch)
        {
            if (aim)
            {
                animation["CrouchStrafeRunRightAim"].speed = 1.0F;
                animation.CrossFade("CrouchStrafeRunRightAim");
            }
            else
            {
                animation["CrouchStrafeRunRight"].speed = 1.0F;
                animation.CrossFade("CrouchStrafeRunRight");
            }
        }
        else
        {
            if (aim)
            {
                animation["StrafeRunRightAim"].speed = 1.0F;
                animation.CrossFade("StrafeRunRightAim");
            }
            else
            {
                animation["StrafeRunRight"].speed = 1.0F;
                animation.CrossFade("StrafeRunRight");
            }
        }
    }

    public void Jump()
    {
        animation["RunJump"].speed = 1F;
        animation.CrossFade("RunJump"); 
    }
}