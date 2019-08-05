using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AfficheFatigue : MonoBehaviour
{

    [SerializeField] int quantiteEnergie;
    [SerializeField] GameObject leJoueur;
    Slider leSlider;

    int fatigue;


    // Start is called before the first frame update
    void Start()
    {

        leSlider = GetComponent<Slider>();
        
    }

    // Update is called once per frame
    void Update()
    {
        fatigue = leJoueur.GetComponent<Player>().Fatigue();
        print (fatigue);
        leSlider.value = quantiteEnergie - fatigue;
        


    }
}
