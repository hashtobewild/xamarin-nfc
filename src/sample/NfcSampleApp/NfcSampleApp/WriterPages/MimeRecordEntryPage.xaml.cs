using Plugin.Nfc;
using Plugin.Toasts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NfcSampleApp
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MimeRecordEntryPage : PageBase
	{
		private readonly INfcTag _tag;
		private TextRecordEntryViewModel _vm;

        public MimeRecordEntryPage(INfcTag tag, bool shouldAddPackageRecord) : base(shouldAddPackageRecord)
		{
			InitializeComponent ();
			_tag = tag;
			_vm = new TextRecordEntryViewModel();
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
				if (String.IsNullOrEmpty(_vm.Text)) return;
				
				try
				{
					var records = new List<NfcDefRecord>
					{
							CrossNfc.CurrentFactory.CreateMimeRecord("application/json", Encoding.UTF8.GetBytes(_vm.Text))
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
}