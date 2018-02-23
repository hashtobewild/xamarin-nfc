using System;
using System.Threading;
using System.Threading.Tasks;

namespace Plugin.Nfc.Abstractions
{
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
        bool IsWriteable { get; }
        NfcDefRecord[] Records { get; }
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

    public class NfcDefRecord
    {
        public NDefTypeNameFormat TypeNameFormat { get; set; }
        public byte[] Payload { get; set; }
    }
}
