using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Nfc;
using System.Threading;

namespace NfcSampleApp
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class NfcPage : ContentPage
	{
        private CancellationTokenSource _cancelSource;
        private INfcDefTag _tag;
        private DefTag _defTag = new DefTag();
		public NfcPage ()
		{
			InitializeComponent ();
             this.BindingContext = _defTag;
            NfcTagList.ItemsSource = _defTag.Records;

            CrossNfc.Current.TagDetected += Current_TagDetected;
            CrossNfc.Current.TagError += Current_TagError;

            this.Button.Command = new Command(async () =>
            {
               await CrossNfc.Current.StartListeningAsync();
            });

            this.WriteButton.Command = new Command(async () =>
            {


            });
		}

        private void Current_TagError(TagErrorEventArgs args)
        {
            Console.WriteLine(args.Exception?.ToString());
        }

        private void Current_TagDetected(TagDetectedEventArgs args)
        {
            _tag = args.Tag;

            _defTag.IsWritable = _tag.IsWriteable;
            _defTag.TagId = _tag.TagId;
            
            var records = new List<INfcDefRecord>();

            foreach(var record  in _tag.Records)
            {
                records.Add(CrossNfc.CurrentConverter.ConvertFrom(record));
            }

            _defTag.Records.AddRange(records);
        }

        protected async override void OnDisappearing()
        {
            base.OnDisappearing();
            CrossNfc.Current.TagDetected -= Current_TagDetected;
            CrossNfc.Current.TagError -= Current_TagError;
            await CrossNfc.Current.StopListeningAsync();

        }
    }

    public class DefTag : ObservableObject
    {
        public DefTag()
        {
            Records = new ObservableRangeCollection<INfcDefRecord>();
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

        public ObservableRangeCollection<INfcDefRecord> Records {get;}

    }
}