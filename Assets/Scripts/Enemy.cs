using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI; //Esto porque vamos a usar le nav mesh (que es como un pathfinding)

public class Enemy : MonoBehaviour
{
    private GameObject player;
    private NavMeshAgent navAgent;
    private Animator animator;
    public Collider rangeCollider;
    public AudioSource audioSource; //esto es para hacer que haga el sonido solo cuando el jugador se acerque a el.
    private bool isDead = false;
    private float originalSpeed;
    private bool onRange=false;
    public float fleeDistance = 10f;
    public float maxSpeed = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player"); //en vez de inicializar el gameobjetc player como de la manera normal, queremos que lo inicie detectando a algun gameobject
                                                             //que tenga la etiqueta player designada.
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource.enabled = false; //asi para que el esqueleto no suene al iniciar.
        originalSpeed = navAgent.speed; //con esto le digo, agarra la velocidad que aparece en el navmesh y guardalo dentro de esta variable, esta variable del navmeesh es speed y
                                        //se determina en el inspector del navmesh.
    }

    // Update is called once per frame
    void Update()
    {
        if(onRange && !isDead) //si esta en rango y no muerto
        {
            Vector3 fleeDirection = transform.position - player.transform.position; //con esto determino la direccion en la que el esqueleto huye
            Vector3 fleeDestination = transform.position + fleeDirection.normalized * fleeDistance; //con esto le digo para donde tiene que ir
            navAgent.SetDestination(fleeDestination); //con esto le digo al navAgent que se mueva a la direccion que creamos
            float distanceToPlayer = fleeDirection.magnitude;
            float fleeSpeed = Mathf.Clamp(originalSpeed*(fleeDistance/distanceToPlayer),0f,maxSpeed); //con esto aseguramos que no se vaya a la velocidad maxima. 
            navAgent.speed = fleeSpeed;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag =="Player") // other.CompareTag("Player") funciona de las dos formas :D
        {
            audioSource.enabled = true;
            animator.SetFloat("Speed",navAgent.speed); //con esto le pasamos al float de la animacion de correr, la velocidad del navAgent. 
            onRange = true;
            navAgent.isStopped = false; //necesitamos esto, para que corra mientras esta el player dentro del rango, osea como que se active. 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player") // other.CompareTag("Player") funciona de las dos formas :D
        {
            audioSource.enabled = false;
            navAgent.speed = originalSpeed;
            animator.SetFloat("Speed", 0f);
            onRange = false;
            navAgent.isStopped = true; //necesitamos esto, para que corra mientras esta el player dentro del rango, osea como que se active. 
        }
    }

    void Die()
    {
        if (!isDead)
        {
            isDead = true;
            animator.SetTrigger("Dead");
            rangeCollider.enabled = false; //con esto desactivamos el collider
            navAgent.isStopped = true; //desactivamos el navagent.
            audioSource.enabled = false; //desactivamos el sonido
        }
    }
}
