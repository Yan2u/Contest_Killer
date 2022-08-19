using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Contest_Killer.ViewModel
{
    public class BuiltInBackgroundItem : ViewModelBase
    {
        private ImageSource source;
        public ImageSource Source
        {
            get => source;
            set => Set(ref source, value);
        }
    }

    public class BackgroundPageViewModel : ViewModelBase
    {
        private ObservableCollection<BuiltInBackgroundItem> items;
        public ObservableCollection<BuiltInBackgroundItem> Items
        {
            get => items;
            set => Set(ref items, value);
        }

        private int selected;
        public int Selected
        {
            get => selected;
            set => Set(ref selected, value);
        }

        public BackgroundPageViewModel()
        {
            Items = new ObservableCollection<BuiltInBackgroundItem>();
        }
    }
}
