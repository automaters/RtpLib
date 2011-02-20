using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace RtpLib
{
    public class RtcpBye : RtcpPacket
    {

        #region Public Methods

        public override void ParseData(Stream stream)
        {
            byte[] bytes = new byte[4];
            if (stream.Read(bytes, 0, bytes.Length) < bytes.Length)
                throw new InvalidDataException();

            this.Ssrc = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 0));
        }

        protected override int GetByteCount()
        {
            return 4;
        }

        protected override void ToStreamInternal(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write((uint)IPAddress.HostToNetworkOrder((int)this.Ssrc));
        }

        #endregion

        #region Properties

        public override Rtcp.PacketType PacketType
        {
            get { return Rtcp.PacketType.Bye; }
        }

        public uint Ssrc
        {
            get;
            set;
        }

        #endregion

        #region ICloneable Implementation

        public override object Clone()
        {
            var packet = this.MemberwiseClone() as RtcpBye;
            packet.Header = packet.Header.Clone() as RtcpHeader;
            return packet;
        }

        #endregion

    }
}
