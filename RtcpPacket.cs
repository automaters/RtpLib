using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace RtpLib
{
    public abstract class RtcpPacket : ICloneable
    {
        
        #region Constructors

        public RtcpPacket()
            : this(null)
        {
        }

        public RtcpPacket(RtcpHeader header)
        {
            if (header == null)
                header = new RtcpHeader();

            this.Header = header;
        }

        public static RtcpPacket FromUdpBuffer(UdpBuffer buffer)
        {
            using (var stream = new MemoryStream(buffer.Data, 0, buffer.Size))
            {
                return FromStream(stream);
            }
        }

        public static RtcpPacket FromStream(Stream stream)
        {
            var header = new RtcpHeader();
            header.Parse(stream);

            RtcpPacket packet = Rtcp.CreatePacketType(header.PacketType);
            packet.Header = header;
            packet.ParseData(stream);

            // Verify the header has the right version
            if (packet.Header.Version != 2)
                throw new InvalidDataException();

            return packet;
        }

        #endregion

        #region Public Methods

        protected virtual void Parse(Stream stream)
        {
            this.Header.Parse(stream);
            this.ParseData(stream);
        }

        public abstract void ParseData(Stream stream);

        public virtual void ToStream(Stream stream)
        {
            // Override whatever the header packet type is with the correct packet type
            this.Header.PacketType = this.PacketType;
            this.Header.ByteCount = this.GetByteCount();
            this.Header.ToStream(stream);
            this.ToStreamInternal(stream);
        }

        protected abstract int GetByteCount();
        protected abstract void ToStreamInternal(Stream stream);

        #endregion

        #region Properties

        public RtcpHeader Header
        {
            get;
            protected set;
        }

        public abstract Rtcp.PacketType PacketType
        {
            get;
        }

        #endregion

        #region ICloneable Implementation

        public abstract object Clone();

        #endregion
    }
}
