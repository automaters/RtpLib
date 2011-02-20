using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RtpLib
{
    public static class Rtcp
    {
        public enum PacketType
        {
            Unknown = 0,
            SenderReport = 200,
            ReceiverReport = 201,
            SourceDescription = 202,
            Bye = 203,
            AppDefined = 204
        }

        public struct Stats
        {
            public uint HighestSequenceNumber;
            public uint ExpectedPacketCount;
            public uint ActualPacketCount;
            public ulong MostRecentPacketTime;
            public uint Jitter;
            public uint ServerSsrc;
        }

        private static readonly Dictionary<PacketType, RtcpPacket> RtcpPacketTypes = new Dictionary<PacketType, RtcpPacket>
            {
                {PacketType.ReceiverReport, new RtcpReceiverReport()},
                {PacketType.SenderReport, new RtcpSenderReport()},
                {PacketType.SourceDescription, new RtcpSourceDescription()},
                {PacketType.Bye, new RtcpBye()},
                {PacketType.AppDefined, new RtcpAppDefined()},
                {PacketType.Unknown, new RtcpUnknown()}
            };

        public static void RegisterPacketType<T>(PacketType type)
            where T : RtcpPacket, new()
        {
            lock (RtcpPacketTypes)
            {
                RtcpPacketTypes.Remove(type);
                RtcpPacketTypes.Add(type, new T());
            }
        }

        public static RtcpPacket CreatePacketType(PacketType type)
        {
            RtcpPacket packet = null;
            lock (RtcpPacketTypes)
            {
                if (!RtcpPacketTypes.TryGetValue(type, out packet))
                    return new RtcpUnknown();
            }

            return packet.Clone() as RtcpPacket;
        }

    }
}
