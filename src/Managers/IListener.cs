using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EVENT_TYPE
{
     SURVIVOR_MOVE, 
     SURVIVOR_STOP, 
     TIME_OVER, 
     TIME_START, 
     SURVIVOR_WIN, 
     COUNT_DOWN, 
     SURVIVOR_DIE, 
     SURVIVOR_CREATE, 
 
     SURVIVOR_HIT, 
     SURVIVOR_GRAMCTRL,
     SURVIVOR_KEYCTRL,
     SURVIVOR_KEYCTRL2,
     SURVIVOR_RADIOCTRL,
     SURVIVOR_RADIO_SUC,
     SURVIVOR_GRAM_SUC,
     KEY_GET,
     KEY_GET2,
     B_RIGHT_BTN_POSSIBLE, 
     B_RIGHT_BTN_IMPOSSIBLE,   

     MURDERER_CREATE, 
     RADIO_SUCCESS,  
     KEY_SUCCESS, 
     GRAM_START, 
     GRAM_STOP, 
     GRAM_SUCCESS, 
     KEY_SUCCESS2, 
     KEY_GET_SUCCESS,    
     KEY_GET_SUCCESS2, 
     GRAM_OPEN_KEY, 
     RADIO_OPEN_KEY

};

public interface IListener
{

    void OnEvent(EVENT_TYPE Evenet_Type, Component Sender, object Parm = null);

}