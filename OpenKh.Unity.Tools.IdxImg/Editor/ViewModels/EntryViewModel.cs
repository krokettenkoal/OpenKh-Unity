namespace OpenKh.Unity.Tools.IdxImg.ViewModels
{
    public abstract class EntryViewModel
    {
        public string Name { get; }
        //  Represents Toggle value in OpenKh.Unity.Tools.IdxImg.MainWindow
        public abstract bool IsChecked { get; set; }

        internal EntryViewModel(string name)
        {
            Name = name;
        }

        public abstract void Extract(string outputPath);
    }
}
