using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OpenKh.Kh2;
using OpenKh.Unity.Settings;
using UnityEditor;
using UnityEngine;

namespace OpenKh.Unity.Exporter.Mson
{
    public class MotionClipImporter : AssetPostprocessor
    {
        #region Fields

        private ModelImporter importer;
        private MsonFile mson;

        #endregion

        #region Properties

        /// <summary>
        /// Get the post processor's current state
        /// </summary>
        public static bool IsActive => OpenKhPrefs.GetBool("MsonRename", true);
        /// <summary>
        /// Regex for matching animation clip names by which moveset motion names can be retrieved
        /// </summary>
        private static Regex MotionTagValidator { get; } = new(@"^(\w\d{3}(?:_?))+$");

        #endregion

        #region Unity methods

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
        }
        //  Process animation clips after importing
        private void OnPostprocessAnimation(GameObject root, AnimationClip clip)
        {
            if (!IsActive || importer == null || mson == null)
                return;

            RenameAnimationClip(root, ref clip);
        }

        #endregion

        /// <summary>
        /// Try to get the corresponding moveset motion name of the specified animation clip
        /// </summary>
        /// <param name="clip">Animation clip to get the corresponding moveset name</param>
        /// <param name="clipName">When this method returns, contains the moveset name of the animation clip; null if no moveset name is found</param>
        /// <returns>True if a corresponding moveset name was found</returns>
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
        /// <summary>
        /// Renames the specified animation clip of the <param name="root">root object</param> to its corresponding moveset motion name.
        /// </summary>
        /// <param name="root">Object/asset containing the specified animation clip</param>
        /// <param name="clip">The animation clip being renamed</param>
        private void RenameAnimationClip(Object root, ref AnimationClip clip)
        {
            if (!TryGetClipName(clip, out var clipName))
                return;

            //Debug.Log($"Renaming animation clip: {clip.name} -> {clipName}");
            
            //  Rename AnimationClip asset
            clip.name = $"{root.name}@{clipName}";
        }
    }

}
