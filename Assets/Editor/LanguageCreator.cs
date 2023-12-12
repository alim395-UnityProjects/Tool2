using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static LanguageObject;
using static UnityEngine.EventSystems.EventTrigger;
using Button = UnityEngine.UIElements.Button;

public class LanguageCreator : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    private VisualTreeAsset translationItemTemplate;

    private List<LanguageObject> LangList = new List<LanguageObject>();
    private List<string> LangNames;
    private LanguageObject currentLang;

    // Visual Elements
    VisualElement LanguageList;
    
    VisualElement LanguageKDPairs;

    DropdownField SelectLang;

    VisualElement KDPairs;
    Button KDUpdate;

    VisualElement KeysElement;
    Button KeyUpdate;

    TextField KDKeyModify;
    VisualElement KDModifyButtons;
    Button KDAddButton;
    Button KDRemoveButton;

    TextField LangModify;
    VisualElement LangModifyButtons;
    Button LangAddButton;
    Button LangRemoveButton;

    [MenuItem("Window/UI Toolkit/LanguageCreator")]
    public static void ShowExample()
    {
        LanguageCreator wnd = GetWindow<LanguageCreator>();
        wnd.titleContent = new GUIContent("LanguageCreator");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        GetAllLanguages();

        // Instantiate UXML
        VisualElement mainUXML = m_VisualTreeAsset.Instantiate();
        root.Add(mainUXML);

        // Grab Templates
        translationItemTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"Assets/Editor/TranslationItem.uxml");
        if(translationItemTemplate == null)
        {
            Debug.Log("Translation Template not found.");
        }

        // Instantiate LanguageUI
        LangListUI(root);
        LangNamesUI(root);
        KDPairsUI(root);
        KeysUI(root);
        KDModifyUI(root);
        LangModifyUI(root);

        //Activate Bindings
        BindTranslations();

        // Register Events
        SelectLang.RegisterValueChangedCallback(v => SelectLangDropdown(root, v.newValue));
        if (KDUpdate != null)
        {
            KDUpdate.clicked += KDUpdate_clicked;
        }
        if(KeyUpdate != null)
        {
            KeyUpdate.clicked += KeyUpdate_clicked;
        }
        else
        {
            Debug.Log("Key Update not found");
        }
        if (KDAddButton != null)
        {
            KDAddButton.clicked += KDAddButton_clicked;
        }
        if (KDRemoveButton != null)
        {
            KDRemoveButton.clicked += KDRemoveButton_clicked;
        }
        if(LangAddButton != null)
        {
            LangAddButton.clicked += LangAddButton_clicked;
        }
        if(LangRemoveButton != null)
        {
            LangRemoveButton.clicked += LangRemoveButton_clicked;
        }
    }

    private void KeyUpdate_clicked()
    {
        UpdateKeyNamesAll();
        refreshLanguages();
    }

    private void UpdateKeyNamesAll()
    {
        List<String> keyNames = new List<String>();
        if (SelectLang.value != null)
        {
            if (KeysElement != null)
            {
                // Get all names for Key
                foreach (TextField key in KeysElement.Children())
                {
                    keyNames.Add(key.value);
                }

                foreach(LanguageObject L in LangList)
                {
                    UpdateKeyNames(L, keyNames);
                }
            }
        }
    }

    private void UpdateKeyNames(LanguageObject l, List<String> k)
    {
        int count = 0;
        foreach(StringTranslation s in l.translationsList)
        {
            s.key = k[count++];
        }
    }

    private void KeysUI(VisualElement root)
    {
        KeysElement = root.Q(name = "Keys");
        KeyUpdate = root.Q<Button>(name = "KeyUpdate");
    }

    private void BindTranslations()
    {
        if(SelectLang.value != null)
        {
            currentLang = LangList.Find(l => l.name.Equals(SelectLang.value));

            foreach (var translation in currentLang.translations)
            {
                var item = translationItemTemplate.CloneTree();
                var valueField = item.Q<TextField>("KDItem");

                valueField.label = translation.Key;
                valueField.value = translation.Value;
                valueField.RegisterValueChangedCallback(evt =>
                {
                    currentLang.translations[translation.Key] = evt.newValue;
                    UpdateTranslationList(translation.Key, evt.newValue);
                });

                KDPairs.Add(item);
            }
        }
    }

    private void UpdateTranslationList(string key, string newValue)
    {
        var translation = currentLang.translationsList.Find(t => t.key == key);
        if (translation != null)
        {
            translation.value = newValue;
        }
        else
        {
            currentLang.translationsList.Add(new LanguageObject.StringTranslation(key, newValue));
        }
    }

    private void LangAddButton_clicked()
    {
        LanguageObject tempLang = LangList.First();
        if (SelectLang != null) {
            if(SelectLang.value != null)
            {
                tempLang = LangList.Find(l => l.name == SelectLang.value);
            }
        }
        if (LangModify.value != null)
        {
            string newLangName = LangModify.value;
            //Debug.Log(newLangName);
            if(!LangNames.Contains(newLangName))
            {
                LanguageObject newLanguage = ScriptableObject.CreateInstance<LanguageObject>();
                AssetDatabase.CreateAsset(newLanguage, $"Assets/Resources/LanguageData/{newLangName}.asset");
                foreach(StringTranslation t in tempLang.translationsList)
                {
                    newLanguage.translationsList.Add(t);
                }
                EditorUtility.SetDirty(newLanguage);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }

        }
    }

    private void LangRemoveButton_clicked()
    {
        if (LangModify.value != null)
        {
            string newLangName = LangModify.value;
            //Debug.Log(newLangName);
            if (LangNames.Contains(newLangName))
            {
                AssetDatabase.DeleteAsset($"Assets/Resources/LanguageData/{newLangName}.asset");
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }

        }
    }

    private void LangModifyUI(VisualElement root)
    {
        LangModify = root.Q<TextField>(name = "LangModify");
        LangModifyButtons = root.Q(name = "LangModifyButtons");
        LangAddButton = LangModifyButtons.Q<Button>(name = "LangAddButton");
        LangRemoveButton = LangModifyButtons.Q<Button>(name = "LangRemoveButton");
    }

    private void KDAddButton_clicked()
    {
        SelectLang = rootVisualElement.Q<DropdownField>(name = "LanguageSelect");
        string nameLang = SelectLang.value;
        bool isDuplicate = false;
        if (nameLang != null)
        {
            LanguageObject currentLang = LangList.Find(l => l.name.Equals(nameLang));
            string newKey = KDKeyModify.value;
            if(!String.IsNullOrEmpty(KDKeyModify.value))
            {
                StringTranslation newTranslation = new StringTranslation(newKey, "Translation");
                //Check if Key already Exists
                foreach(StringTranslation s in currentLang.translationsList)
                {
                    if (newTranslation.equalKey(s)) {
                        isDuplicate = true;
                        break;
                    }
                }
                if (!isDuplicate)
                {
                    currentLang.translationsList.Add(newTranslation);
                    refreshLanguages();
                    currentLang = LangList.Find(l => l.name.Equals(nameLang));
                    TextField newKeyValue = KDPairs.Q<TextField>(name = newKey);
                    newKeyValue = new TextField(name = newKey);
                    newKeyValue.label = newKey;
                    newKeyValue.value = "Translation";
                    KDPairs.Add(newKeyValue);
                    KDPairsUI(rootVisualElement);
                }
            }
            
        }
    }

    private void KDRemoveButton_clicked()
    {
        SelectLang = rootVisualElement.Q<DropdownField>(name = "LanguageSelect");
        string nameLang = SelectLang.value;
        if (nameLang != null)
        {
            // remove Key from current language
            LanguageObject currentLang = LangList.Find(l => l.name.Equals(nameLang));
            string newKey = KDKeyModify.value;
            StringTranslation newTranslation = new StringTranslation(newKey, "Translation");
            int index = currentLang.translationsList.FindIndex(t => t.key.Equals(newKey));
            if (index != -1)
            {
                currentLang.translationsList.RemoveAt(index);
            }
            // remove key from other languages
            foreach(LanguageObject l in LangList)
            {
                index = l.translationsList.FindIndex(t => t.key.Equals(newKey));
                if (index != -1)
                {
                    l.translationsList.RemoveAt(index);
                }
                EditorUtility.SetDirty(l);
            }
            refreshLanguages();
            currentLang = LangList.Find(l => l.name.Equals(nameLang));
            KDPairsUI(rootVisualElement);
            //TextField newKeyValue = KDPairs.Q<TextField>(name = newKey);
            //KDPairs.Remove(newKeyValue);
            //KDPairsUI(rootVisualElement);
        }
    }

    private void KDModifyUI(VisualElement root)
    {
        KDKeyModify = rootVisualElement.Q<TextField>(name = "KDKeyModify");
        KDModifyButtons = rootVisualElement.Q(name = "KDModifyButtons");
        KDAddButton = KDModifyButtons.Q<Button>(name= "KDAddButton");
        KDRemoveButton = KDModifyButtons.Q<Button>(name = "KDRemoveButton");
    }

    private void KDPairsUI(VisualElement root)
    {
        KDPairs = rootVisualElement.Q<VisualElement>(name = "KDPairs");
        KDUpdate = rootVisualElement.Q<Button>(name = "KDUpdate");
    }

    private void DisplayKDPairEntrys(LanguageObject L)
    {
        KDPairs = rootVisualElement.Q<VisualElement>(name = "KDPairs");
        if(KDPairs != null)
        {
            KDPairs.Clear();
        }
        SelectLang = rootVisualElement.Q<DropdownField>(name = "LanguageSelect");
        if(L != null)
        {
            if(L.translationsList != null)
            {
                if (L.translationsList.Count > 0)
                {
                    L.translationsList = L.translationsList.Distinct().ToList();
                    foreach (var l in L.translationsList)
                    {
                        // Create Fields for user Input.
                        TextField keyValue = new TextField(name = l.key);
                        keyValue.label = l.key;
                        keyValue.value = l.value;
                        KDPairs.Add(keyValue);
                    }
                }
            }
        }
    }

    private void UpdateKDPairEntrys(LanguageObject L)
    {
        if(LanguageKDPairs.childCount > 0)
        {
            // Update Values in currently selected language
            List<VisualElement> ListKDPairs = LanguageKDPairs.Children().ToList();
            foreach(var kv in ListKDPairs) {
                TextField tempField = (TextField)kv;
                string keyString = tempField.label;
                string valueString = tempField.value;
                Debug.Log(keyString + " " + valueString);
                StringTranslation tempTranslation = new StringTranslation(keyString, valueString);
                int index = L.translationsList.FindIndex(t => t.key.Equals(tempTranslation.key));
                if(index != -1)
                {
                    L.translationsList[index] = tempTranslation;
                }
                else
                {
                    L.translationsList.Add(tempTranslation);
                }
            }

            // Update Other Languages
            LangList.ForEach(l => UpdateKDPairLanguage(L, l));
        }
    }

    private void KDUpdate_clicked()
    {
        //Debug.Log("Update Button Clicked!");
        UpdateKDPairLanguageAll();
        refreshLanguages();
    }

    private void UpdateKDPairLanguage(LanguageObject updatedLanguage, LanguageObject otherLanguage)
    {
        foreach(StringTranslation t in updatedLanguage.translationsList)
        {
            if(!otherLanguage.translationsList.Any(x => x.key == t.key))
            {
                otherLanguage.translationsList.Add(t);
            }
        }
    }

    private void UpdateKDPairLanguageAll()
    {
        if(SelectLang == null)
        {
            LanguageObject maxLang = LangList.OrderByDescending(x => x.translationsList.Count).First();
            LangList.ForEach(l => UpdateKDPairLanguage(maxLang, l));
        }
        else
        {
            LanguageObject currentLang = LangList.Find(l => l.name == SelectLang.value);
            UpdateKDPairEntrys(currentLang);
            LangList.ForEach(l => UpdateKDPairLanguage(currentLang, l));
        }
        LangList.ForEach(L => EditorUtility.SetDirty(L));
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    // Initialize List of Languages
    private void LangListUI(VisualElement root)
    {
        LanguageList = rootVisualElement.Q<VisualElement>(name = "LanguageList");
        if (LanguageList != null)
        {
            foreach (var l in LangList)
            {
                Label label = new Label(l.name);
                LanguageList.Add(label);
            }
        }
        else
        {
            Debug.Log("Language List Container not Found");
        }
    }

    // Initialize Name of Languages Dropdown
    private void LangNamesUI(VisualElement root)
    {
        SelectLang = rootVisualElement.Q<DropdownField>(name = "LanguageSelect");
        if(SelectLang != null)
        {
            foreach (var l in LangNames)
            {
                SelectLang.choices.Add(l);
            }
        }
        else
        {
            Debug.Log("Dropdown Field not Found.");
        }
    }

    private void SelectLangDropdown(VisualElement root, string v)
    {
        LanguageKDPairs = rootVisualElement.Q<VisualElement>(name = "KDPairs");
        SelectLang = rootVisualElement.Q<DropdownField>(name = "LanguageSelect");
        if (LanguageKDPairs != null)
        {
            refreshLanguages();
            if (LangList.Find(l => l.name == v))
            {
                LanguageObject tempLang = LangList.Find(l => l.name == v);
                if (SelectLang.value != null)
                {
                    string nameLang = SelectLang.value;
                    LanguageObject currentLang = LangList.Find(l => l.name.Equals(nameLang));
                    DisplayKDPairEntrys(currentLang);
                    DisplayKeys(currentLang);
                }
            }
            else
            {
                Debug.Log(v + "Language data not Found");
            }
        }
        else
        {
            Debug.Log("Key Data Field not found.");
        }
    }

    private void DisplayKeys(LanguageObject L)
    {
        KeysElement = rootVisualElement.Q<VisualElement>(name = "Keys");
        if (KeysElement != null)
        {
            KeysElement.Clear();
        }
        SelectLang = rootVisualElement.Q<DropdownField>(name = "LanguageSelect");
        if (L != null)
        {
            if (L.translationsList != null)
            {
                if (L.translationsList.Count > 0)
                {
                    L.translationsList = L.translationsList.Distinct().ToList();
                    int count = 1;
                    foreach (var l in L.translationsList)
                    {
                        // Create Fields for user Input.
                        TextField keyValue = new TextField(name = l.key);
                        keyValue.label = count.ToString();
                        keyValue.value = l.key;
                        KeysElement.Add(keyValue);
                        count++;
                    }
                }
            }
        }
    }

    private void GetAllLanguages()
    {
        refreshLanguages();
        LangListUI(rootVisualElement);
        LangNamesUI(rootVisualElement);
        KDPairsUI(rootVisualElement);
        UpdateKDPairLanguageAll();
        if (SelectLang != null)
        {
            if(SelectLang.value != null)
            {
                LanguageObject currentLang = LangList.Find(l => l.name.Equals(SelectLang.value));
                DisplayKDPairEntrys(currentLang);
            }
        }
    }

    private void refreshLanguages()
    {
        AssetDatabase.Refresh();
        if (LangList == null)
        {
            LangList = new List<LanguageObject>();
        }
        else
        {
            LangList.Clear();
        }
        if (LangNames == null)
        {
            LangNames = new List<string>();
        }
        else{
            LangNames.Clear();
        }
        var languages = Resources.LoadAll("LanguageData", typeof(LanguageObject)).Cast<LanguageObject>().ToList<LanguageObject>();
        if (languages != null)
        {
            foreach (var l in languages)
            {
                //Debug.Log(l.name);
                LangList.Add(l);
                LangNames.Add(l.name);
            }
            LangList = LangList.OrderBy(l => l.name).ToList();
            LangNames.Sort();
        }
        else
        {
            Debug.Log("Languages NOT FOUND");
        }
    }

    private void RefreshUI()
    {
        // Clear existing UI elements
        rootVisualElement.Clear();

        CreateGUI();

        // Repaint the window
        this.Repaint();
    }

    private void BuildUI()
    {
        // Implement UI building logic here
        GetAllLanguages();
    }

    private void OnProjectChange()
    {
        RefreshUI();
    }
}
