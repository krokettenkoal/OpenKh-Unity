using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OpenKh.Unity.Tools.IdxImg.ViewModels
{
    public abstract class NodeViewModel : EntryViewModel
    {
        public ObservableCollection<EntryViewModel> Children { get; }

        //  Represents Toggle value in OpenKh.Unity.Tools.IdxImg.MainWindow
        public override bool IsChecked
        {
            get => Children.All(c => c.IsChecked);
            set => Children.ForEach(c => c.IsChecked = value);
        }

        protected NodeViewModel(string name, IEnumerable<EntryViewModel> entries) :
            base(name)
        {
            Children = new ObservableCollection<EntryViewModel>(entries);
        }
    }
}
