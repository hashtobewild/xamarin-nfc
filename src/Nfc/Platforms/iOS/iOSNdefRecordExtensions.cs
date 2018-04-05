using CoreNFC;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;

namespace Plugin.Nfc
{
    public static class iOSNdefRecordExtensions
    {
        private const byte FlatMb = (byte)0x80;
        private const byte FlagMe = (byte)0x40;
        private const byte FlagCf = (byte)0x20;
        private const byte FlagSr = (byte)0x10;
        private const byte FlagIl = (byte)0x08;
        private static byte[] EmptyByteArray = new byte[0];
        private const int MaxPayloadSize = 10 * (1 << 20);  // 10 MB payload limit
        private const short TnfAbsoluteUri = 3;
        private const short TnfEmpty = 0;
        private const short TnfExternalType = 4;
        private const short TnfMimeMedia = 2;
        private const short TnfUnchanged = 6;
        private const short TnfUnknown = 5;
        private const short TnfWellKnown = 1;
        private const short TnfReserved = 7;

        public static NfcDefRecord[] Create(byte[] data)
        {
            return GetNdefRecordFromData(data);
        }

        private static iOSNdefRecord[] GetNdefRecordFromData(byte[] data)
        {
            using (var stream = new MemoryStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                return Parse(stream, true);
            }
        }

        /**
    * Main record parsing method.<p>
    * Expects NdefMessage to begin immediately, allows trailing data.<p>
    * Currently has strict validation of all fields as per NDEF 1.0
    * specification section 2.5. We will attempt to keep this as strict as
    * possible to encourage well-formatted NDEF.<p>
    * Always returns 1 or more NdefRecord's, or throws FormatException.
    *
    * @param buffer ByteBuffer to read from
    * @param ignoreMbMe ignore MB and ME flags, and read only 1 complete record
    * @return one or more records
    * @throws FormatException on any parsing error
    */

        private static iOSNdefRecord[] Parse(MemoryStream buffer, bool ignoreMbMe)
        {
            var records = new List<iOSNdefRecord>();
            try
            {
                byte[] type = null;
                byte[] id = null;
                byte[] payload = null;
                var chunks = new List<byte[]>();
                var inChunk = false;
                short chunkTnf = -1;
                var me = false;
                while (!me)
                {
                    var flag = (byte)buffer.ReadByte();
                    var mb = (flag & FlatMb) != 0;
                    me = (flag & FlagMe) != 0;
                    var cf = (flag & FlagCf) != 0;
                    var sr = (flag & FlagSr) != 0;
                    var il = (flag & FlagIl) != 0;
                    short tnf = (short)(flag & 0x07);
                    if (!mb && records.Count == 0 && !inChunk && !ignoreMbMe)
                    {
                        throw new FormatException("expected MB flag");
                    }
                    else if (mb && (records.Count != 0 || inChunk) && !ignoreMbMe)
                    {
                        throw new FormatException("unexpected MB flag");
                    }
                    else if (inChunk && il)
                    {
                        throw new FormatException("unexpected IL flag in non-leading chunk");
                    }
                    else if (cf && me)
                    {
                        throw new FormatException("unexpected ME flag in non-trailing chunk");
                    }
                    else if (inChunk && tnf != TnfUnchanged)
                    {
                        throw new FormatException("expected TNF_UNCHANGED in non-leading chunk");
                    }
                    else if (!inChunk && tnf == TnfUnchanged)
                    {
                        throw new FormatException("" +
                                "unexpected TNF_UNCHANGED in first chunk or unchunked record");
                    }
                    int typeLength = (byte)buffer.ReadByte() & 0xFF;
                    long payloadLength = sr ? ((byte)buffer.ReadByte() & 0xFF) : (buffer.ReadByte() & 0xFFFFFFFFL);
                    int idLength = il ? (buffer.ReadByte() & 0xFF) : 0;
                    if (inChunk && typeLength != 0)
                    {
                        throw new FormatException("expected zero-length type in non-leading chunk");
                    }
                    if (!inChunk)
                    {
                        type = (typeLength > 0 ? new byte[typeLength] : EmptyByteArray);
                        id = (idLength > 0 ? new byte[idLength] : EmptyByteArray);
                        buffer.Read(type, 0, type.Length);
                        buffer.Read(id, 0, id.Length);
                    }

                    EnsureSanePayloadSize(payloadLength);
                    payload = (payloadLength > 0 ? new byte[(int)payloadLength] : EmptyByteArray);
                    buffer.Read(payload, 0, payload.Length);
                    if (cf && !inChunk)
                    {
                        // first chunk
                        if (typeLength == 0 && tnf != TnfUnknown)
                        {
                            throw new FormatException("expected non-zero type length in first chunk");
                        }
                        chunks.Clear();
                        chunkTnf = tnf;
                    }
                    if (cf || inChunk)
                    {
                        // any chunk
                        chunks.Add(payload);
                    }
                    if (!cf && inChunk)
                    {
                        // last chunk, flatten the payload
                        payloadLength = 0;
                        foreach (byte[] p in chunks)
                        {
                            payloadLength += p.Length;
                        }
                        EnsureSanePayloadSize(payloadLength);
                        payload = new byte[(int)payloadLength];
                        int i = 0;
                        foreach (byte[] p in chunks)
                        {
                            Buffer.BlockCopy(p, 0, payload, i, p.Length);
                            i += p.Length;
                        }
                        tnf = chunkTnf;
                    }
                    if (cf)
                    {
                        // more chunks to come
                        inChunk = true;
                        continue;
                    }
                    else
                    {
                        inChunk = false;
                    }
                    var error = ValidateTnf(tnf, type, id, payload);
                    if (error != null)
                    {
                        throw new FormatException(error);
                    }
                    records.Add(new iOSNdefRecord(payload, id, type, GetTnf(tnf)));

                    if (ignoreMbMe)
                    {  // for parsing a single NdefRecord
                        break;
                    }
                }
            }
            catch (StackOverflowException e)
            {
                throw new FormatException("expected more data", e);
            }

            return records.ToArray();
        }

        private static void EnsureSanePayloadSize(long size)
        {
            if (size > MaxPayloadSize)
            {
                throw new FormatException(
                        "payload above max limit: " + size + " > " + MaxPayloadSize);
            }
        }

        private static string ValidateTnf(short tnf, byte[] type, byte[] id, byte[] payload)
        {
            switch (tnf)
            {
                case TnfEmpty:
                    if (type.Length != 0 || id.Length != 0 || payload.Length != 0)
                    {
                        return "unexpected data in TNF_EMPTY record";
                    }
                    return null;
                case TnfWellKnown:
                case TnfMimeMedia:
                case TnfAbsoluteUri:
                case TnfExternalType:
                    return null;
                case TnfUnknown:
                case TnfReserved:
                    if (type.Length != 0)
                    {
                        return "unexpected type field in TnfUnknown or TnfReserved record";
                    }
                    return null;
                case TnfUnchanged:
                    return "unexpected TnfUnchanged in first chunk or logical record";
                default:
                    return $"unexpected tnf value: {tnf}";
            }
        }

        private static NFCTypeNameFormat GetTnf(short nativeRecordTnf)
        {
            switch (nativeRecordTnf)
            {
                case TnfAbsoluteUri:
                    return NFCTypeNameFormat.AbsoluteUri;
                case TnfEmpty:
                    return NFCTypeNameFormat.Empty;
                case TnfExternalType:
                    return NFCTypeNameFormat.NFCExternal;
                case TnfMimeMedia:
                    return NFCTypeNameFormat.Media;
                case TnfUnchanged:
                    return NFCTypeNameFormat.Unchanged;
                case TnfUnknown:
                    return NFCTypeNameFormat.Unknown;
                case TnfWellKnown:
                    return NFCTypeNameFormat.NFCWellKnown;
                default:
                    return NFCTypeNameFormat.Unknown;
            }
        }
    }
}
