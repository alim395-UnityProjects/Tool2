using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static LanguageObject;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI textMeshProUGUI;
    public List<string> dialogueLines;
    public float textSpeed;

    private int index;

    private void Awake()
    {
        if(GameManager.instance == null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            if (GameManager.instance.currentLang == null)
            {
                gameObject.SetActive(false);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        LanguageObject currentLang = GameManager.instance.currentLang;
        if (currentLang != null)
        {
            foreach(StringTranslation s in currentLang.translationsList)
            {
                dialogueLines.Add(s.value);
            }
        }
        textMeshProUGUI.text = string.Empty;
        startDialogue();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if(textMeshProUGUI.text == dialogueLines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textMeshProUGUI.text = dialogueLines[index];
            }
        }
    }

    void startDialogue()
    {
        index = 0;
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach( char c in dialogueLines[index].ToCharArray()) {
            textMeshProUGUI.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if(index < dialogueLines.Count - 1) {
            index++;
            textMeshProUGUI.text = string.Empty;
            StartCoroutine (TypeLine());
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

}
