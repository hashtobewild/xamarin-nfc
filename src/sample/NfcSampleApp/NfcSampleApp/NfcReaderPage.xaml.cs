using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Nfc;
using System.Threading;
using Plugin.Toasts;

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
		        ApplicationRecordTemplate = new DataTemplate(typeof(ApplicationRecordWriteCell)),
                ExternalRecordTemplate = new DataTemplate(typeof(ExternalRecordWriteCell)),
                MimeRecordTemplate = new DataTemplate(typeof(MimeRecordWriteCell)),
                TextRecordTemplate = new DataTemplate(typeof(TextRecordWriteCell)),
                UrlRecordTemplate = new DataTemplate(typeof(UriRecordWriteCell)),
                UnknownRecordTemplate = new DataTemplate(typeof(UnknownRecordCell))
		    };

            NfcTagList.ItemsSource = _defTag.Records;
         
		}

        private async void Current_TagError(TagErrorEventArgs args)
        {
            var notificator = DependencyService.Get<IToastNotificator>();

            var options = new NotificationOptions()
                        {
                            Title = "Error",
                            Description = args.Exception.ToString()
                        };

            var result = await notificator.Notify(options);
        }

        private async void Current_TagDetected(TagDetectedEventArgs args)
        {
           /* _tag = args.Tag;

            _defTag.IsWritable = _tag.IsWriteable;
            _defTag.TagId = _tag.TagId;
            
            var records = new List<INfcDefRecord>();

            foreach(var record  in _tag.Records)
            {
                records.Add(CrossNfc.CurrentConverter.ConvertFrom(record));
            }

            _defTag.Records.AddRange(records);*/
              var notificator = DependencyService.Get<IToastNotificator>();

              var options = new NotificationOptions()
                        {
                            Title = "Error",
                            Description = "Heelo"
                        };

            var result = await notificator.Notify(options);
        }

        protected override void OnAppearing()
        {
            CrossNfc.Current.TagDetected += Current_TagDetected;
            CrossNfc.Current.TagError += Current_TagError;
            if(CrossNfc.Current.IsAvailable())
            {
                CrossNfc.Current.StartListening();
            }

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            CrossNfc.Current.TagDetected -= Current_TagDetected;
            CrossNfc.Current.TagError -= Current_TagError;
            CrossNfc.Current.StopListening();

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