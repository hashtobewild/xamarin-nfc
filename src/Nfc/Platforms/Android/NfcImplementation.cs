using System;
using System.Collections.Generic;
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
        private static IEnumerable<NfcTechnologyType> _defaultSupportedTechnologies = new[] { NfcTechnologyType.Ndef };
        private IEnumerable<NfcTechnologyType> _supportedTechnologies;
        public event EventHandler<TagDetectedEventArgs> TagDetected;
        public event EventHandler<TagErrorEventArgs> TagError;

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
            {
               TagError?.Invoke(null, new TagErrorEventArgs(new InvalidOperationException("NFC is not available")));
               return;
            }

            if (!IsEnabled())
            {
                ShowNfcSettingDialog();
                return;
            }

        
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                Bundle options = new Bundle();
                _nfcAdapter?.EnableReaderMode(CrossNfc.CurrentActivity, this, NfcReaderFlags.NfcA |  NfcReaderFlags.NfcB | NfcReaderFlags.NfcF | NfcReaderFlags.NfcV, options);
            }
            else
            {
                var techs = GetSupportedTechnologies(_supportedTechnologies);
                var tagDetected = new IntentFilter(NfcAdapter.ActionNdefDiscovered);
                tagDetected.AddDataType("*/*");
                var filters = new[] { tagDetected };
                var intent = new Intent(activity, activity.GetType()).AddFlags(ActivityFlags.SingleTop);
                var pendingIntent = PendingIntent.GetActivity(activity, 0, intent, 0);
                _nfcAdapter.EnableForegroundDispatch(activity, pendingIntent, filters, new[] { techs.ToArray() });
            }

        }

        public void StopListening()
        {
            var activity = CrossNfc.CurrentActivity;
            _supportedTechnologies = _defaultSupportedTechnologies;

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
            var actions = actionsToHandle;
            
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
            
            TagDetected?.Invoke(null, new TagDetectedEventArgs(new NfcDefTag(null, records)));
        }

        public void OnTagDiscovered(Tag tag)
        {
            var techs = tag.GetTechList();
            var supportedTechnologies = GetSupportedTechnologies(_supportedTechnologies ?? _defaultSupportedTechnologies);
            if (!supportedTechnologies.Any(m => techs.Contains(m)))
                return;
           
            try
            {
                var nfcTag = TagFactory.Create(techs, tag, supportedTechnologies);
                TagDetected?.Invoke(this, new TagDetectedEventArgs(nfcTag));
            }
            catch(Java.IO.IOException ex)
            {
                TagError?.Invoke(null, new TagErrorEventArgs(new NfcReadException(NfcReadError.SessionTimeout, ex)));
            }
            catch(Exception ex)
            {
                TagError?.Invoke(null, new TagErrorEventArgs(new NfcReadException(NfcReadError.TagResponseError, ex)));
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

        private static IEnumerable<string> GetSupportedTechnologies(IEnumerable<NfcTechnologyType> supportedTypes)
        {
            return supportedTypes.Select(type =>
            {
                switch (type)
                {
                    case NfcTechnologyType.Ndef:
                        return Java.Lang.Class.FromType(typeof(Ndef)).Name;
                    case NfcTechnologyType.MifareUltraLight:
                        return Java.Lang.Class.FromType(typeof(MifareUltralight)).Name;
                    case NfcTechnologyType.MifareClassic:
                        return Java.Lang.Class.FromType(typeof(MifareClassic)).Name;
                    default:
                        return String.Empty;
                }

            }).ToArray();
        }

        public void SetSupportedTechnologies(IEnumerable<NfcTechnologyType> supportedTechnologies) => _supportedTechnologies = supportedTechnologies;
    }
}