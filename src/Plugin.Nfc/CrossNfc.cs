using System;
using System.Threading;

namespace Plugin.Nfc
{
    public partial class CrossNfc
    {
        private static Lazy<INfc> _implementation = new Lazy<INfc>(CreateNfc, LazyThreadSafetyMode.PublicationOnly);
        private static readonly Lazy<INfcDefRecordFactory> _factory = new Lazy<INfcDefRecordFactory>(CreateFactory, LazyThreadSafetyMode.PublicationOnly);
        public static INfcDefRecordFactory CurrentFactory => _factory.Value;

        private static INfcDefRecordFactory CreateFactory()
        {
#if PORTABLE
            throw NotImplementedInReferenceAssembly();
#else
            return new NfcDefRecordFactoryImplementation();
#endif
        }

        public static INfc Current => _implementation.Value;

        private static INfc CreateNfc()
        {
#if PORTABLE
            throw NotImplementedInReferenceAssembly();
#else
            return new NfcImplementation();
#endif
        }

        public static void Dispose()
        {
            if (_implementation != null && _implementation.IsValueCreated)
            {
                _implementation = new Lazy<INfc>(CreateNfc, LazyThreadSafetyMode.PublicationOnly);
            }
        }

        private static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException("This functionality is not implemented in the portable version of this assembly. You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
        }
    }
}