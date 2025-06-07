using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.parent != null)
        {
            Enemy enemigo = other.transform.parent.GetComponent<Enemy>();   

            if(enemigo != null)
            {
                enemigo.Die();
                //other.tag = "Untagged"; //con esto hacemos que el enemigo que murio le demos un tag ahora de destagueado, asi no webea con el programa despues de morir
            }
        }
    }
}
