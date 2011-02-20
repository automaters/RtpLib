using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace RtpLib
{
    public class RtcpUnknown : RtcpPacket
    {

        #region Public Methods

        public override void ParseData(Stream stream)
        {
            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            this.Data = ms.GetBuffer();
        }

        protected override int GetByteCount()
        {
            return this.Data.Length;
        }

        protected override void ToStreamInternal(Stream stream)
        {
            stream.Write(this.Data, 0, this.Data.Length);
        }

        #endregion

        #region Properties

        public override Rtcp.PacketType PacketType
        {
            get { return Rtcp.PacketType.Unknown; }
        }

        public byte[] Data
        {
            get;
            set;
        }

        #endregion

        #region ICloneable Implementation

        public override object Clone()
        {
            var packet = this.MemberwiseClone() as RtcpUnknown;
            packet.Header = packet.Header.Clone() as RtcpHeader;
            packet.Data = new byte[this.Data.Length];
            this.Data.CopyTo(packet.Data, 0);
            return packet;
        }

        #endregion

    }
}
