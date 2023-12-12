using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLanguage", menuName = "Localization/Language")]

public class LanguageObject : ScriptableObject, ISerializationCallbackReceiver
{
    public Dictionary<string, string> translations;

    [System.Serializable]
    public class StringTranslation
    {
        public string key;
        public string value;

        public StringTranslation(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

        public bool equalKey(StringTranslation s)
        {
            if(this.key == s.key)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public List<StringTranslation> translationsList = new List<StringTranslation>();

    public void OnBeforeSerialize()
    {
        // Nothing Here
    }
    public void OnAfterDeserialize()
    {
        translations = new Dictionary<string, string>();
        foreach (StringTranslation translation in translationsList)
        {
            if (!translations.ContainsKey(translation.key))
            {
                translations.Add(translation.key, translation.value);
            }
            else
            {
                translations[translation.key] = translation.value;
            }
        }
    }

}