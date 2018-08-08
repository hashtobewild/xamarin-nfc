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
        public string TagId { get; }

        public bool HasNfcDefRecords => Records != null && Records.Length > 0;

        private Ndef _tag;
        public NfcDefTag(Ndef tag, IEnumerable<NdefRecord> records, string id = null)
        {
            _tag = tag;
            IsWriteable = tag?.IsWritable ?? false;
            TagId = GetTagId(tag.Tag);
            Records = records
                .Select(r => new AndroidNdefRecord(r))
                .ToArray();
        }

        private string GetTagId(Tag tag)
        {
            var uid = tag.GetId();

            if (uid != null && uid.Length > 0)
            {
                return BytesToHex(uid);
            }

            return null;
        }

        private static char[] hexArray = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
        public static string BytesToHex(byte[] bytes)
        {
            char[] hexChars = new char[bytes.Length * 2];
            int v;
            for (int j = 0; j < bytes.Length; j++)
            {
                v = bytes[j] & 0xFF;
                hexChars[j * 2] = hexArray[v >> 4];
                hexChars[j * 2 + 1] = hexArray[v & 0x0F];
            }
            return new string(hexChars);
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

        public async Task<bool> Lock()
        {
            if (!IsWriteable) return true;
            if (_tag == null) return true;

            try
            {
                await _tag.ConnectAsync();
                return await _tag.MakeReadOnlyAsync();
            }
            catch (Exception ex)
            {
                throw new NfcWriteException(ex);
            }
            finally
            {
                _tag.Close();
            }
        }
          
    }
}