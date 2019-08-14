using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillStamina : MonoBehaviour
{

    [SerializeField] GameObject leJoueur;
    [SerializeField] int staminaBoost;
    Animator GaugeAnimator;
    bool pompeOuverte = true;

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
        if (pompeOuverte)
        {
        //print("collision");
        collision.GetComponent<Player>().FillingStamina(staminaBoost);
        GaugeAnimator.SetBool("IsFillingStamina", true);
        pompeOuverte = false;
        }

    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        //collision.GetComponent<Player>().StopFillingStamina();
        GaugeAnimator.SetBool("IsFillingStamina", false);
        pompeOuverte = true;
    }
}
