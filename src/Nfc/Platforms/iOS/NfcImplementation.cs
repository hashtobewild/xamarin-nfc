using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CoreNFC;
using Foundation;

namespace Plugin.Nfc
{
    public class NfcImplementation : NSObject, INfc
    {
        public event TagDetectedDelegate TagDetected;
        public event TagErrorDelegate TagError;

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
            try
            {
                var reader = new NfcReader();
                var tag = await reader.ScanAsync(token);
                TagDetected?.Invoke(new TagDetectedEventArgs(tag));
            }
            catch(Exception ex)
            {
                TagError?.Invoke(new TagErrorEventArgs(ex));
            }
        }

        public async Task StopListeningAsync()
        {
            
        }

    }
}