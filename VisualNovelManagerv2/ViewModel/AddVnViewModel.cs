using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using VisualNovelManagerv2.Design;

namespace VisualNovelManagerv2.ViewModel
{
    public class AddVnViewModel: ViewModelBase
    {
        private string _fileName;
        private readonly AddVnViewModelService _service;
        public RelayCommand GetFile { get; private set; }

        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                RaisePropertyChanged("FileName");
            }
        }

        public AddVnViewModel()
        {
            this.GetFile = new RelayCommand(() => Messenger.Default.Send(_service));
            _service = new AddVnViewModelService();
            _service.FilePicked = this.FilePicked;
        }

        private void FilePicked()
        {
            this.FileName = _service.PickedFileName;
        }
    }
}
