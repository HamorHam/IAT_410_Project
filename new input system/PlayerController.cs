using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("监听事件")]
    public SceneLoadEventSO loadEvent;
    public VoidEventSO afterSceneLoadedEvent;


    public PlayerInputControl inputControl;
    private Rigidbody2D rb;
    private CapsuleCollider2D coll;
    private PhysicsCheck physicsCheck;
    private PlayerAnimation playerAnimation;
    private Character character;
    public Vector2 inputDirection;
    [Header("基本参数")]
    public float speed;
    private float runSpeed;
    private float walkSpeed=> runSpeed/2.5f;
    public float jumpFoce;
    public float wallJumpFoce;
    public float hurtForce;
    public float slideDistance;
    public float slideSpeed;
    public float slidePowerCost;
    private Vector2 originalOffset;
    private Vector2 originalSize;
    

    [Header("物理材质")]
    public PhysicsMaterial2D normal;
    public PhysicsMaterial2D wall;



    [Header("状态")]
    public bool isCrouch;
    public bool isHurt;
    public bool isDead;
    public bool isAttack;
    public bool wallJump;
    public bool isSlide;
    private void Awake() 
    {
       rb=GetComponent<Rigidbody2D>();
       physicsCheck=GetComponent<PhysicsCheck>();
       coll =GetComponent<CapsuleCollider2D>();
       playerAnimation=GetComponent<PlayerAnimation>();
       character = GetComponent<Character>();
       originalOffset=coll.offset;
       originalSize=coll.size;

       inputControl= new PlayerInputControl();
        //jump(按键)
       inputControl.Gameplay.Jump.started+= Jump;

        //强制走路
        runSpeed =speed;
       inputControl.Gameplay.WalkButton.performed +=ctx =>
       {
           if(physicsCheck.isGround)
            speed = walkSpeed;
       };
      inputControl.Gameplay.WalkButton.canceled += ctx =>
       {
           if(physicsCheck.isGround)
            speed = runSpeed;
       };
       //attack（按键）
       inputControl.Gameplay.Attack.started+=PlayerAttack;

       //滑铲（按键）
       inputControl.Gameplay.Slide.started+=Slide;

       
    }
   
  
    private void OnEnable()
    {
       inputControl.Enable();
       loadEvent.LoadRequestEvent += OnLoadEvent;
       afterSceneLoadedEvent.OnEventRaised += onAfterSceneLoadedEvent;
    }


    private void OnDisable() 
    {
       inputControl.Disable();
       loadEvent.LoadRequestEvent -= OnLoadEvent;
       afterSceneLoadedEvent.OnEventRaised -= onAfterSceneLoadedEvent;
    }


    private void Update() {
       inputDirection=inputControl.Gameplay.Move.ReadValue<Vector2>();
       CheckState();

    }

    private void FixedUpdate() 
    {   if(!isHurt&&!isAttack)
        Move();
    }

    //test
    //  private void OnTriggerStay2D(Collider2D other) 
    //  {
    //    Debug.Log(other.name);
    //  }

    //加载场景时停止人物操作
    private void OnLoadEvent(GameSceneSO arg0, Vector3 arg1, bool arg2)
    {
        inputControl.Gameplay.Disable();
    }
    //加载结束后恢复操作
    private void onAfterSceneLoadedEvent()
    {
        inputControl.Gameplay.Enable();
    }

    public void Move()
    {
      if(!isCrouch&&!wallJump)
      rb.velocity = new Vector2(inputDirection.x * speed * Time.deltaTime,rb.velocity.y);
      
      int faceDir = (int)transform.localScale.x;

      //player flip
      if(inputDirection.x>0) faceDir=1;
      if(inputDirection.x<0) faceDir=-1;
      transform.localScale = new Vector3(faceDir,1,1);

      //crouch
      isCrouch = inputDirection.y< -0.5f && physicsCheck.isGround;
      if(isCrouch)
      {
         //change collion
         coll.offset=new Vector2(-0.05f,0.85f);
         coll.size=new Vector2(0.7f,1.7f);
       
      }else
      {
         //back to original
         coll.size=originalSize;
         coll.offset=originalOffset;

      }


      
    }
    //跳跃
     private void Jump(InputAction.CallbackContext obj)
   {
     if (physicsCheck.isGround)
      {
         rb.AddForce(transform.up*jumpFoce,ForceMode2D.Impulse);
         GetComponent<AudioDefination>()?.PlayAudioClip();
         //打断滑铲
         isSlide = false;
         StopAllCoroutines();

      }
      //蹬墙跳
      else if (physicsCheck.onWall)
      {
         rb.AddForce(new Vector2(-inputDirection.x,2f)*wallJumpFoce,ForceMode2D.Impulse);
         wallJump=true;
      }
   }
   //攻击
   private void PlayerAttack(InputAction.CallbackContext obj)
   {
      //使角色跳跃时无法攻击
      //if (!physicsCheck.isGround)
      //return;
     playerAnimation.PlayAttack();
     isAttack=true;
   }
   //滑铲
   private void Slide(InputAction.CallbackContext obj)
   {
      if(!isSlide && physicsCheck.isGround && character.currentPower>=slidePowerCost)
      {
        isSlide=true;
        var targetPos = new Vector3(transform.position.x+slideDistance*transform.localScale.x,transform.position.y);
        //调用滑铲协程
        StartCoroutine(TriggerSlide(targetPos));

        character.OnSlide(slidePowerCost);
      }

    
   }
   //创建滑铲协程
   private IEnumerator TriggerSlide(Vector3 target)
   {
    do
    {
      yield return null;
       //在悬崖边停止滑铲
      if(!physicsCheck.isGround)
         break;
      //滑铲过程中撞墙
      if (physicsCheck.touchLeftWall && transform.localScale.x<0f||
      physicsCheck.touchRightWall && transform.localScale.x > 0f)
      {
         isSlide=false;
         break;
      }
      
      rb.MovePosition(new Vector2 (transform.position.x+transform.localScale.x*slideSpeed,transform.position.y));
    } while (MathF.Abs(target.x-transform.position.x)>0.1f);
        //滑铲结束人物碰撞改回player层
        isSlide = false;
    }

   public void GetHurt(Transform attacker)
   {
      isHurt=true;
      rb.velocity=Vector2.zero;
      Vector2 dir=new Vector2((transform.position.x-attacker.position.x),0).normalized;
      rb.AddForce(dir* hurtForce,ForceMode2D.Impulse);
      GetComponent<AudioHurt>()?.PlayAudioClip();
    }

   public void PlayerDead()
   {
      isDead=true;
      inputControl.Gameplay.Disable();
   }

   private void CheckState()
   {
    //swich player physical material when jumpping
     coll.sharedMaterial = physicsCheck.isGround ? normal : wall;
     //玩家在墙上下滑速度
    if (physicsCheck.onWall)
    {
      rb.velocity=new Vector2(rb.velocity.x,rb.velocity.y/2f);
    }
      else
      rb.velocity=new Vector2(rb.velocity.x,rb.velocity.y);

   if(wallJump&&rb.velocity.y<0f)
   {
      wallJump=false;
   }
    
    //enemy stop acttack when player dei
     if (isDead||isSlide)
         gameObject.layer=LayerMask.NameToLayer("Enemy");
      else
         gameObject.layer=LayerMask.NameToLayer("Player");
   }
}
