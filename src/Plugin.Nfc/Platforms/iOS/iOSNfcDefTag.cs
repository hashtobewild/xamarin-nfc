using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreNFC;

namespace Plugin.Nfc
{
    public class iOSNfcDefTag : INfcDefTag
    {
        public bool IsWriteable { get; }
        public NfcDefRecord[] Records { get; }

        public string TagId {get;}

        public iOSNfcDefTag(IEnumerable<NFCNdefMessage> records)
        {
            IsWriteable = false;
            Records = records
                .SelectMany(r => r.Records.Select(m => new iOSNdefRecord(m)))
                .ToArray();
        }

        public ValueTask<bool> WriteMessage(NfcDefMessage message)
        {
           return new ValueTask<bool>(true);
        }
    }
}