using Plugin.Nfc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Nfc.SampleApp
{
    public class Record : ObservableObject
    {
        private string _typeFormat;
        public string TypeNameFormat
        {
            get => _typeFormat;
            set => SetProperty(ref _typeFormat, value);
        }

        private string _payload;
        public string Payload
        {
            get => _payload;
            set => SetProperty(ref _payload, value);
        }
    }

   
    public class DefTag : ObservableObject
    {
        public DefTag()
        {
            Records = new ObservableRangeCollection<Record>();
        }

        private string _tagId;
        public string TagId
        {
            get => _tagId;
            set => SetProperty(ref _tagId, value);
        }

        private bool _isWritable;
        public bool IsWritable
        {
            get => _isWritable;
            set => SetProperty(ref _isWritable, value);
        }

        public ObservableRangeCollection<Record> Records {get;}

    }

	public partial class MainPage : ContentPage
	{
        private CancellationTokenSource _cancelSource;
        private INfcDefTag _tag;
        private DefTag _defTag = new DefTag();
		public MainPage()
		{
			InitializeComponent();
            this.BindingContext = _defTag;
            this.NfcTags.ItemsSource = _defTag.Records;

            CrossNfc.Current.TagDetected += Current_TagDetected;

            this.Button.Command = new Command(async () =>
            {
               await CrossNfc.Current.StartListeningAsync();
            });

            this.WriteButton.Command = new Command(async () =>
            {


            });
		}

        private void Current_TagDetected(INfcDefTag tag)
        {
            _tag = tag;

            _defTag.IsWritable = _tag.IsWriteable;
            _defTag.TagId = _tag.TagId;
            
            var records = new List<Record>();
            foreach(var record  in _tag.Records)
            {
                records.Add(new Record()
                {
                    TypeNameFormat = record.TypeNameFormat.ToString(),
                    Payload =  System.Text.Encoding.Default.GetString(record.Payload)
                });
            }
        }

        protected async override void OnDisappearing()
        {
            base.OnDisappearing();
            CrossNfc.Current.TagDetected -= Current_TagDetected;
            await CrossNfc.Current.StopListeningAsync();

        }
    }
}
