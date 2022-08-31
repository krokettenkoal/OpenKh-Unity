using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OpenKh.Kh2;
using OpenKh.Unity.Exporter.Aset.Motion;

namespace OpenKh.Unity.Exporter.Mson
{
    [Serializable]
    public class MsonFile : List<MotionClipInfo>
    {
        public string FilePath { get; private set; }

        #region Constructors

        public MsonFile() { }
        public MsonFile(string filePath)
        {
            FilePath = filePath;
        }
        public MsonFile(string filePath, IEnumerable<MotionClipInfo> motions) : base(motions)
        {
            FilePath = filePath;
        }
        public MsonFile(string filePath, IEnumerable<MotionInformation> motions) 
            : this(filePath, DummyFill(motions)) { }
        public MsonFile(string filePath, IEnumerable<Bar.Entry> bar) 
            : this(filePath, bar.Select((e, i) => new MotionClipInfo(i, e))) { }
        
        #endregion

        #region Private methods

        private static IEnumerable<MotionClipInfo> DummyFill(IEnumerable<MotionInformation> motions)
        {
            var clips = new List<MotionClipInfo>();
            var idx = 0;
            string prevTag = null;

            foreach (var motion in motions)
            {
                if (prevTag != null && prevTag != motion.motion.id)
                {
                    while (idx % 4 > 0)
                    {
                        var dummy = new MotionClipInfo(++idx);
                        clips.Add(dummy);
                    }
                }

                var info = new MotionClipInfo(idx++, motion);
                clips.Add(info);
                prevTag = motion.motion.id;
            }

            return clips;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Save MsonFile object to a JSON file
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrEmpty(FilePath))
                return;

            var dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText(FilePath, json);
        }
        /// <summary>
        /// Save MsonFile object to a JSON file at a specified path
        /// </summary>
        /// <param name="filePath">The path at which the file will be saved</param>
        public void Save(string filePath)
        {
            FilePath = filePath;
            Save();
        }

        #endregion

        #region Static methods

        /// <summary>
        /// Create a new MSON file at a specific path containing the specified motion clip infos
        /// </summary>
        /// <param name="filePath">Path to save the MSON file at</param>
        /// <param name="motions">Motion clip infos to write to the file</param>
        public static void Create(string filePath, IEnumerable<MotionClipInfo> motions) =>
            new MsonFile(filePath, motions).Save();
        /// <summary>
        /// Create a new MSON file at a specific path containing the specified motion infos
        /// </summary>
        /// <param name="filePath">Path to save the MSON file at</param>
        /// <param name="motions">Motion infos to write to the file</param>
        public static void Create(string filePath, IEnumerable<MotionInformation> motions) =>
            new MsonFile(filePath, motions).Save();
        /// <summary>
        /// Create a new MSON file at a specific path containing the specified BAR entries
        /// </summary>
        /// <param name="filePath">Path to save the MSON file at</param>
        /// <param name="bar">BAR entries to write to the file</param>
        public static void Create(string filePath, IEnumerable<Bar.Entry> bar) =>
            new MsonFile(filePath, bar).Save();

        /// <summary>
        /// Try to load an MSON data from a JSON file
        /// </summary>
        /// <param name="msonPath">Path to the JSON file to load</param>
        /// <param name="mson">The loaded MsonFile object</param>
        /// <returns>True if the file has been loaded successfully</returns>
        public static bool TryLoad(string msonPath, out MsonFile mson)
        {
            if (!File.Exists(msonPath))
            {
                mson = null;
                return false;
            }

            var json = File.ReadAllText(msonPath);
            mson = JsonConvert.DeserializeObject<MsonFile>(json);

            if (mson is null)
                return false;

            mson.FilePath = msonPath;
            
            return true;
        }

        #endregion

    }
}
