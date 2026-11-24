using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace VerticalShooter.UI
{
    public abstract class ScreenController : MonoBehaviour
    {
        protected VisualElement root;
        private VisualElement safeArea;
        private readonly List<(LocalizedString value, LocalizedString.ChangeHandler handler)> strings =
            new List<(LocalizedString, LocalizedString.ChangeHandler)>();
        private Rect previousSafeArea;
        private int previousWidth;
        private int previousHeight;

        protected virtual void OnEnable()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            safeArea = root.Q<VisualElement>("safe-area");
            root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            ApplySafeArea();
        }

        protected virtual void OnDisable()
        {
            if (root != null)
                root.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            foreach (var item in strings)
                item.value.StringChanged -= item.handler;
            strings.Clear();
            root.Q<Button>("english").clicked -= SelectEnglish;
            root.Q<Button>("turkish").clicked -= SelectTurkish;
        }

        private void Update()
        {
            if (Screen.width != previousWidth || Screen.height != previousHeight || Screen.safeArea != previousSafeArea)
                ApplySafeArea();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt) => ApplySafeArea();

        private void ApplySafeArea()
        {
            if (safeArea == null || Screen.width <= 0 || Screen.height <= 0)
                return;
            previousWidth = Screen.width;
            previousHeight = Screen.height;
            previousSafeArea = Screen.safeArea;
            float scaleX = root.resolvedStyle.width / Screen.width;
            float scaleY = root.resolvedStyle.height / Screen.height;
            if (float.IsNaN(scaleX) || float.IsNaN(scaleY) || scaleX <= 0 || scaleY <= 0)
                return;
            safeArea.style.paddingLeft = previousSafeArea.xMin * scaleX;
            safeArea.style.paddingRight = (Screen.width - previousSafeArea.xMax) * scaleX;
            safeArea.style.paddingTop = (Screen.height - previousSafeArea.yMax) * scaleY;
            safeArea.style.paddingBottom = previousSafeArea.yMin * scaleY;
        }

        protected void Bind(string elementName, string key)
        {
            var element = root.Q<TextElement>(elementName);
            var localized = new LocalizedString("UI", key);
            LocalizedString.ChangeHandler handler = value => element.text = value;
            localized.StringChanged += handler;
            strings.Add((localized, handler));
        }

        protected void BindOptions()
        {
            Bind("options-title", "options");
            Bind("language-label", "language");
            Bind("options-back", "back");
            root.Q<Button>("english").clicked += SelectEnglish;
            root.Q<Button>("turkish").clicked += SelectTurkish;
        }

        private void SelectEnglish() => StartCoroutine(SelectLanguage("en"));
        private void SelectTurkish() => StartCoroutine(SelectLanguage("tr"));

        private IEnumerator SelectLanguage(string code)
        {
            yield return LocalizationSettings.InitializationOperation;
            var locale = LocalizationSettings.AvailableLocales.GetLocale(code);
            if (locale != null)
                LocalizationSettings.SelectedLocale = locale;
        }

        protected void Show(string elementName, bool visible)
        {
            root.Q<VisualElement>(elementName).EnableInClassList("hidden", !visible);
        }
    }
}
