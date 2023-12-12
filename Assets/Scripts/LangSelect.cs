using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class LangSelect : MonoBehaviour
{
    public UIDocument document;
    public VisualElement root;
    private List<LanguageObject> LangList;
    private LanguageObject currentLang;

    // Start is called before the first frame update
    void Start()
    {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;
        GenerateLangSelectButtons();
    }

    private void GenerateLangSelectButtons()
    {
        VisualElement buttonContainer = root.Q("MenuButtons");
        LangList = Resources.LoadAll("LanguageData", typeof(LanguageObject)).Cast<LanguageObject>().ToList<LanguageObject>();
        foreach (var lang in LangList)
        {
            Debug.Log(lang.name);
            Button tempButton = new Button();
            tempButton.text = lang.name;
            buttonContainer.Add(tempButton);
            tempButton.clicked += () => { SelectLang(lang.name); };
        }
    }

    private void SelectLang(string langName)
    {
        currentLang = LangList.Find(l => l.name == langName);
        Debug.Log(langName);
        GameManager.instance.setCurrentLang(currentLang);
        this.gameObject.SetActive(false);
        GameManager.instance.activateDialogue();
    }
}
