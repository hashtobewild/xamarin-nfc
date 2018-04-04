using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Nfc;
using Android.Nfc.Tech;

namespace Plugin.Nfc
{
    public class MifareUltraLightTag : INfcTag
    {
        private MifareUltralight _tag;

        public MifareUltraLightTag(MifareUltralight tag, IEnumerable<NdefRecord> records)
        {
            _tag = tag;
            Records = records
                .Select(r => new AndroidNdefRecord(r))
                .ToArray();
        }

        public string TagId => null;

        public bool IsWriteable => true;

        public bool HasNfcDefRecords => Records != null && Records.Length > 0;

        public NfcDefRecord[] Records { get; }

        public void Dispose()
        {
            _tag?.Dispose();
            _tag = null;
        }

        public async Task<bool> WriteMessage(NfcDefMessage message)
        {
            if (!IsWriteable) return false;
            if (message == null || message.Records.Length == 0) return false;
            if (_tag == null) return false;

            var records = message.Records.Cast<AndroidNdefRecord>().Select(m => m.ToNdefRecord()).ToArray();
            var msg = new NdefMessage(records);
            var pageSize = MifareUltralight.PageSize;

            try
            {
                var mifare = _tag;

                if (mifare != null)
                {
                    await mifare.ConnectAsync();

                    try
                    {
                        var msgInBytes = msg.ToByteArray();
                        var msgLength = msgInBytes.Length;
                        var numberOfPages = (int)Math.Ceiling((decimal)msgLength / 4);
                        var pageNumber = 5;
                        await SetControlData(mifare, msgLength);

                        for (var i = 0; i < numberOfPages; i++)
                        {
                            var c = new byte[MifareUltralight.PageSize];
                            var sourceOffset = (i * pageSize);
                            var count = pageSize;
                            var offset = 0;

                            if (i == 0)
                            {
                                sourceOffset = 0;
                            }
                            else if (i == (numberOfPages - 1))
                            {
                                count = msgLength - sourceOffset;
                                offset = 0;
                            }

                            Buffer.BlockCopy(msgInBytes, sourceOffset, c, offset, count);
                            if (mifare.MaxTransceiveLength < c.Length)
                            {
                                return false;
                            }

                            await mifare.WritePageAsync(pageNumber, c);
                            pageNumber++;
                        }

                    }
                    finally
                    {
                        mifare.Close();
                    }

                   

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new NfcWriteException(ex);
            }
        }

        private static async Task SetControlData(MifareUltralight mifare, int msgLength)
        {
            var length = Convert.ToByte(msgLength);
            var type = Convert.ToByte(NfcRecordTypeConstants.RTD_MIFAREULTRALIGHT_NDEF_MESSAGE);
            var controldata = new byte[] { length, type, 0, 0 };
            await mifare.WritePageAsync(4, controldata);
        }

        public static NdefMessage GetMessage(IEnumerable<byte[]> data)
        {
            var array = data.ToArray();
            var r = ByteArrayExtensions.Combine(array);
            var control = array[0];
            var ln = (int)control[0];
            var t = Convert.ToChar(control[1]);

            if (t == NfcRecordTypeConstants.RTD_MIFAREULTRALIGHT_NDEF_MESSAGE)
            {
               var x = new byte[ln];
               Buffer.BlockCopy(r, MifareUltralight.PageSize, x, 0, ln);
               return new NdefMessage(x);
            }

            return null;

        }
    }
}