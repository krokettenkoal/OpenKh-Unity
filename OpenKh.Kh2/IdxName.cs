using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenKh.Unity;

namespace OpenKh.Kh2
{
    public class IdxName
    {
        private static string _filenameDictionary = Path.Combine(OpenKhPath.PackageRoot, "OpenKh.Kh2/resources/kh2idx.txt");

        public static string[] Names = File.ReadAllLines(_filenameDictionary);

        private static Dictionary<long, string> _nameDictionary = Names
            .ToDictionary(name => IdxDictionary.GetHash(name), name => name);

        public Idx.Entry Entry { get; set; }
        public string Name { get; set; }

        public static IEnumerable<IdxName> GetNameEntries(IEnumerable<Idx.Entry> entries) => entries
            .Select(entry => new IdxName
            {
                Entry = entry,
                Name = Lookup(entry)
            });

        public static string Lookup(uint hash32, ushort hash16) =>
            _nameDictionary.TryGetValue(IdxDictionary.GetHash(hash32, hash16), out var name) ? name : null;

        public static string Lookup(Idx.Entry entry) =>
            _nameDictionary.TryGetValue(IdxDictionary.GetHash(entry), out var name) ? name : null;
    }
}
