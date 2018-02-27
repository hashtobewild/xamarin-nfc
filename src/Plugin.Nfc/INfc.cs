using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.Nfc
{
    public static class NfcRecordTypeConstants
    {
        /**
         * RTD Text type. For use with {@literal TNF_WELL_KNOWN}.
         * @see #TNF_WELL_KNOWN
         */
        public static byte[] RTD_TEXT = {0x54};  // "T"

        /**
         * RTD URI type. For use with {@literal TNF_WELL_KNOWN}.
         * @see #TNF_WELL_KNOWN
         */
        public static byte[] RTD_URI = {0x55};   // "U"

        /**
         * RTD Smart Poster type. For use with {@literal TNF_WELL_KNOWN}.
         * @see #TNF_WELL_KNOWN
         */
        public static byte[] RTD_SMART_POSTER = {0x53, 0x70};  // "Sp"

        /**
         * RTD Alternative Carrier type. For use with {@literal TNF_WELL_KNOWN}.
         * @see #TNF_WELL_KNOWN
         */
        public static byte[] RTD_ALTERNATIVE_CARRIER = {0x61, 0x63};  // "ac"

        /**
         * RTD Handover Carrier type. For use with {@literal TNF_WELL_KNOWN}.
         * @see #TNF_WELL_KNOWN
         */
        public static byte[] RTD_HANDOVER_CARRIER = {0x48, 0x63};  // "Hc"

        /**
         * RTD Handover Request type. For use with {@literal TNF_WELL_KNOWN}.
         * @see #TNF_WELL_KNOWN
         */
        public static byte[] RTD_HANDOVER_REQUEST = {0x48, 0x72};  // "Hr"

        /**
         * RTD Handover Select type. For use with {@literal TNF_WELL_KNOWN}.
         * @see #TNF_WELL_KNOWN
         */
        public static byte[] RTD_HANDOVER_SELECT = {0x48, 0x73}; // "Hs"

        /**
         * RTD Android app type. For use with {@literal TNF_EXTERNAL}.
         * <p>
         * The payload of a record with type RTD_ANDROID_APP
         * should be the package name identifying an application.
         * Multiple RTD_ANDROID_APP records may be included
         * in a single {@link NdefMessage}.
         * <p>
         * Use {@link #createApplicationRecord(String)} to create
         * RTD_ANDROID_APP records.
         * @hide
         */
        public static byte[] RTD_ANDROID_APP = Encoding.UTF8.GetBytes("android.com:pkg");
    }

    public delegate void TagDetectedDelegate(INfcDefTag tag);

    public interface INfc
    {
        event TagDetectedDelegate TagDetected;
        ValueTask<bool> IsAvailableAsync();
        ValueTask<bool> IsEnabledAsync();
        Task StartListeningAsync(CancellationToken token = default(CancellationToken));
        Task StopListeningAsync();
    }

    public interface INfcDefTag
    {
        string TagId { get;}
        bool IsWriteable { get; }
        NfcDefRecord[] Records { get; }
        ValueTask<bool> WriteMessage(NfcDefMessage message);
    }

    public interface INfcDefRecordFactory
    {
        NfcDefRecord CreateApplicationRecord(string packageName);
        NfcDefRecord CreateMimeRecord(string mimeType, 
            byte[] mimeData);
        NfcDefRecord CreateExternalRecord(string domain, string type, byte[] data);
        NfcDefRecord CreateTextRecord(string languageCode,
            string text);
        NfcDefRecord CreateUriRecord(Uri uri);
    }
    
    public class NfcDefMessage
    {
        public NfcDefMessage(NfcDefRecord[] records)
        {
            Records = records;
        }
        
        public NfcDefRecord[] Records { get; }
    }

    public enum NDefTypeNameFormat
    {
        AbsoluteUri,
        Empty,
        Media,
        External,
        WellKnown,
        Unchanged,
        Unknown
    }

    
    public interface INfcDefRecord
    {
         NfcDefRecordType RecordType {get;}
    }

    public class NfcDefRecord
    {
        public NfcDefRecord() : this(false)
        {

        }

        public NfcDefRecord(bool isEmpty)
        { 
            this.IsEmpty = isEmpty;
        }

        public static NfcDefRecord Empty => new NfcDefRecord(true);

        public NDefTypeNameFormat TypeNameFormat { get; protected set; }
        public byte[] Payload { get; protected set; }
        public byte[] Id { get; protected set;}
        public byte[] TypeInfo { get; protected set;}
        public bool IsEmpty {get; protected set;}

        public Encoding EncodedWith
        {
            get
            {
                if(Payload == null || Payload.Length <= 0) return Encoding.UTF8;
                return ((Payload[0] & 0x80) == 0) ?  Encoding.Unicode : Encoding.UTF8;
            }
        }
       

    }

    public enum NfcDefRecordType
    {
        Unknown,
        Application,
        Mime,
        Text,
        External,
        Uri
     }

    public abstract class NfcRecord : INfcDefRecord
    {
        public abstract NfcDefRecordType RecordType {get;}
    }

    public class UnknownRecord : NfcRecord
    {
        public override NfcDefRecordType RecordType => NfcDefRecordType.Unknown;
    }

    public class TextRecord : NfcRecord
    {
        public TextRecord(NfcDefRecord record)
        {
            var encoding = record.EncodedWith;
            int languageCodeLength = record.Payload[0] & 0077;
            LanguageCode = Encoding.Unicode.GetString(record.Payload, 1, languageCodeLength);
            Text = encoding.GetString(record.Payload, languageCodeLength + 1, record.Payload.Length - languageCodeLength - 1);
        }

        public override NfcDefRecordType RecordType => NfcDefRecordType.Text;

        public string LanguageCode {get;}
        public string Text {get;}
    }

    public class ApplicationRecord : NfcRecord
    {
        public ApplicationRecord(NfcDefRecord record)
        {
            PackageName = Encoding.UTF8.GetString(record.Payload, 0, record.Payload.Length);
        }

        public override NfcDefRecordType RecordType => NfcDefRecordType.Application;

        public string PackageName {get;}
    }

    public class MimeRecord : NfcRecord
    {
        public MimeRecord(NfcDefRecord record)
        {
            MimeData = record.Payload;
            MimeType = Encoding.Unicode.GetString(record.TypeInfo,  0, record.TypeInfo.Length);
        }

        public override NfcDefRecordType RecordType => NfcDefRecordType.Mime;

        public string MimeType {get;}
        public byte[] MimeData {get;}
    }

    public class ExternalRecord : NfcRecord
    {
        public ExternalRecord(NfcDefRecord record)
        {
            Data = record.Payload;
            var domainAndType = Encoding.UTF8.GetString(record.TypeInfo,  0, record.TypeInfo.Length);
            var domainAndTypes = domainAndType.Split(':');

            if (domainAndTypes.Length > 0)
            {
                Domain = domainAndTypes[0];
                Type = domainAndTypes[1];
            }
        }


        public override NfcDefRecordType RecordType => NfcDefRecordType.External;

        public string Domain {get;}
        public string Type {get;}
        public byte[] Data {get;}
    }

    public class UriRecord : NfcRecord
    {
        public UriRecord(NfcDefRecord record)
        {
            Prefix = Encoding.UTF8.GetString(record.Payload, 0, 1);
            var url = Encoding.UTF8.GetString(record.Payload, 1, record.Payload.Length);
            Uri = new Uri(url);
        }

        public override NfcDefRecordType RecordType => NfcDefRecordType.Uri;

        public string Prefix {get;}
        public Uri Uri {get;}
    }

    public static class NfcDefPayloadParser
    {
        public static INfcDefRecord Parse(NfcDefRecord record)
        {
            if(record == null) throw new ArgumentNullException(nameof(record));
            
            if(record.Payload == null || record.Payload.Length == 0) return new UnknownRecord();
            
            if(record.TypeInfo == null || record.TypeInfo.Length == 0) return new UnknownRecord();

            if(record.TypeNameFormat == NDefTypeNameFormat.Empty || record.TypeNameFormat == NDefTypeNameFormat.Unknown) return new UnknownRecord();

            if(record.TypeNameFormat == NDefTypeNameFormat.AbsoluteUri)
            {
                return new UriRecord(record);
            }
           
            if(record.TypeNameFormat ==  NDefTypeNameFormat.External)
            {
                if(record.TypeInfo == NfcRecordTypeConstants.RTD_ANDROID_APP)
                {
                    return new ApplicationRecord(record);
                }

                return new ExternalRecord(record);
            }
            
            if(record.TypeNameFormat == NDefTypeNameFormat.WellKnown)
            {
                if(record.TypeInfo == NfcRecordTypeConstants.RTD_URI)
                {
                    return new UriRecord(record);
                }
                else if(record.TypeInfo == NfcRecordTypeConstants.RTD_TEXT)
                {
                    return new TextRecord(record);
                }
            }
            
            if(record.TypeNameFormat == NDefTypeNameFormat.Media)
            {
                return new MimeRecord(record);
            }
            
            return new UnknownRecord();
        }

     
    }
}
