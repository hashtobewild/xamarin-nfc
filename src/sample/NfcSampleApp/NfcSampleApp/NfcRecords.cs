using System;

namespace NfcSampleApp
{
    public abstract class RecordWriter : ObservableObject
    {
        public bool IsWritable => true;
    }
    
    public class UriRecordWriter : RecordWriter
    {
        private string _url;
        public string Url
        {
            get => _url;
            set => this.SetProperty(ref _url, value);
        }
    }
    
    public class ExternalRecordWriter : RecordWriter
    {
        private string _domain;
        public string Domain
        {
            get => _domain;
            set => SetProperty(ref _domain, value);
        }
        private string _type;
        public string Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }
        
        public byte[] Data { get; set; }
    }
    
    public class TextRecordWriter : RecordWriter
    {
        private string _languageCode;
        private string _text;
        
        public string LanguageCode
        {
            get => _languageCode;
            set => SetProperty(ref _languageCode, value);
        }
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }
    }
    
    public class ApplicatioNRecordWriter : RecordWriter
    {
        private string _packageName;
        public string PackageName
        {
            get => _packageName;
            set => SetProperty(ref _packageName, value);
        }
        
    }
    
    public class MimeRecordWriter : RecordWriter
    {
        private string _mimeType;
        public string MimeType
        {
            get => _mimeType;
            set => SetProperty(ref _mimeType, value);
        }
        
        public byte[] MimeData { get; set; }
        
    }
}
