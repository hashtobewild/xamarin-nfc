using Plugin.Nfc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Toasts;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NfcSampleApp
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class UriRecordEntryPage : PageBase
	{
        private readonly INfcTag _tag;
		private UriRecordEntryViewModel _vm;
        public UriRecordEntryPage(INfcTag tag, bool shouldAddPackageRecord) : base(shouldAddPackageRecord)
		{
			InitializeComponent ();
            _tag = tag;
			_vm = new UriRecordEntryViewModel();
			this.BindingContext = _vm;
             ToolbarItems.Add(new ToolbarItem()
            {
                Text = "Close",
                Command = new Command(async () =>
                {
                   await Navigation.PopModalAsync();
                })
            });

			this.WriteButton.Command = new Command(async () =>
			{
				if (String.IsNullOrEmpty(_vm.Uri)) return;
				if (!Uri.IsWellFormedUriString(_vm.Uri, UriKind.RelativeOrAbsolute)) return;
				
				try
				{
                    var records = new List<NfcDefRecord>
                    {
                        CrossNfc.CurrentFactory.CreateUriRecord(new Uri(_vm.Uri))
                    };

                    if (ShouldAddPackageRecord)
                    {
                      records.Add(CrossNfc.CurrentFactory.CreateApplicationRecord(this.PackageName));
                    }

					await _tag.WriteMessage(new NfcDefMessage(records.ToArray()));
				}
				catch (Exception ex)
				{
					var notificator = DependencyService.Get<IToastNotificator>();

					var options = new NotificationOptions
					{
							Title = "Error",
							Description = ex.ToString()
					};

					var result = await notificator.Notify(options);
				}
			});
		}
	}

    public class UriRecordEntryViewModel : ObservableObject
	{
		private string _uri;
		public string Uri
		{
			get => _uri;
			set => SetProperty(ref _uri, value);
		}
	}
	
	
}