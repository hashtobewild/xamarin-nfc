using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Nfc;
using Android.Nfc.Tech;
using Android.OS;

namespace Plugin.Nfc
{
    internal class NfcImplementation : Java.Lang.Object, INfc, NfcAdapter.IReaderCallback
    {
        private readonly NfcAdapter _nfcAdapter;
        public event TagDetectedDelegate TagDetected;
        public event TagErrorDelegate TagError;

        public NfcImplementation()
        {
            _nfcAdapter = NfcAdapter.GetDefaultAdapter(CrossNfc.CurrentActivity);
            
        }

        public bool IsAvailable()
        {
            var activity = CrossNfc.CurrentActivity;;
            return activity.CheckCallingOrSelfPermission(Manifest.Permission.Nfc) == Permission.Granted;
        }

        public bool IsEnabled()
        {
            return _nfcAdapter?.IsEnabled ?? false;
        }

        public void StartListening()
        {
            var activity = CrossNfc.CurrentActivity;
           
            if (!IsAvailable())
                throw new InvalidOperationException("NFC not available");

            if (!IsEnabled())
            {
                ShowNfcSettingDialog();
                return;
            }

        
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                Bundle options = new Bundle();
                _nfcAdapter?.EnableReaderMode(CrossNfc.CurrentActivity, this, NfcReaderFlags.NfcA |  NfcReaderFlags.NfcB | NfcReaderFlags.NfcBarcode | NfcReaderFlags.NfcF | NfcReaderFlags.NfcV, options);
            }
            else
            {
                var tagDetected = new IntentFilter(NfcAdapter.ActionNdefDiscovered);
                tagDetected.AddDataType("*/*");
                var filters = new[] { tagDetected };
                var intent = new Intent(activity, activity.GetType()).AddFlags(ActivityFlags.SingleTop);
                var pendingIntent = PendingIntent.GetActivity(activity, 0, intent, 0);
                _nfcAdapter.EnableForegroundDispatch(activity, pendingIntent, filters, new[] { new[] { Java.Lang.Class.FromType(typeof(Ndef)).Name } });
            }

        }

        public void StopListening()
        {
            var activity = CrossNfc.CurrentActivity;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                _nfcAdapter?.DisableReaderMode(CrossNfc.CurrentActivity);
            }
            else
            {
                _nfcAdapter?.DisableForegroundDispatch(CrossNfc.CurrentActivity);
            }
        }

        internal void CheckForNfcMessage(Intent intent, string[] actionsToHandle)
        {
            var actions = new[] {NfcAdapter.ActionNdefDiscovered.ToLower(),  NfcAdapter.ActionTechDiscovered.ToLower(),  NfcAdapter.ActionTagDiscovered.ToLower()};
            if(actionsToHandle != null)
            {
                actions = actionsToHandle;
            }
            
            if (intent == null || !actions.Contains(intent.Action.ToLower())) 
            {
                    return;
            };
            
            if (intent.GetParcelableExtra(NfcAdapter.ExtraTag) is Tag tag)
            {
                var tagId = intent.GetParcelableExtra(NfcAdapter.ExtraId) as Java.Lang.String;
                OnTagDiscovered(tag);
                return;
            }
            
            var nativeMessages = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);
            if (nativeMessages == null)
            {
                return;
            }

            var records = nativeMessages
                .Cast<NdefMessage>()
                .SelectMany(m => m.GetRecords().Select(r => r))
                .ToArray();
            
            TagDetected?.Invoke(new TagDetectedEventArgs(new NfcDefTag(null, records)));
        }

        public void OnTagDiscovered(Tag tag)
        {
            var techs = tag.GetTechList();
            if (!techs.Contains(Java.Lang.Class.FromType(typeof(Ndef)).Name))
                return;
           
            try
            {
               var ndef = Ndef.Get(tag);
               ndef.Connect();
               var ndefMessage = ndef.NdefMessage;

               if(ndefMessage == null)
               {
                 TagError?.Invoke(new TagErrorEventArgs(new NfcReadException("There is no data registered in the NFC tag")));
                 return;
               }

               var records = ndefMessage.GetRecords();
               ndef.Close();
               var isWritable = ndef?.IsWritable ?? false;
               var nfcTag = new NfcDefTag(ndef, records);
               TagDetected?.Invoke(new TagDetectedEventArgs(nfcTag));
            }
            catch(Exception ex)
            {
                TagError?.Invoke(new TagErrorEventArgs(new NfcReadException(ex)));
            }
        }

        private void ShowNfcSettingDialog()
        {
            var activity = CrossNfc.CurrentActivity;
            var builder = new AlertDialog.Builder(activity);
            builder.SetTitle(Resource.String.nfc_setting_title);
            builder.SetMessage(Resource.String.nfc_setting_message);
            builder.SetPositiveButton("Settings", (sender, e) => {
                 activity.StartActivity(new Intent(Android.Provider.Settings.ActionNfcSettings));
            });
            builder.SetNegativeButton("Close", (sender , e) =>
            {
               
            });

            builder.Show();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if(disposing)
            {
                _nfcAdapter.Dispose();
            }
        }
    }
}