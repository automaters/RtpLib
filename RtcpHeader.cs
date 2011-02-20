using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace RtpLib
{
    public class RtcpHeader : ICloneable
    {

        #region Constructor

        public RtcpHeader()
        {
            this.Version = 2;
            this.PacketType = Rtcp.PacketType.Unknown;
        }

        #endregion

        #region Public Methods

        public void Parse(Stream stream)
        {
            byte[] bytes = new byte[4];
            if (stream.Read(bytes, 0, bytes.Length) < bytes.Length)
                throw new InvalidDataException();

            this.Version = (byte)((bytes[0] >> 6) & 0x03);
            this.IsPadded = (0 != (bytes[0] & 0x20));
            this.ItemCount = (byte)(bytes[0] & 0x1F);
            this.PacketType = (Rtcp.PacketType)bytes[1];
            this.OctetCount = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(bytes, 2));
        }

        public void ToStream(Stream stream)
        {
            byte first = (byte)((this.Version << 6) & ~0x03);
            if (this.IsPadded)
                first |= 0x20;
            first |= (byte)(this.ItemCount & 0x1F);

            stream.WriteByte(first);
            stream.WriteByte((byte)this.PacketType);
            stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)this.OctetCount)), 0, 2);
        }

        #endregion

        #region Properties

        public byte Version
        {
            get;
            set;
        }

        public bool IsPadded
        {
            get;
            set;
        }

        public byte ItemCount
        {
            get;
            set;
        }

        public Rtcp.PacketType PacketType
        {
            get;
            set;
        }

        public ushort OctetCount
        {
            get;
            set;
        }

        public int ByteCount
        {
            get { return (ushort)(this.OctetCount * 4); }
            set { this.OctetCount = (ushort)(value / 4); }
        }

        #endregion

        #region ICloneableImplementation

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion

    }
}
