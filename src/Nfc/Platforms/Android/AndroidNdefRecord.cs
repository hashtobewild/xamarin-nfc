using Android.Nfc;

namespace Plugin.Nfc
{
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

       public short Tnf { get;}

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
                    return NDefTypeNameFormat.Unknown ;
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
}