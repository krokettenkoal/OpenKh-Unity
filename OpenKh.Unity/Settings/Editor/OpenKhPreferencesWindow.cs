using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OpenKh.Unity.Settings
{
    public class OpenKhPreferencesWindow : EditorWindow
    {
        protected class PrefSection
        {
            public string Title => _titleLabel != null ? _titleLabel.text : _template.name[8..];
            public int Order => int.TryParse(_template.name.Substring(6, 2), out var o) ? o : 99;
            public TemplateContainer Container { get; }

            private readonly Label _titleLabel;
            private readonly VisualTreeAsset _template;

            public PrefSection(VisualTreeAsset template)
            {
                _template = template;
                Container = template.Instantiate();
                _titleLabel = Container.Q<Label>(className: "pref-section-title");
            }
        }

        #region Fields

        //  Visual tree assets
        [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;
        [SerializeField] private VisualTreeAsset m_PreferencesTreeAsset = default;
        [SerializeField] private VisualTreeAsset m_PreferencesPanelAsset = default;
        
        protected List<VisualTreeAsset> m_SectionTemplates;
        protected List<PrefSection> m_Sections;

        //  Views
        private VisualElement m_RootElement;
        protected TwoPaneSplitView m_MainPanel;
        private VisualElement m_PreferencesTree;
        private VisualElement m_PreferencesPanel;
        protected TreeView m_Tree;
        protected VisualElement m_Prefs;

        #endregion

        #region Properties

        protected PrefSection ActiveSection { get; set; }

        #endregion

        #region Menu entry

        [MenuItem("OpenKh/Settings/Preferences..", false, priority = 99)]
        public static void ShowSettingsWindow()
        {
            var wnd = GetWindow<OpenKhPreferencesWindow>();
            wnd.titleContent = new GUIContent("OpenKh Preferences");
        }

        #endregion

        /// <summary>
        /// Automatically called when creating/opening the window
        /// </summary>
        public void CreateGUI()
        {
            if (m_PreferencesTreeAsset == null || m_PreferencesPanelAsset == null)
                return;

            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;
            Init(root);
            BindViews();
            AddListeners();
        }

        /// <summary>
        /// Initialize the preference window
        /// </summary>
        /// <param name="root">The rootVisualElement of the current window to add all views to</param>
        protected virtual void Init(VisualElement root)
        {
            // Instantiate UXML
            m_RootElement = m_VisualTreeAsset.Instantiate();
            root.Add(m_RootElement);

            //  Instantiate views
            m_MainPanel = new TwoPaneSplitView(0, 260, TwoPaneSplitViewOrientation.Horizontal);
            m_PreferencesTree = m_PreferencesTreeAsset.Instantiate();
            m_PreferencesPanel = m_PreferencesPanelAsset.Instantiate();

            //  Assemble main view
            m_MainPanel.Add(m_PreferencesTree);
            m_MainPanel.Add(m_PreferencesPanel);
            rootVisualElement.Add(m_MainPanel);

            //  Query main containers
            m_Tree = m_PreferencesTree.Q<TreeView>("PreferencesTree");
            m_Prefs = m_PreferencesPanel.Q<VisualElement>("PreferencesPanel");
            
            //  Retrieve sections
            m_SectionTemplates = AssetDatabase.FindAssets("Prefs_")
                .Select(guid => AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToList();

            m_Sections = m_SectionTemplates.Select(t => new PrefSection(t)).OrderBy(s => s.Order).ToList();

            foreach (var section in m_Sections)
            {
                section.Container.AddToClassList("hidden");
                m_Prefs.Add(section.Container);
            }
        }
        /// <summary>
        /// Bind preference sections to the TreeView
        /// </summary>
        protected virtual void BindViews()
        {
            m_Tree.makeItem = () => new Label();
            m_Tree.bindItem = (element, index) =>
            {
                if (element is Label l)
                {
                    l.text += m_Tree.GetItemDataForIndex<PrefSection>(index).Title;
                }
            };

            m_Tree.SetRootItems(m_Sections.Select((p, i) => new TreeViewItemData<PrefSection>(i, p)).ToList());
            
            m_Tree.SetSelection(0);
            DisplaySection(0);
        }
        /// <summary>
        /// Add change listeners to the window
        /// </summary>
        protected virtual void AddListeners()
        {
            m_Tree.onSelectionChange += SectionSelected;
            m_PreferencesPanel.RegisterCallback<ChangeEvent<int>>(SettingChanged);
            m_PreferencesPanel.RegisterCallback<ChangeEvent<float>>(SettingChanged);
            m_PreferencesPanel.RegisterCallback<ChangeEvent<bool>>(SettingChanged);
            m_PreferencesPanel.RegisterCallback<ChangeEvent<string>>(SettingChanged);
        }

        /// <summary>
        /// Method to call when the section selection changed
        /// </summary>
        /// <param name="selection">IEnumerable containing the selected tree items</param>
        private void SectionSelected(IEnumerable<object> selection)
        {
            var section = selection.OfType<PrefSection>().FirstOrDefault();

            if (section is null)
                return;

            DisplaySection(section);
        }

        /// <summary>
        /// Displays the preferences section with the specified index.
        /// </summary>
        /// <param name="index">Tree index of the section to show</param>
        protected void DisplaySection(int index)
        {
            var section = m_Tree.GetItemDataForIndex<PrefSection>(index);

            if(section is null)
            {
                ActiveSection = null;
                return;
            }
            
            DisplaySection(section);
        }
        /// <summary>
        /// Displays the specified preference section.
        /// </summary>
        /// <param name="section">The preference section to display</param>
        protected void DisplaySection(PrefSection section)
        {
            ActiveSection = section;
            UpdateFieldValues();

            foreach (var prefSection in m_Sections)
            {
                prefSection.Container.EnableInClassList("hidden", prefSection.Title != section.Title);
            }
        }

        /// <summary>
        /// Updates all control fields' values to their corresponding preferences saved in the storage.
        /// </summary>
        protected void UpdateFieldValues()
        {
            ActiveSection?.Container.Query<IntegerField>().ForEach(field =>
            {
                if (OpenKhPrefs.TryGet(field.name, out int i))
                    field.value = i;
            });
            ActiveSection?.Container.Query<FloatField>().ForEach(field =>
            {
                if (OpenKhPrefs.TryGet(field.name, out float f))
                    field.value = f;
            });
            ActiveSection?.Container.Query<Toggle>().ForEach(field =>
            {
                if (OpenKhPrefs.TryGet(field.name, out bool b))
                    field.value = b;
            });
            ActiveSection?.Container.Query<TextField>().ForEach(field =>
            {
                if (OpenKhPrefs.TryGet(field.name, out string s))
                    field.value = s.Trim(' ');
            });
        }
        /// <summary>
        /// Callback handler when a preference value changes
        /// </summary>
        /// <typeparam name="T">Type of the value changed</typeparam>
        /// <param name="ev">ChangeEvent fired by the field that had its value changed</param>
        protected void SettingChanged<T>(ChangeEvent<T> ev)
        {
            if (ev.target is not VisualElement ve)
                return;

            OpenKhPrefs.Set(ve.name, ev.newValue);
        }
    }
}

