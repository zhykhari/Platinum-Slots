using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soundcontroller : MonoBehaviour {

    public GameObject soundimageOn,soundimageOff;
    public int  soundcontrol;
    public GameObject sounds;
    public string ANDROID_RATE_URL = "market://details?id=com.Focus.SeeSharp";
    public AudioSource bttnsound;

    void Start()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            soundcontrol = PlayerPrefs.GetInt("Sound");
        }
       
        if (soundcontrol == 1)
        {
            soundimageOn.SetActive(true);
            soundimageOff.SetActive(false);
            sounds.SetActive(false);
        }
        else
        {
            soundimageOn.SetActive(false);
            soundimageOff.SetActive(true);           
        }      
    }
    public void soundOnOff()
    {
        soundcontrol++;
        Debug.Log("h");
        if (soundcontrol == 1)
        {
            soundimageOn.SetActive(true);
            soundimageOff.SetActive(false);
            sounds.SetActive(false);
            PlayerPrefs.SetInt("Sound", 1);
            
        }
        else
        {
            soundcontrol = 0;
            soundimageOn.SetActive(false);
            soundimageOff.SetActive(true);
            sounds.SetActive(true);
            PlayerPrefs.SetInt("Sound", 0);
        }
    }
    public void rateus()
    {
        Application.OpenURL(ANDROID_RATE_URL);
    }
}