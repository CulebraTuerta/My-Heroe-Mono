using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Timeline;

public class PlayerControl : MonoBehaviour
{
    private PlayerActions inputActionPlayer; //con esto llamamos al archivo que controla todos los movimientos e inputs
    private InputAction move;

    public float moveForce;
    public float jumpForce;
    public float maxSpeed;
    public float multiplicadorGravedad = 2.5f;
    private Vector3 directionForce = Vector3.zero; //hacia donde empujo un objeto... con vector zero, no hago fuerza inicial

    public Camera playerCam;
    public AudioClip swordHit;

    private Rigidbody rb;
    private Animator anim;
    private AudioSource audio;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();
        inputActionPlayer = new PlayerActions();
    }

    private void FixedUpdate()
    {
        rb.AddForce(Physics.gravity * (multiplicadorGravedad -1), ForceMode.Acceleration);

        directionForce += move.ReadValue<Vector2>().x * GetCameraRigth(playerCam) * moveForce; //Con esto le digo que a la fuerza de direccion que es un vector3
                                                                                               //le tomare los valores que toma en el eje de los horizotales
                                                                                               //(el x representa eso) y luego toma el vector right de la camara
                                                                                               //y luego lo multiplicamos por la fuerza de movimiento
                                                                                               //--------------------------------------------------------------------
                                                                                               //Con todo esto basicamente le decimos que recalcule el movimiento
                                                                                               //segun la posicion de la camara ya que esta al parecer se movera de manera individual
                                                                                               //--------------------------------------------------------------------
        directionForce += move.ReadValue<Vector2>().y * GetCameraForward(playerCam) * moveForce; //aqui se calculan para Y

        rb.AddForce(directionForce, ForceMode.Impulse); //con los datos obtenidos anteriormente en direction force, los aplicamos al Rigidbody y le damos que sea una fuerza tipo impulso
        directionForce = Vector3.zero; //con esto la variable esta listo 

        if(rb.linearVelocity.y<0f) //con esto verificamos el salto, para aplicar la gravedad... aunque no deberia aplicarse automaticamente?  //con este if vemos si ya va cayendo...
        {
            rb.linearVelocity -= Vector3.down * Physics.gravity.y * Time.deltaTime; //con esto hacemos que la velocidad lineal sea la gravedad
        }

        Vector3 horizontalVelocity = rb.linearVelocity; //este es simplemente una variable con la velocidad horizontal ya que considerara solo x
        horizontalVelocity.y = 0f; //aqui ya borramos el valor en y asi que solo quedo el X (y el Z tambien)

        if(horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed) //horizontalVelocity.sqrMagnitude: Devuelve la magnitud al cuadrado, sin hacer la raíz cuadrada. Es más eficiente
                                                                  //Si solo calcularamos la magnitud, el computador tiene que hacer una raiz cuadrada para determinarlo (Pitagoras)
                                                                  //Esto nos ayuda a regular y controlar la estabilidad del movimiento cuando trabajamos con fisicas.
        {
            rb.linearVelocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.linearVelocity.y; //con esto hacemos que la velocidad lineal en horizontal sea una normal
                                                                                                             //multiplicado por la maxSpeed y le sumamos un vector hacia arriba por
                                                                                                             //la velocidad lineal en Y (justo hacia arriba) para dejar a la velocidad
                                                                                                             //vertical sin cambios
        }

        LookAt(); //orienta al jugador en la direccion que se esta moviendo... 
    }   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void LookAt()
    {
        Vector3 direccion = rb.linearVelocity;
        direccion.y = 0f;

        if(move.ReadValue<Vector2>().sqrMagnitude>0.1f && direccion.sqrMagnitude > 0.1f) //con esto veo si las teclas se estan apretando
                                                                                         //y si la direccion tambien es mayor,
                                                                                         //con esto nos aseguramos que el jugador se este moviendo
        {
            rb.rotation = Quaternion.LookRotation(direccion, Vector3.up); //entonces ajustamos la direccion de rotacion
                                                                          //precisamente hacia donde se esta moviendo,
                                                                          //el vector3.up es para decirle que rote en torno al eje Y
        }
        else
        {
            rb.angularVelocity = Vector3.zero; //Si no se esta moviendo el jugador entonces esto asegura que no se mueva :D
        }
    }
    private Vector3 GetCameraRigth(Camera playerCam)
    {
        Vector3 right = playerCam.transform.right;
        right.y = 0f;
        return right.normalized;
    }
    private Vector3 GetCameraForward(Camera playerCam) //con esto calculamos el vector hacia adelante de la camara
    {
        Vector3 forward = playerCam.transform.forward; //con esto le decimos crea un v3 llamado padelante con la transformada hacia adelante de la camara (osea pa onde esta viendo) (EJE Z)
        forward.y = 0f; //con esto el valor vertical nos vale verga
        return forward.normalized; //y con esto el metodo no retorna el vector en esa direccion normalizado.
    }

    private void OnEnable() //esto habilita la funcionabilidad de los imputs
    {
        inputActionPlayer.PlayerMove.Jump.started += OnPlayerJump;
        move = inputActionPlayer.PlayerMove.Move; //basicamente las acciones de movimiento las metemos dentro de la variable "move"
        inputActionPlayer.PlayerMove.Enable();// y que luego las habilitamos
    }
    private void OnDisable() //esto deshabilita la funcionabilidad de los imputs //basicamente 
    {
        inputActionPlayer.PlayerMove.Jump.started -= OnPlayerJump;
        inputActionPlayer.PlayerMove.Disable();// aqui las desabilitamos

    }
    private void OnPlayerJump(InputAction.CallbackContext context) //contexto contiene toda la info relacionada con este metodo... 
    {
        anim.SetTrigger("Jump"); //esto lo tenemos que definir en otra parte (se hizo casi al principio... el profe partio asi.
        if (isGrounded()) //si esta tocando suelo
        {
            directionForce += Vector3.up * jumpForce;
        }
    }
    private bool isGrounded()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.25f, Vector3.down); //con esto estamos creando un rayo que se dibuja
                                                                                  //a partir de la posicion del objeto (con el punto puesto en los pies)
                                                                                  //pero que le sume un vector hacia arriba
                                                                                  //(solo para subir la posicion de partida de este rayo)(multiplicado por 0,25)
                                                                                  //y luego le damos una direccion de dibujo hacia abajo. 

        if (Physics.Raycast(ray, out RaycastHit hit, 0.3f)) //con esto decimos que si hacemos un rayo, desde la posicion de inicio con una longitud de 0.3f,
                                                            //luego si el rayo golpea algo entonces va a determinar todo lo anterior como un true,
                                                            //por ende podriamos decir que esta con algo tocando en sus pies, por ende isGrounded
        {
            return true; //esta tocando suelo
        }
        else
        {
            return false; //no esta tocando suelo
        }
    }
}
