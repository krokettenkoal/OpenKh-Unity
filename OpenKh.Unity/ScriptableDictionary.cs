using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace OpenKh.Unity
{
    [Serializable]
    public abstract class ScriptableDictionary : ScriptableObject, ISerializationCallbackReceiver
    {
        //  int settings
        public Dictionary<string, int> IntSettings { get; } = new();
        [SerializeField] private List<string> _intKeys = new();
        [SerializeField] private List<int> _intValues = new();

        //  float settings
        public Dictionary<string, float> FloatSettings { get; } = new();
        [SerializeField] private List<string> _floatKeys = new();
        [SerializeField] private List<float> _floatValues = new();

        //  bool settings
        public Dictionary<string, bool> BoolSettings { get; } = new();
        [SerializeField] private List<string> _boolKeys = new();
        [SerializeField] private List<bool> _boolValues = new();

        //  string settings
        public Dictionary<string, string> StringSettings { get; } = new();
        [SerializeField] private List<string> _stringKeys = new();
        [SerializeField] private List<string> _stringValues = new();

        public void Set<T>(string key, T value)
        {
            switch (value)
            {
                case int i:
                    IntSettings[key] = i;
                    break;
                case float f:
                    FloatSettings[key] = f;
                    break;
                case bool b:
                    BoolSettings[key] = b;
                    break;
                case string s:
                    StringSettings[key] = s;
                    break;
                default: 
                    throw new ArgumentException($"{key} does not have a supported value type: {value.GetType()}");
            }
        }

        public bool TryGet<T>(string key, out T value)
        {
            value = default;
            object val;

            if (typeof(T) == typeof(int))
            {
                if (!IntSettings.ContainsKey(key))
                    return false;
                val = IntSettings[key];
            }
            else if (typeof(T) == typeof(float))
            {
                if (!FloatSettings.ContainsKey(key))
                    return false;
                val = FloatSettings[key];
            }
            else if (typeof(T) == typeof(bool))
            {
                if (!BoolSettings.ContainsKey(key))
                    return false;
                val = BoolSettings[key];
            }
            else if (typeof(T) == typeof(string))
            {
                if (!StringSettings.ContainsKey(key))
                    return false;
                val = StringSettings[key];
            }
            else
                return false;

            value = (T)Convert.ChangeType(val, typeof(T));
            return true;
        }

        public bool ContainsKey(string key)
        {
            return IntSettings.ContainsKey(key) ||
                   FloatSettings.ContainsKey(key) ||
                   BoolSettings.ContainsKey(key) ||
                   StringSettings.ContainsKey(key);
        }

        #region ISerializationCallbackReceiver

        public void OnBeforeSerialize()
        {
            _intKeys.Clear();
            _intValues.Clear();
            _floatKeys.Clear();
            _floatValues.Clear();
            _boolKeys.Clear();
            _boolValues.Clear();
            _stringKeys.Clear();
            _stringValues.Clear();

            //  int settings
            foreach (var setting in IntSettings)
            {
                _intKeys.Add(setting.Key);
                _intValues.Add(setting.Value);
            }

            //  float settings
            foreach (var setting in FloatSettings)
            {
                _floatKeys.Add(setting.Key);
                _floatValues.Add(setting.Value);
            }

            //  bool settings
            foreach (var setting in BoolSettings)
            {
                _boolKeys.Add(setting.Key);
                _boolValues.Add(setting.Value);
            }

            //  string settings
            foreach (var setting in StringSettings)
            {
                _stringKeys.Add(setting.Key);
                _stringValues.Add(setting.Value);
            }
        }
        public void OnAfterDeserialize()
        {
            IntSettings.Clear();
            FloatSettings.Clear();
            BoolSettings.Clear();
            StringSettings.Clear();

            //  int settings
            if (_intKeys.Count != _intValues.Count)
                throw new SerializationException(
                    $"Inconsistent int key/value count: {_intKeys.Count} keys, {_intValues.Count} values.");

            for (var i = 0; i < _intKeys.Count; i++)
            {
                IntSettings.Add(_intKeys[i], _intValues[i]);
            }

            //  float settings
            if (_floatKeys.Count != _floatValues.Count)
                throw new SerializationException(
                    $"Inconsistent float key/value count: {_floatKeys.Count} keys, {_floatValues.Count} values.");

            for (var i = 0; i < _floatKeys.Count; i++)
            {
                FloatSettings.Add(_floatKeys[i], _floatValues[i]);
            }

            //  bool settings
            if (_boolKeys.Count != _boolValues.Count)
                throw new SerializationException(
                    $"Inconsistent bool key/value count: {_boolKeys.Count} keys, {_boolValues.Count} values.");

            for (var i = 0; i < _boolKeys.Count; i++)
            {
                BoolSettings.Add(_boolKeys[i], _boolValues[i]);
            }

            //  string settings
            if (_stringKeys.Count != _stringValues.Count)
                throw new SerializationException(
                    $"Inconsistent string key/value count: {_stringKeys.Count} keys, {_stringValues.Count} values.");

            for (var i = 0; i < _stringKeys.Count; i++)
            {
                StringSettings.Add(_stringKeys[i], _stringValues[i]);
            }
        }

        #endregion

    }
}

