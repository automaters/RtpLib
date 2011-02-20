using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace RtpLib
{
    public class RtcpSenderReport : RtcpPacket
    {

        #region Public Methods

        public override void ParseData(Stream stream)
        {
            byte[] bytes = new byte[24];
            if (stream.Read(bytes, 0, bytes.Length) < bytes.Length)
                throw new InvalidDataException();

            this.Ssrc = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 0));
            this.NtpTimestamp = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(bytes, 4));
            this.RtpTimestamp = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 12));
            this.SenderPacketCount = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 16));
            this.SenderOctetCount = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 20));
        }

        #endregion

        #region Protected Methods

        protected override int GetByteCount()
        {
            return 24;
        }

        protected override void ToStreamInternal(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write((uint)IPAddress.HostToNetworkOrder((int)this.Ssrc));
            writer.Write((ulong)IPAddress.HostToNetworkOrder((long)this.NtpTimestamp));
            writer.Write((uint)IPAddress.HostToNetworkOrder((int)this.RtpTimestamp));
            writer.Write((uint)IPAddress.HostToNetworkOrder((int)this.SenderPacketCount));
            writer.Write((uint)IPAddress.HostToNetworkOrder((int)this.SenderOctetCount));
        }

        #endregion

        #region Properties

        public override Rtcp.PacketType PacketType
        {
            get { return Rtcp.PacketType.SenderReport; }
        }

        public uint Ssrc
        {
            get;
            set;
        }

        public ulong NtpTimestamp
        {
            get;
            set;
        }

        public uint RtpTimestamp
        {
            get;
            set;
        }

        public uint SenderPacketCount
        {
            get;
            set;
        }

        public uint SenderOctetCount
        {
            get;
            set;
        }

        #endregion

        #region ICloneable Implementation

        public override object Clone()
        {
            var packet = this.MemberwiseClone() as RtcpSenderReport;
            packet.Header = packet.Header.Clone() as RtcpHeader;
            return packet;
        }

        #endregion

    }
}
