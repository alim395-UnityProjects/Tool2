using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public LanguageObject currentLang;
    public Canvas dialogueCanvas;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        dialogueCanvas = FindFirstObjectByType<Canvas>();
        dialogueCanvas.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setCurrentLang(LanguageObject L)
    {
        this.currentLang = L;
    }

    public LanguageObject getCurrentLang()
    {
        return this.currentLang;
    }

    public void activateDialogue()
    {
        dialogueCanvas.gameObject.SetActive(true);
        dialogueCanvas.transform.GetChild(0).gameObject.SetActive(true);
    }
}
