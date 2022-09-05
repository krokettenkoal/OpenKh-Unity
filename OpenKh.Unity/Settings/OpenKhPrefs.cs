using System;
using UnityEditor;

namespace OpenKh.Unity.Settings
{
    /// <summary>
    /// Stores and accesses OpenKh preferences.
    /// </summary>
    public static class OpenKhPrefs
    {
        private const string _prefix = "OpenKh";

        /// <summary>
        /// Returns true if key exists in the preferences file.
        /// </summary>
        /// <param name="key">Name of key to check for.</param>
        /// <returns>True if the key exists in the storage</returns>
        public static bool HasKey(string key) => EditorPrefs.HasKey(_prefix + key);
        /// <summary>
        /// Removes key and its corresponding value from the preferences.
        /// </summary>
        /// <param name="key">The key to remove</param>
        public static void DeleteKey(string key) => EditorPrefs.DeleteKey(_prefix + key);

        #region Getters

        /// <summary>
        /// Try to get a generic value from the preference storage.
        /// </summary>
        /// <typeparam name="T">Type of the value to get from storage</typeparam>
        /// <param name="key">Name of the key to get the value from</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>True if the key exists and holds a value of the specified Type</returns>
        public static bool TryGet<T>(string key, out T value)
        {
            value = default;
            object val;

            if (!HasKey(key))
                return false;

            if (typeof(T) == typeof(int))
            {
                val = GetInt(key);
            }
            else if (typeof(T) == typeof(float))
            {
                val = GetFloat(key);
            }
            else if (typeof(T) == typeof(bool))
            {
                val = GetBool(key);
            }
            else if (typeof(T) == typeof(string))
            {
                val = GetString(key);
            }
            else
                return false;

            value = (T)Convert.ChangeType(val, typeof(T));
            return true;
        }

        //  int getters
        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        /// <param name="key">Name of key to read integer from.</param>
        /// <returns>The value stored in the preference file.</returns>
        public static int GetInt(string key) => EditorPrefs.GetInt(_prefix + key);
        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        /// <param name="key">Name of key to read integer from.</param>
        /// <param name="defaultValue">Integer value to return if the key is not in the storage.</param>
        /// <returns>The value stored in the preference file or the defaultValue if the requested int does not exist.</returns>
        public static int GetInt(string key, int defaultValue) => EditorPrefs.GetInt(_prefix + key, defaultValue);

        //  float getters
        /// <summary>
        /// Returns the float value corresponding to key if it exists in the preference file.
        /// </summary>
        /// <param name="key">Name of key to read float from.</param>
        /// <returns>The float value stored in the preference file or the defaultValue if the requested float does not exist.</returns>
        public static float GetFloat(string key) => EditorPrefs.GetFloat(_prefix + key);
        /// <summary>
        /// Returns the float value corresponding to key if it exists in the preference file.
        /// </summary>
        /// <param name="key">Name of key to read float from.</param>
        /// <param name="defaultValue">Float value to return if the key is not in the storage.</param>
        /// <returns>The float value stored in the preference file or the defaultValue if the requested float does not exist.</returns>
        public static float GetFloat(string key, float defaultValue) => EditorPrefs.GetFloat(_prefix + key, defaultValue);

        //  bool getters
        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        /// <param name="key">Name of key to read bool from.</param>
        /// <returns>The bool value stored in the preference file or the defaultValue if the requested bool does not exist.</returns>
        public static bool GetBool(string key) => EditorPrefs.GetBool(_prefix + key);
        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        /// <param name="key">Name of key to read bool from.</param>
        /// <param name="defaultValue">Bool value to return if the key is not in the storage.</param>
        /// <returns>The bool value stored in the preference file or the defaultValue if the requested bool does not exist.</returns>
        public static bool GetBool(string key, bool defaultValue) => EditorPrefs.GetBool(_prefix + key, defaultValue);

        //  string getters
        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        /// <param name="key">Name of key to read string from.</param>
        /// <returns>The string value stored in the preference file or the defaultValue if the requested string does not exist.</returns>
        public static string GetString(string key) => EditorPrefs.GetString(_prefix + key);
        /// <summary>
        /// Returns the value corresponding to key in the preference file if it exists.
        /// </summary>
        /// <param name="key">Name of key to read string from.</param>
        /// <param name="defaultValue">String value to return if the key is not in the storage.</param>
        /// <returns>The string value stored in the preference file or the defaultValue if the requested string does not exist.</returns>
        public static string GetString(string key, string defaultValue) => EditorPrefs.GetString(_prefix + key, defaultValue);

        #endregion

        #region Setters

        /// <summary>
        /// Sets the value of the preference identified by a key as Type <typeparam name="T">T</typeparam>.
        /// </summary>
        /// <typeparam name="T">Type of the value to set in storage</typeparam>
        /// <param name="key">Name of the key to set the value to</param>
        /// <param name="value">Value to write into the preferences storage associated with the specified <param name="key">key</param></param>
        /// <exception cref="ArgumentException">Thrown if <typeparam name="T">T</typeparam> is not a supported value Type.</exception>
        public static void Set<T>(string key, T value)
        {
            switch (value)
            {
                case int i:
                    SetInt(key, i);
                    break;
                case float f:
                    SetFloat(key, f);
                    break;
                case bool b:
                    SetBool(key, b);
                    break;
                case string s:
                    SetString(key, s);
                    break;
                default:
                    throw new ArgumentException($"{key} does not have a supported value type: {value.GetType()}");
            }
        }

        /// <summary>
        /// Sets the value of the preference identified by key as an integer.
        /// </summary>
        /// <param name="key">Name of key to write integer to.</param>
        /// <param name="value">Value of the integer to write into the storage.</param>
        public static void SetInt(string key, int value) => EditorPrefs.SetInt(_prefix + key, value);
        /// <summary>
        /// Sets the value of the preference identified by key as a float.
        /// </summary>
        /// <param name="key">Name of key to write float to.</param>
        /// <param name="value">Value of the float to write into the storage.</param>
        public static void SetFloat(string key, float value) => EditorPrefs.SetFloat(_prefix + key, value);
        /// <summary>
        /// Sets the value of the preference identified by key as a bool.
        /// </summary>
        /// <param name="key">Name of key to write bool to.</param>
        /// <param name="value">Value of the bool to write into the storage.</param>
        public static void SetBool(string key, bool value) => EditorPrefs.SetBool(_prefix + key, value);
        /// <summary>
        /// Sets the value of the preference identified by key as a string.
        /// </summary>
        /// <param name="key">Name of key to write string to.</param>
        /// <param name="value">Value of the string to write into the storage.</param>
        public static void SetString(string key, string value) => EditorPrefs.SetString(_prefix + key, value);

        #endregion
    }
}
