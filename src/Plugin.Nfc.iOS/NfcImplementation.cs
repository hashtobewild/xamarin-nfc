using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoreFoundation;
using CoreNFC;
using Foundation;
using Plugin.Nfc.Abstractions;

namespace Plugin.Nfc
{
    public class NfcImplementation : NSObject, INfc
    {
        public event TagDetectedDelegate TagDetected;
        public ValueTask<bool> IsAvailableAsync()
        {
            return new ValueTask<bool>(NFCNdefReaderSession.ReadingAvailable);
        }

        public ValueTask<bool> IsEnabledAsync()
        {
            return new ValueTask<bool>(true);
        }

        public async Task StartListeningAsync(CancellationToken token = default(CancellationToken))
        {
            var reader = new NfcReader();
            var tag = await reader.ScanAsync(token);
            TagDetected?.Invoke(tag);
        }

        public async Task StopListeningAsync()
        {
            
        }

    }

    public class NfcReader : NSObject, INFCNdefReaderSessionDelegate
    {
        private NFCNdefReaderSession _session;
        private TaskCompletionSource<INfcDefTag> _tcs;

        public Task<INfcDefTag> ScanAsync(CancellationToken token = default(CancellationToken))
        {
            if (!NFCNdefReaderSession.ReadingAvailable)
            {
                throw new InvalidOperationException("Reading NDEF is not available");
            }

            _tcs = new TaskCompletionSource<INfcDefTag>();
            _session = new NFCNdefReaderSession(this, DispatchQueue.CurrentQueue, true);
            _session.BeginSession();
            
            token.Register(() =>
            {
                _session.InvalidateSession(); 
            });
            return _tcs.Task;
        }

        public void DidInvalidate(NFCNdefReaderSession session, NSError error)
        {
            if (error == null)
            {
                _tcs.SetResult(null);
                return;
            }
            
            var readerError = (NFCReaderError)(long)error.Code;
            if (readerError != NFCReaderError.ReaderSessionInvalidationErrorFirstNDEFTagRead &&
                readerError != NFCReaderError.ReaderSessionInvalidationErrorUserCanceled)
            {
                _tcs.TrySetException(new Exception(error.LocalizedFailureReason));
            }
            else if (readerError == NFCReaderError.ReaderSessionInvalidationErrorUserCanceled)
            {
                _tcs.TrySetCanceled();
            }
            else
            {
                _tcs.TrySetException(new Exception(error.LocalizedFailureReason));
            }
          
            
        }

        public void DidDetect(NFCNdefReaderSession session, NFCNdefMessage[] messages)
        {
            _tcs.SetResult(new iOSNfcDefTag(messages));
        }
    }
    
    public class iOSNfcDefTag : INfcDefTag
    {
        public bool IsWriteable { get; }
        public NfcDefRecord[] Records { get; }

        public iOSNfcDefTag(IEnumerable<NFCNdefMessage> records)
        {
            IsWriteable = false;
            Records = records
                .SelectMany(r => r.Records.Select(m => new iOSNdefRecord(m)))
                .ToArray();
        }
    }

    public class iOSNdefRecord : NfcDefRecord
    {
        public iOSNdefRecord(NFCNdefPayload nativeRecord)
        {
            TypeNameFormat = GetTypeNameFormat(nativeRecord.TypeNameFormat);
            Payload = nativeRecord.Payload?.ToArray();
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