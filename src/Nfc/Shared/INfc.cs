using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.Nfc
{
    public enum NfcTechnologyType
    {
        None,
        Ndef,
        MifareUltraLight,
        MifareClassic
    }

    public static class NfcRecordTypeConstants
    {
        /**
         * RTD Text type. For use with {@literal TNF_WELL_KNOWN}.
         * @see #TNF_WELL_KNOWN
         */
        public const string RTD_TEXT = "T";

        /**
         * RTD URI type. For use with {@literal TNF_WELL_KNOWN}.
         * @see #TNF_WELL_KNOWN
         */
        public const string RTD_URI = "U";

        /**
         * RTD Smart Poster type. For use with {@literal TNF_WELL_KNOWN}.
         * @see #TNF_WELL_KNOWN
         */
        public const string RTD_SMART_POSTER = "Sp";

        /**
         * RTD Alternative Carrier type. For use with {@literal TNF_WELL_KNOWN}.
         * @see #TNF_WELL_KNOWN
         */
        public const string RTD_ALTERNATIVE_CARRIER = "ac";

        /**
         * RTD Handover Carrier type. For use with {@literal TNF_WELL_KNOWN}.
         * @see #TNF_WELL_KNOWN
         */
        public const string RTD_HANDOVER_CARRIER = "Hc";

        /**
         * RTD Handover Request type. For use with {@literal TNF_WELL_KNOWN}.
         * @see #TNF_WELL_KNOWN
         */
        public const string RTD_HANDOVER_REQUEST =  "Hr";

        /**
         * RTD Handover Select type. For use with {@literal TNF_WELL_KNOWN}.
         * @see #TNF_WELL_KNOWN
         */
        public const string RTD_HANDOVER_SELECT = "Hs";

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
        public const string RTD_ANDROID_APP = "android.com:pkg";

        public const char RTD_MIFAREULTRALIGHT_NDEF_MESSAGE = 'N';

    }



    public class TagDetectedEventArgs : EventArgs
    {
        public TagDetectedEventArgs(INfcTag tag)
        {
            Tag = tag;
        }

        public INfcTag Tag {get; }
    }

    public class TagErrorEventArgs : EventArgs
    {
        public TagErrorEventArgs(Exception ex)
        {
            Exception = ex;
        }

        public Exception Exception {get;}
    }


    public interface INfc
    {
        event EventHandler<TagDetectedEventArgs> TagDetected;
        event EventHandler<TagErrorEventArgs> TagError;
        bool IsAvailable();
        bool IsEnabled();
        void StartListening();
        void StopListening();
        void SetSupportedTechnologies(IEnumerable<NfcTechnologyType> supportedTechnologies);
    }

    public interface INfcTag : IDisposable
    {
        string TagId { get;}
        bool IsWriteable { get; }
        bool HasNfcDefRecords {get;}
        NfcDefRecord[] Records { get; }
        Task<bool> WriteMessage(NfcDefMessage message);
        Task<bool> Lock();
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
                var isUnicode = Convert.ToBoolean((Payload[0] & 0x80));
                return (isUnicode) ?  Encoding.Unicode : Encoding.UTF8;
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
            int languageCodeLength = record.Payload[0] & 0x77;

#if NETSTANDARD1_0
            LanguageCode = "en";
#else

            LanguageCode = Encoding.ASCII.GetString(record.Payload, 1, languageCodeLength);
#endif
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
         private static Dictionary<byte, string> UriPrefixMap = new  Dictionary<byte, string>() {
                {0x00, ""}, // 0x00
                {0x01, "http://www."},
                {0x02, "https://www."},
                {0x03, "http://"},
                {0x04, "https://"},
                {0x05, "tel:"},
                {0x06, "mailto:"},
                {0x07, "ftp://anonymous:anonymous@"},
                {0x08, "ftp://ftp."},
                {0x09, "ftps://"},
                {0x0A, "sftp://"},
                {0x0B, "smb://"},
                {0x0C, "nfs://"},
                {0x0D, "ftp://"},
                {0x0E, "dav://"},
                {0x0F, "news:"},
                {0x10, "telnet://"},
                {0x11, "imap:"},
                {0x12, "rtsp://"},
                {0x13, "urn:"},
                {0x14, "pop:"},
                {0x15, "sip:"},
                {0x16, "sips:"},
                {0x17, "tftp:"},
                {0x18, "btspp://"},
                {0x19, "btl2cap://"},
                {0x1A, "btgoep://"},
                {0x1B, "tcpobex://"},
                {0x1C, "irdaobex://"},
                {0x1D, "file://"},
                {0x1E, "urn:epc:id:"},
                {0x1F, "urn:epc:tag:"},
                {0x20, "urn:epc:tag:"},
                {0x21, "urn:epc:raw:"},
                {0x22, "urn:epc:"}
             
        };

        public UriRecord(NfcDefRecord record)
        {
            if(UriPrefixMap.ContainsKey(record.Payload[0]))
            {
               Prefix = UriPrefixMap[record.Payload[0]];
            }

            var url = record.EncodedWith.GetString(record.Payload, 1, record.Payload.Length - 1);
            Url = !String.IsNullOrWhiteSpace(Prefix) ? new Uri(String.Format("{0}{1}", Prefix, url)) : new Uri(url);
        }

        public override NfcDefRecordType RecordType => NfcDefRecordType.Uri;

        public string Prefix {get;}
        public Uri Url {get;}
        public string UrlString => Url?.AbsoluteUri;
    }

    public interface INfcDefRecordConverter
    {
        INfcDefRecord ConvertFrom(NfcDefRecord record);
    }

    public class NfcDefRecordConverter : INfcDefRecordConverter
    {
        public INfcDefRecord ConvertFrom(NfcDefRecord record)
        {
            if(record == null) throw new ArgumentNullException(nameof(record));
            
            if(record.Payload == null || record.Payload.Length == 0) return new UnknownRecord();
            
            if(record.TypeInfo == null || record.TypeInfo.Length == 0) return new UnknownRecord();

            if(record.TypeNameFormat == NDefTypeNameFormat.Empty || record.TypeNameFormat == NDefTypeNameFormat.Unknown) return new UnknownRecord();


            var type = Encoding.UTF8.GetString(record.TypeInfo, 0, record.TypeInfo.Length);

            if(record.TypeNameFormat == NDefTypeNameFormat.AbsoluteUri)
            {
                return new UriRecord(record);
            }
           
            if(record.TypeNameFormat ==  NDefTypeNameFormat.External)
            {
                if(type == NfcRecordTypeConstants.RTD_ANDROID_APP)
                {
                    return new ApplicationRecord(record);
                }

                return new ExternalRecord(record);
            }
            
            if(record.TypeNameFormat == NDefTypeNameFormat.WellKnown)
            {
                if(type == NfcRecordTypeConstants.RTD_URI)
                {
                    return new UriRecord(record);
                }
                else if(type == NfcRecordTypeConstants.RTD_TEXT)
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
