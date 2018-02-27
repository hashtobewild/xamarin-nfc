using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Nfc;
using Android.Nfc.Tech;
using Java.IO;

namespace Plugin.Nfc
{
    public class NfcDefTag : INfcDefTag
    {
        public bool IsWriteable { get; }
        public NfcDefRecord[] Records { get; }
        public string TagId {get;}

        private Tag _tag;
        public NfcDefTag(Tag tag , IEnumerable<NdefRecord> records, bool isWritable = false, string id = null)
        {
            _tag = tag;
            IsWriteable = isWritable;
            TagId = id;
            Records = records
                .Select(r => new AndroidNdefRecord(r))
                .ToArray();
        }
        
        public async ValueTask<bool> WriteMessage(NfcDefMessage message)
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
}