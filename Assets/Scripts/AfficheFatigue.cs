using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AfficheFatigue : MonoBehaviour
{

    [SerializeField] GameObject leJoueur;
    Slider leSlider;
    int staminaCurrent;


    // Start is called before the first frame update
    void Start()
    {

        leSlider = GetComponent<Slider>();
        
    }

    // Update is called once per frame
    void Update()
    {
        staminaCurrent = leJoueur.GetComponent<Player>().Stamina();
        //print (message: "The current stamina is = "+ staminaCurrent);
        leSlider.value = staminaCurrent;
        


    }
}
