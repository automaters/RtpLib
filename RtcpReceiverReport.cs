using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace RtpLib
{
    public class RtcpReceiverReport : RtcpPacket
    {

        #region Public Methods

        public override void ParseData(Stream stream)
        {
            byte[] bytes = new byte[28];
            int length = stream.Read(bytes, 0, bytes.Length);

            // The receiver report may only contain the SSRC, but may contain more
            // So as long as we have at least 4 bytes then we have valid data
            if (length < 4)
                throw new InvalidDataException();

            this.Ssrc = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 0));
            this.SsrcOnly = (length == bytes.Length);
            if (!this.SsrcOnly)
            {
                this.ReporteeSsrc = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 4));
                this.LossFraction = bytes[4];
                this.CumulativeLoss = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 4)) & 0x00FFFFFF;
                this.ExtendedHighestSequenceNumber = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 8));
                this.InterarrivalJitter = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 12));
                this.LastSenderReportTimestamp = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 16));
                this.LastSenderReportDelay = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 20));

                // Need to sign-extend this value.
                if ((0x00800000 & this.CumulativeLoss) != 0)
                {
                    this.CumulativeLoss |= 0xFF000000;
                }
            }
            else
            {
                this.ReporteeSsrc = 0;
                this.LossFraction = 0;
                this.CumulativeLoss = 0;
                this.ExtendedHighestSequenceNumber = 0;
                this.InterarrivalJitter = 0;
                this.LastSenderReportTimestamp = 0;
                this.LastSenderReportDelay = 0;
            }
        }

        #endregion

        #region Protected Methods

        protected override int GetByteCount()
        {
            return (this.SsrcOnly ? 4 : 24);
        }

        protected override void ToStreamInternal(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write((uint)IPAddress.HostToNetworkOrder((int)this.Ssrc));

            // If we're only writing the SSRC then we're done
            if (this.SsrcOnly)
                return;

            writer.Write((uint)IPAddress.HostToNetworkOrder((int)this.ReporteeSsrc));
            writer.Write(this.LossFraction);
            writer.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)this.CumulativeLoss)), 1, 3);
            writer.Write((uint)IPAddress.HostToNetworkOrder((int)this.ExtendedHighestSequenceNumber));
            writer.Write((uint)IPAddress.HostToNetworkOrder((int)this.InterarrivalJitter));
            writer.Write((uint)IPAddress.HostToNetworkOrder((int)this.LastSenderReportTimestamp));
            writer.Write((uint)IPAddress.HostToNetworkOrder((int)this.LastSenderReportDelay));
        }

        #endregion

        #region

        public override Rtcp.PacketType PacketType
        {
            get { return Rtcp.PacketType.ReceiverReport; }
        }

        public bool SsrcOnly
        {
            get;
            set;
        }

        public uint Ssrc
        {
            get;
            set;
        }

        public uint ReporteeSsrc
        {
            get;
            set;
        }

        public byte LossFraction
        {
            get;
            set;
        }

        public uint CumulativeLoss
        {
            get;
            set;
        }

        public uint ExtendedHighestSequenceNumber
        {
            get;
            set;
        }

        public uint InterarrivalJitter
        {
            get;
            set;
        }

        public uint LastSenderReportTimestamp
        {
            get;
            set;
        }

        public uint LastSenderReportDelay
        {
            get;
            set;
        }

#endregion

        #region ICloneable Implementation

        public override object Clone()
        {
            var packet = this.MemberwiseClone() as RtcpReceiverReport;
            packet.Header = packet.Header.Clone() as RtcpHeader;
            return packet;
        }

        #endregion

    }
}
