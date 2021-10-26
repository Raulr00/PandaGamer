using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.InputSystem;
using System.Runtime.InteropServices;

public class PlayerController : MonoBehaviourPun,IPunObservable
{

    //[HideInInspector]
    public int id;

    public int idCanvas;
    public bool canvasBool;

    public float runSpeed = 2f;
    public float jumpSpeed = 3f;
    public Rigidbody2D rb2D;
    public CheckGround cg;

    public float doubleJumpSpeed = 2.5f;
    public float pushForce = 10f;

    public bool canDoubleJump = false;
    public bool attached = false;

    public Transform attachedTo;
    private GameObject disregard;
    
    public bool superJump = false;
    public float lowJumpMultiplier = 1f;
    public float fallMultiplier = 0.5f;

    public Player photonPlayer;
    public SpriteRenderer sr;
    public Animator anim;
    Vector2 ad;
    float Salto;
    float agarrar;
    float gravedad;
    bool activadaGravedad = false;
    bool top = false;
    Vector2 gravedadLados;
    public static PlayerController me;

    GameObject canvas;


    //Para quitar lag
    public Vector2 posicionReal;
    public Vector2 posicionUltimoPaquete;
    public double tiempoActual;
    public double tiempoActualPaquete;
    public double tiempoUltimoPaquete;
    public double tiempo;

    private Animator animator;

    private CheckGround checkGround;
    public bool chocandLatPlat;
   public int aux;


    void Awake(){
        rb2D = this.GetComponent<Rigidbody2D>();
        sr = this.GetComponent<SpriteRenderer>();
        anim = this.GetComponent<Animator>();
        cg = this.GetComponentInChildren<CheckGround>();
        animator = this.GetComponent<Animator>();
        checkGround = this.GetComponentInChildren<CheckGround>();
        

        posicionReal = new Vector2(this.GetComponent<Transform>().position.x, this.GetComponent<Transform>().position.y);
        posicionUltimoPaquete = Vector2.zero;
        tiempoActual=0.0;
        tiempoActualPaquete = 0.0;
        tiempoUltimoPaquete = 0.0;
        tiempo = 0.0;
        chocandLatPlat = false;
        aux = 0;
        //GetComponent<PlayerInput>().SwitchCurrentControlScheme.Gravedad;
        //InputSystem.EnableDevice(Keyboard.current);
        //InputSystem.DisableDevice();
    }

    bool isMobile;

#if !UNITY_EDITOR && UNITY_WEBGL
    [System.Runtime.InteropServices.DllImport("__Internal")]
    static extern bool IsMobile();
#endif

    void CheckIfMobile()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        isMobile = IsMobile();
#endif
    }

    void Start(){
        if (!photonView.IsMine)
            return;

        var user = GetComponent<PlayerInput>().user;
        //canvas = GameObject.FindWithTag("Canvas");
        canvas = this.transform.GetChild(1).gameObject;
        canvas.SetActive(false);
        /*
        if (SystemInfo.deviceType == DeviceType.Desktop){
            user.ActivateControlScheme("Keyboard&Mouse");
        } else if (SystemInfo.deviceType == DeviceType.Handheld){
            user.ActivateControlScheme("Movil");   
        }*/
        CheckIfMobile();
        if(isMobile)
            canvas.SetActive(true);
    }

    public void Movimiento(InputAction.CallbackContext callback) {
        if (!photonView.IsMine && !canvasBool)
            return;

        ad = callback.ReadValue<Vector2>();
      

        
        if (top == false){
            if (ad.x > 0){
            transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            } else if (ad.x < 0) {
            transform.localScale = new Vector3(-0.1f, 0.1f, 0.1f);
            } 
        } else {
            if (ad.x < 0){
            transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            } else if (ad.x > 0) {
            transform.localScale = new Vector3(-0.1f, 0.1f, 0.1f);
            }
        }

     
/*
            if (ad.y > 0)

            else

            */
        
    }

    public void salto(InputAction.CallbackContext callback)
    {
        if (!photonView.IsMine && !canvasBool)
            return;

        if(attached)
            return;

        Salto = callback.ReadValue<float>();
        Debug.Log("Salto");

        if (cg.isGrounded)
        {
            canDoubleJump = true;
            rb2D.velocity = new Vector2(rb2D.velocity.x, jumpSpeed);
        }
        else
        {
            if (Salto > 0f)
            {
                if (canDoubleJump)
                {
                    rb2D.velocity = new Vector2(rb2D.velocity.x, doubleJumpSpeed);
                    canDoubleJump = false;
                    return;
                }
            }
        }

        if (superJump)
        {
            if (rb2D.velocity.y > 0 && Salto == 0)
            {
                rb2D.velocity = new Vector2(rb2D.velocity.x, Physics2D.gravity.y);
            }
        }

    }
    
    public void Gravedad(InputAction.CallbackContext callback) {
        if (!photonView.IsMine)
            return;
        gravedad = callback.ReadValue<float>();

        if (gravedad > 0 && activadaGravedad == false){
            if (top == false){
                transform.eulerAngles = new Vector3(0,0,180f);
                Physics2D.gravity = new Vector2 (0,9.81f);
            } else {
                transform.eulerAngles = Vector3.zero;
                Physics2D.gravity = new Vector2 (0,-9.81f);
            }

            top = !top;
            StartCoroutine(activarGravedad());
        }
        
    }

    private IEnumerator activarGravedad()
    {
        activadaGravedad = true;
        yield return new WaitForSeconds(1f);
        activadaGravedad = false;
    }

    public void GravedadLados(InputAction.CallbackContext callback) {
        if (!photonView.IsMine)
            return;
        gravedadLados = callback.ReadValue<Vector2>();

        if (gravedadLados.x > 0){
            Physics2D.gravity = new Vector2 (9.81f, 0);
            transform.eulerAngles = new Vector3(0,0,90f);
        } else if (gravedadLados.x < 0){
            Physics2D.gravity = new Vector2 (-9.81f, 0);
            transform.eulerAngles = new Vector3(0,0,-90f);
        }
        
    }
    
    /*
    public void Agarrar(InputAction.CallbackContext callback) {
        agarrar = callback.ReadValue<Float>();

        if (agarrar > 0 && attached){
            Detach();
        }
        
    }
    */

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            tiempo = tiempoActualPaquete - tiempoUltimoPaquete;
            tiempoActual += Time.deltaTime;
            transform.position = Vector2.Lerp(posicionUltimoPaquete, posicionReal, (float)(tiempoActual / tiempo));

        }
        else if(!chocandLatPlat) {
            rb2D.velocity = new Vector2(runSpeed * ad.x, rb2D.velocity.y);
            if (ad.x != 0)
            {
                animator.SetBool("Walk", true);
            }
            else {
                animator.SetBool("Walk", false);

            }
        }
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(new Vector2(transform.position.x, transform.position.y));
        }
        else {
            tiempoActual = 0.0;
            posicionUltimoPaquete = transform.position;
            posicionReal = (Vector2)stream.ReceiveNext();
            tiempoUltimoPaquete = tiempoActualPaquete;
            tiempoActualPaquete = info.SentServerTime;
        }
    }

    [PunRPC]
    public void Initialize(Player player){
        id = player.ActorNumber;
        photonPlayer = player;

        GameManager.instance.players[id - 1] = this;

        this.gameObject.layer = 9+id;
    }



    public void OnCollisionEnter2D(Collision2D collision)
    {
        //No quirto que los objetos copia de los otro jugadores lo ejecuten
        if (!photonView.IsMine)
            return;
        if (!collision.gameObject.tag.Equals("suelo") && !collision.gameObject.tag.Equals("Objects")) {
            return;
        }

      
        if ( !checkGround.isGrounded)
        {
            chocandLatPlat = true;

        }
        else if(checkGround.isGrounded) {
            chocandLatPlat = false;
        }
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        if (!photonView.IsMine)
            return;

        if (!collision.gameObject.tag.Equals("suelo") && !collision.gameObject.tag.Equals("Objects")) {
            return;
        }

        if ( checkGround.isGrounded)
        {
            chocandLatPlat = false;

        }
        else {
            chocandLatPlat = true;
        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if (!photonView.IsMine)
            return;
        if (!collision.gameObject.tag.Equals("suelo") && !collision.gameObject.tag.Equals("Objects")) {
            return;
        }
 
            chocandLatPlat = false;
        
     
    }

    public void OnclickCanvas(){
        StartCoroutine(canvasCo());
    }

    IEnumerator canvasCo(){
        canvasBool = true;
        yield return new WaitForSeconds(2.0f);
        canvasBool = false;
    }



}
