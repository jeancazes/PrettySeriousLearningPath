using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillStamina : MonoBehaviour
{

    [SerializeField] GameObject leJoueur;
    Animator GaugeAnimator;

    // Start is called before the first frame update
    void Start()
    {
        GaugeAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("collision");
        collision.GetComponent<Player>().FillingStamina();
        GaugeAnimator.SetBool("IsFillingStamina", true);
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        GaugeAnimator.SetBool("IsFillingStamina", false);
    }
}
