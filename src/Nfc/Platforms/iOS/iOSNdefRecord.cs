using CoreNFC;

namespace Plugin.Nfc
{
    public class iOSNdefRecord : NfcDefRecord
    {
        public iOSNdefRecord(NFCNdefPayload nativeRecord)
        {
            TypeNameFormat = GetTypeNameFormat(nativeRecord.TypeNameFormat);
            if(nativeRecord.Payload != null && nativeRecord.Payload.Length > 0)
            {
                Payload = nativeRecord.Payload?.ToArray();
            }
            
            if(nativeRecord.Type != null && nativeRecord.Type.Length > 0)
            {
                TypeInfo = nativeRecord.Type.ToArray();
            }
    
            if(nativeRecord.Identifier != null && nativeRecord.Identifier.Length > 0)
            {
                Id = nativeRecord.Identifier.ToArray();
            }
        }

        private NDefTypeNameFormat GetTypeNameFormat(NFCTypeNameFormat nativeTypeNameFormat)
        {
            switch (nativeTypeNameFormat)
            {
                case NFCTypeNameFormat.AbsoluteUri:
                    return NDefTypeNameFormat.AbsoluteUri;
                case NFCTypeNameFormat.Empty:
                    return NDefTypeNameFormat.Empty;
                case NFCTypeNameFormat.NFCExternal:
                    return NDefTypeNameFormat.External;
                case NFCTypeNameFormat.Media:
                    return NDefTypeNameFormat.Media;
                case NFCTypeNameFormat.Unchanged:
                    return NDefTypeNameFormat.Unchanged;
                case NFCTypeNameFormat.Unknown:
                    return NDefTypeNameFormat.Unchanged;
                case NFCTypeNameFormat.NFCWellKnown:
                    return NDefTypeNameFormat.WellKnown;
                default:
                    return NDefTypeNameFormat.Unknown;
            }
        }
    }
}