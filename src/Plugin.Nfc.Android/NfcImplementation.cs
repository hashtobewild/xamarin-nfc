using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Nfc;
using Android.Nfc.Tech;
using Android.OS;
using Java.IO;
using Plugin.Nfc.Abstractions;

namespace Plugin.Nfc
{
    internal class NfcImplementation : Java.Lang.Object, INfc, NfcAdapter.IReaderCallback
    {
        private readonly NfcAdapter _nfcAdapter;
        public event TagDetectedDelegate TagDetected;

        public NfcImplementation()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat)
                return;
            _nfcAdapter = NfcAdapter.GetDefaultAdapter(CrossNfc.CurrentActivity);
        }

        public ValueTask<bool> IsAvailableAsync()
        {
            var context = Application.Context;
            return context.CheckCallingOrSelfPermission(Manifest.Permission.Nfc) != Permission.Granted ? new ValueTask<bool>(false) : new ValueTask<bool>(_nfcAdapter != null);
        }

        public ValueTask<bool> IsEnabledAsync()
        {
            return new ValueTask<bool>(_nfcAdapter?.IsEnabled ?? false);
        }

        public async Task StartListeningAsync(CancellationToken token = default(CancellationToken))
        {
            if (!await IsAvailableAsync())
                throw new InvalidOperationException("NFC not available");

            if (!await IsEnabledAsync()) // todo: offer possibility to open dialog
                throw new InvalidOperationException("NFC is not enabled");

            token.Register(() =>
            {
                _nfcAdapter?.DisableForegroundDispatch(CrossNfc.CurrentActivity);
            });
            
            var activity = CrossNfc.CurrentActivity;
            var tagDetected = new IntentFilter(NfcAdapter.ActionNdefDiscovered);
            tagDetected.AddDataType("*/*");
            var filters = new[] { tagDetected };
            var intent = new Intent(activity, activity.GetType()).AddFlags(ActivityFlags.SingleTop);
            var pendingIntent = PendingIntent.GetActivity(activity, 0, intent, 0);
            _nfcAdapter.EnableForegroundDispatch(activity, pendingIntent, filters, new[] { new[] { Java.Lang.Class.FromType(typeof(Ndef)).Name } });
        }

        public async Task StopListeningAsync()
        {
            await Task.Run(() => { _nfcAdapter?.DisableForegroundDispatch(CrossNfc.CurrentActivity); });
        }

        internal void CheckForNfcMessage(Intent intent)
        {
            if (intent == null || !NfcAdapter.ActionNdefDiscovered.Equals(intent.Action)) return;

            if (intent.GetParcelableExtra(NfcAdapter.ExtraTag) is Tag tag)
            {
                OnTagDiscovered(tag);
                return;
            }
            
            var nativeMessages = intent.GetParcelableArrayExtra(NfcAdapter.ExtraNdefMessages);
            if (nativeMessages == null)
                return;

            var records = nativeMessages
                .Cast<NdefMessage>()
                .SelectMany(m => m.GetRecords().Select(r => r))
                .ToArray();
            
            TagDetected?.Invoke(new NfcDefTag(null, records));
        }

        public void OnTagDiscovered(Tag tag)
        {
            var techs = tag.GetTechList();
            if (!techs.Contains(Java.Lang.Class.FromType(typeof(Ndef)).Name))
                return;

            var ndef = Ndef.Get(tag);
            ndef.Connect();
            var ndefMessage = ndef.NdefMessage;
            var records = ndefMessage.GetRecords();
            ndef.Close();
            var nfcTag = new NfcDefTag(tag, ndef?.IsWritable ?? false, records);
            TagDetected?.Invoke(nfcTag);
        }
    }

    public class NfcDefTag : INfcDefTag
    {
        public bool IsWriteable { get; }
        public NfcDefRecord[] Records { get; }
        private Tag _tag;
        public NfcDefTag(Tag tag , bool isWritable, IEnumerable<NdefRecord> records)
        {
            _tag = tag;
            IsWriteable = isWritable;
            Records = records
                .Select(r => new AndroidNdefRecord(r))
                .ToArray();
        }
        
        public async Task<bool> WriteMessage(NfcDefMessage message)
        {
            if (!IsWriteable) return false;
            var records = message.Records.Cast<AndroidNdefRecord>().Select(m => m.ToNdefRecord()).ToArray();
            var msg = new NdefMessage(records);
            try
            {
                var ndef = Ndef.Get(_tag);

                if (ndef != null)
                {
                    await ndef.ConnectAsync();
                    if (ndef.MaxSize < msg.ToByteArray().Length)
                    {
                        return false;
                    }

                    if (!ndef.IsWritable)
                    {
                        return false;
                    }
                    await ndef.WriteNdefMessageAsync(msg);
                    ndef.Close();
                    return true;
                }
                
                var nDefFormatableTag = NdefFormatable.Get(_tag);
                try
                {
                    await nDefFormatableTag.ConnectAsync();
                    nDefFormatableTag.Format(msg);
                    nDefFormatableTag.Close();
                    //The data is written to the tag
                    return true;
                } catch (IOException ex) {
                    //Failed to format tag
                    return false;
                }
          
            } catch (Exception ex) {
                throw new ApplicationException("Writing to Nfc Tag failed", ex);
            }
        }
    }

    public class AndroidNdefRecord : NfcDefRecord
    {
        public AndroidNdefRecord(NdefRecord nativeRecord)
        {
            TypeNameFormat = GetTypeNameFormat(nativeRecord.Tnf);
            Payload = nativeRecord.GetPayload();
            Id = nativeRecord.GetId();
            TypeInfo = nativeRecord.GetTypeInfo();
            Tnf = nativeRecord.Tnf;
        }

        public byte[] Id { get; }
        public byte[] TypeInfo { get; }
        public short Tnf { get; }

        private NDefTypeNameFormat GetTypeNameFormat(short nativeRecordTnf)
        {
            switch (nativeRecordTnf)
            {
                case NdefRecord.TnfAbsoluteUri:
                    return NDefTypeNameFormat.AbsoluteUri;
                case NdefRecord.TnfEmpty:
                    return NDefTypeNameFormat.Empty;
                case NdefRecord.TnfExternalType:
                    return NDefTypeNameFormat.External;
                case NdefRecord.TnfMimeMedia:
                    return NDefTypeNameFormat.Media;
                case NdefRecord.TnfUnchanged:
                    return NDefTypeNameFormat.Unchanged;
                case NdefRecord.TnfUnknown:
                    return NDefTypeNameFormat.Unchanged;
                case NdefRecord.TnfWellKnown:
                    return NDefTypeNameFormat.WellKnown;
            }

            return NDefTypeNameFormat.Unknown;
        }

        public NdefRecord ToNdefRecord()
        {
            return new NdefRecord(Tnf, TypeInfo, Id, Payload);
        }
    }

    public class AndroidNfcDefRecordFactory : INfcDefRecordFactory
    {
        public NfcDefRecord CreateApplicationRecord(string packageName)
        {
            var record = NdefRecord.CreateApplicationRecord(packageName);
            var r = new AndroidNdefRecord(record);
            return r;
        }

        public NfcDefRecord CreateMimeRecord(string mimeType, byte[] mimeData)
        {
            var record = NdefRecord.CreateMime(mimeType, mimeData);
            var r = new AndroidNdefRecord(record);
            return r;
        }

        public NfcDefRecord CreateExternalRecord(string domain, string type, byte[] data)
        {
            var record = NdefRecord.CreateExternal(domain, type, data);
            var r = new AndroidNdefRecord(record);
            return r;
        }

        public NfcDefRecord CreateTextRecord(string languageCode, string text)
        {
            var record = NdefRecord.CreateTextRecord(languageCode,text);
            var r = new AndroidNdefRecord(record);
            return r;
        }

        public NfcDefRecord CreateUriRecord(Uri uri)
        {
            var record = NdefRecord.CreateUri(uri.AbsoluteUri);
            var r = new AndroidNdefRecord(record);
            return r;
        }
    }
}