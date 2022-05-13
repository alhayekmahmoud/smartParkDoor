using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreElement : MonoBehaviour
{

    public TMP_Text usernameText;
    
    public TMP_Text AutSignNumber;

    public void NewScoreElement (string _username, string _autSignNumber)
    {
        usernameText.text = _username;
       
        AutSignNumber.text = _autSignNumber;
    }

}
