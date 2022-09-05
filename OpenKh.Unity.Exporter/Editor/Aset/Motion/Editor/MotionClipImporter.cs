using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OpenKh.Kh2;
using OpenKh.Unity.Exporter.Mson;
using UnityEditor;
using UnityEngine;

namespace OpenKh.Unity.Aset.Motion
{
    public class MotionClipImporter : AssetPostprocessor
    {

        [MenuItem("OpenKh/Settings/Rename Motion Clips", false)]
        private static void ToggleMotionNames()
        {
            IsActive = !IsActive;

            var stat = IsActive ? "enabled" : "disabled";
            Debug.Log($"Motion Names {stat}. Please reimport models to apply/revert changes.");
        }

        [MenuItem("OpenKh/Settings/Rename Motion Clips", true)]
        private static bool ValidateToggleMotionNames()
        {
            Menu.SetChecked("OpenKh/Settings/Rename Motion Clips", IsActive);
            return true;
        }

        /// <summary>
        /// Defines the post processor's current state
        /// </summary>
        public static bool IsActive
        {
            get => EditorPrefs.GetBool("OpenKhMotionRename", false);
            private set => EditorPrefs.SetBool("OpenKhMotionRename", value);
        }

        private ModelImporter importer;
        private MsonFile mson;
        private Dictionary<string, MotionSet.MotionName> MotionTagReg { get; } = new();
        private static Regex MotionTagValidator { get; } = new(@"^(\w\d{3}(?:_?))+$");

        //  Initialize processor before import
        private void OnPreprocessAnimation()
        {
            if (!IsActive)
                return;

            if (assetImporter is not ModelImporter _importer)
                return;

            //  Already loaded
            if (mson != null)
                return;

            importer = _importer;

            var jsonPath = Path.ChangeExtension(importer.assetPath, ".json");
            if (!MsonFile.TryLoad(jsonPath, out mson))
            {
                Debug.LogWarning($"Motion JSON for '{importer.assetPath}' could not be parsed.");
            }

            //  TODO: Remove fragments
            #region Old code fragments
            
            /*
            if (importer.clipAnimations.Length == 0)
                return;

            var anims = importer.defaultClipAnimations;

            foreach (var clip in anims)
            {
                if (!TryGetClipName(clip, out var clipName))
                    continue;

                Debug.Log($"Renaming animation clip: {clip.name} -> {clipName}");
                clip.name = clipName;
            }

            importer.clipAnimations = anims;

            //Debug.Log("Motion Name processor initialized.");
        */

            #endregion

        }

        //  Process animation clips after importing
        private void OnPostprocessAnimation(GameObject root, AnimationClip clip)
        {
            if (!IsActive || importer == null || mson == null)
                return;

            RenameAnimationClip(root, ref clip);
        }

        private bool TryGetClipName(object clip, out string clipName)
        {
            var cname = clip.GetType().GetProperty("name")?.GetValue(clip, null) as string;

            if (string.IsNullOrEmpty(cname))
            {
                clipName = null;
                return false;
            }

            //  Process only clips matching the motion tag syntax (e.g. 'A000_A000' or 'A172')
            if (!MotionTagValidator.IsMatch(cname.Trim()))
            {
                Debug.Log($"Skipping unknown motion '{cname}'.");
                clipName = null;
                return false;
            }

            //  Search for clip in MSON
            var tag = cname.Split("_")[0];  //  Primitive tag trimming: no fancy Regex matching needed
            var motionInfo = mson.FirstOrDefault(minf => minf.Tag == tag);

            //  Skip unknown motions
            if (motionInfo is null)
            {
                Debug.Log($"No motion with tag '{tag}' found.");
                clipName = null;
                return false;
            }

            clipName = $"{motionInfo.Slot:D3}-{motionInfo.Name}";
            return true;
        }

        private void RenameAnimationClip(GameObject root, ref AnimationClip clip)
        {
            if (!TryGetClipName(clip, out var clipName))
                return;

            //Debug.Log($"Renaming animation clip: {clip.name} -> {clipName}");
            
            //  Rename AnimationClip asset
            clip.name = $"{root.name}@{clipName}";
        }
    }

}
