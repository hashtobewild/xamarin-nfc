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
	public partial class NfcReaderPage : ContentPage
	{

        private CancellationTokenSource _cancelSource;
        private INfcDefTag _tag;
        private DefTag _defTag = new DefTag();
		public NfcReaderPage ()
		{
			InitializeComponent ();

            this.Title = "Read";
            this.BindingContext = _defTag;
            NfcTagList.ItemTemplate = new NfcReaderListTemplate()
		    {
		        ApplicationRecordTemplate = new DataTemplate(typeof(ApplicationRecordCell)),
                ExternalRecordTemplate = new DataTemplate(typeof(ExternalRecordCell)),
                MimeRecordTemplate = new DataTemplate(typeof(MimeRecordCell)),
                TextRecordTemplate = new DataTemplate(typeof(TextRecordCell)),
                UrlRecordTemplate = new DataTemplate(typeof(UriRecordCell)),
                UnknownRecordTemplate = new DataTemplate(typeof(UnknownRecordCell))
		    };

            NfcTagList.ItemsSource = _defTag.Records;
         
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

        protected async override void OnAppearing()
        {
            CrossNfc.Current.TagDetected += Current_TagDetected;
            CrossNfc.Current.TagError += Current_TagError;
            await CrossNfc.Current.StartListeningAsync();

            base.OnAppearing();
        }

        protected async override void OnDisappearing()
        {
            base.OnDisappearing();
            CrossNfc.Current.TagDetected -= Current_TagDetected;
            CrossNfc.Current.TagError -= Current_TagError;
            await CrossNfc.Current.StopListeningAsync();

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
}