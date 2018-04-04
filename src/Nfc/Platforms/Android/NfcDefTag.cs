using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Nfc;
using Android.Nfc.Tech;
using Java.IO;

namespace Plugin.Nfc
{
    public class NfcDefTag : INfcTag
    {
        public bool IsWriteable { get; }
        public NfcDefRecord[] Records { get; }
        public string TagId {get;}

        public bool HasNfcDefRecords => Records != null && Records.Length > 0;

        private Ndef _tag;
        public NfcDefTag(Ndef tag , IEnumerable<NdefRecord> records, string id = null)
        {
            _tag = tag;
            IsWriteable = tag?.IsWritable ?? false;
            TagId = id;
            Records = records
                .Select(r => new AndroidNdefRecord(r))
                .ToArray();
        }
        
        public async Task<bool> WriteMessage(NfcDefMessage message)
        {
            if (!IsWriteable) return false;
            if(message == null || message.Records.Length == 0) return false;
            if (_tag == null) return false;

            var records = message.Records.Cast<AndroidNdefRecord>().Select(m => m.ToNdefRecord()).ToArray();
            var msg = new NdefMessage(records);
            try
            {
                var ndef = _tag;

                if (ndef != null)
                {
                    await ndef.ConnectAsync();
                    try
                    {
                        if (ndef.MaxSize < msg.ToByteArray().Length)
                        {
                            return false;
                        }

                        if (!ndef.IsWritable)
                        {
                            return false;
                        }

                        await ndef.WriteNdefMessageAsync(msg);

                    }
                    finally
                    {
                        ndef.Close();
                    }

                    return true;
                }
                
                var nDefFormatableTag = NdefFormatable.Get(_tag.Tag);
                try
                {
                    await nDefFormatableTag.ConnectAsync();
                    try
                    {
                        nDefFormatableTag.Format(msg);
                    }
                    finally
                    {
                        nDefFormatableTag.Close();

                    }
                    //The data is written to the tag
                    return true;
                } catch (Exception ex) {
                    //Failed to format tag
                    return false;
                }
          
            } catch (Exception ex) {
                throw new NfcWriteException(ex);
            }
        }

        public void Dispose()
        {
            _tag?.Dispose();
            _tag = null;
        }

    }
}