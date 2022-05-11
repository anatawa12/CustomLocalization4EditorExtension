using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace CustomLocalization4EditorExtension
{
    // TODO: add api to show dropdown menu
    // this class expects ISO-639-1/2 two/three letter language code and
    // optional ISO 3166-1 alpha-2 country codes. e.g. 'en', 'en-us', 'ja-jp'.
    public class Localization
    {
        [NotNull] private readonly string _inputAssetPath;
        [NotNull] private readonly string _currentLocale;
        [NotNull] private readonly string _defaultLocale;
        [CanBeNull] private Dictionary<string, LocalizationAsset> _locales;
        [CanBeNull] private LocalizationAsset _currentLocaleAsset;
        [CanBeNull] private LocalizationAsset _defaultLocaleAsset;
        private bool _initialized;

        /// <summary>
        /// Instantiate Localization
        /// </summary>
        /// <param name="localeAssetPath">
        /// The asset path or GUID of <code cref="LocalizationAsset">LocalizationAsset</code> or
        /// the asset path to directory contains <code cref="LocalizationAsset">LocalizationAsset</code>s.
        /// </param>
        /// <param name="defaultLocale">The fallback locale of localization.</param>
        /// <exception cref="ArgumentException">Both keyLocale and defaultLocale are null</exception>
        public Localization(
            [NotNull] string localeAssetPath,
            [NotNull] string defaultLocale) {
            _inputAssetPath = localeAssetPath;
            _defaultLocale = defaultLocale;
            _currentLocale = _defaultLocale;
        }

        /// <summary>
        /// Initializes the localization.
        /// This function will access assets so you should not call this in your constructor.
        /// Calling this function is not required.　If you don't call this manually,
        /// The first call of <c cref="Tr">Tr</c> will call this function.
        /// </summary>
        public void Setup()
        {
            // first, list-up locales
            _locales = null;
            void LoadDirectory(string pathOfDirectory)
            {
                try
                {
                    _locales = Directory.GetFiles($"{pathOfDirectory}")
                        .Select(AssetDatabase.LoadAssetAtPath<LocalizationAsset>)
                        .Where(asset => asset != null)
                        .ToDictionary(asset => asset.localeIsoCode, asset => asset);
                }
                catch (IOException e)
                {
                    Debug.LogError(e);
                }
                catch (AggregateException e)
                {
                    Debug.LogError($"locale duplicated: {e}");
                }
            }

            if (_inputAssetPath.EndsWith("/"))
            {
                // it's asset path to folder
                // folder: UnityEditor.DefaultAsset
                if (Directory.Exists(_inputAssetPath))
                    LoadDirectory(_inputAssetPath);
                else
                    Debug.LogError($"inputAssetPath not found: {_inputAssetPath}");
            }
            else if (IsGuid(_inputAssetPath))
            {
                // it's GUID to folder or LocalizationAsset
                string path = AssetDatabase.GUIDToAssetPath(_inputAssetPath);
                if (Directory.Exists(path))
                    LoadDirectory(path);
                else if (File.Exists(path))
                    LoadDirectory(Path.GetDirectoryName(path));
                else
                    Debug.LogError($"inputAssetPath not found: {_inputAssetPath}");
            }
            else
            {
                // it's asset path to LocalizationAsset
                if (File.Exists(_inputAssetPath))
                    LoadDirectory(Path.GetDirectoryName(_inputAssetPath));
                else
                    Debug.LogError($"inputAssetPath not found: {_inputAssetPath}");
            }

            // then, find current locale
            if (_locales != null)
            {
                _locales.TryGetValue(_currentLocale, out _currentLocaleAsset); 
                _locales.TryGetValue(_defaultLocale, out _defaultLocaleAsset);
                if (_defaultLocaleAsset == null)
                    Debug.LogError($"locale asset for default locale({_defaultLocale}) not found.");
            }
            _initialized = true;
        }

        /// <summary>
        /// Translate the specified text via this Localization setting
        /// </summary>
        /// <param name="key">
        /// The untranslated text. This should be english but can be changed via keyLocale of constructor.
        /// </param>
        /// <returns>The translated text or translation key</returns>
        [NotNull]
        public string Tr([NotNull] string key) => TryTr(key) ?? key;

        /// <summary>
        /// Translate the specified text via this Localization setting
        /// </summary>
        /// <param name="key">
        /// The untranslated text. This should be english but can be changed via keyLocale of constructor.
        /// </param>
        /// <returns>The translated text or null</returns>
        [CanBeNull]
        public string TryTr([NotNull] string key)
        {
            if (!_initialized)
                Setup();

            if (_currentLocaleAsset != null)
            {
                var localized = _currentLocaleAsset.GetLocalizedString(key);
                if (localized != key) return localized;
            }

            if (_defaultLocaleAsset != null)
            {
                var localized = _defaultLocaleAsset.GetLocalizedString(key);
                if (localized != key) return localized;
            }

            return null;
        }

        private static bool IsGuid([NotNull] string mayGuid)
        {
            int hexCnt = 0;
            foreach (var c in mayGuid)
            {
                if ('0' <= c && c <= '9') hexCnt++;
                else if ('a' <= c && c <= 'f') hexCnt++;
                else if ('A' <= c && c <= 'F') hexCnt++;
                else if (c == '-') {}
                else return false;
                if (hexCnt > 32) return false;
            }

            return hexCnt == 32;
        }
    }
}
