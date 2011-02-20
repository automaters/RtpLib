using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace RtpLib
{
    public class RtcpClient : IDisposable
    {

        #region Constructors

        public RtcpClient(RtpListener client)
            : this(client, new IPEndPoint(IPAddress.Any, 0))
        {
        }

        public RtcpClient(RtpListener client, int port)
            : this(client, new IPEndPoint(IPAddress.Any, port))
        {
        }

        public RtcpClient(RtpListener client, IPEndPoint localEndPoint)
        {
            this.RtpClient = client;
            this.Listener = new UdpListener(localEndPoint);
            this.Listener.ReceiveCallback = this.OnDataReceived;
        }

        #endregion

        #region Public Methods

        public void SendReceiverReport(Rtcp.Stats stats)
        {
            // Make sure we have the necessary information to send the report
            if (this.ServerSsrc == 0 && stats.ServerSsrc == 0)
                throw new InvalidOperationException();

            var report = new RtcpReceiverReport();
            report.Header.ItemCount = 1;
            report.Ssrc = this.Ssrc;
            report.SsrcOnly = this.UseShortReportFormat;
            report.ReporteeSsrc = (this.ServerSsrc != 0 ? this.ServerSsrc : stats.ServerSsrc);
            report.CumulativeLoss = stats.ExpectedPacketCount - stats.ActualPacketCount;
            report.LossFraction = (byte)((256 * report.CumulativeLoss) / stats.ExpectedPacketCount);
            report.ExtendedHighestSequenceNumber = stats.HighestSequenceNumber;
            report.InterarrivalJitter = stats.Jitter;
            report.LastSenderReportTimestamp = (uint)this.LastSenderReport.ToFileTimeUtc();
            report.LastSenderReportDelay = (uint)(DateTime.UtcNow.ToFileTimeUtc() - this.LastSenderReport.ToFileTimeUtc());
            this.SendPacket(report);
        }

        public void SendBye()
        {
            if (this.ServerSsrc == 0)
                throw new InvalidOperationException();

            var packet = new RtcpBye();
            packet.Ssrc = this.Ssrc;
            this.SendPacket(packet);
        }

        #endregion

        #region Protected Methods

        protected void SendPacket(RtcpPacket packet)
        {
            using (var ms = new MemoryStream())
            {
                packet.ToStream(ms);
                this.Listener.Send(ms.GetBuffer(), this.RemoteEndPoint);
            }
        }

        protected virtual void OnDataReceived(object sender, UdpBuffer buffer)
        {
            ThreadPool.QueueUserWorkItem((data) =>
            {
                try
                {
                    this.OnRtcpPacketReceived(RtcpPacket.FromUdpBuffer(buffer));
                }
                catch (InvalidDataException ex)
                {
                    Trace.TraceError("Error parsing RTCP packet: {0}", ex.ToString());
                }
            });
        }

        protected virtual void OnRtcpPacketReceived(RtcpPacket packet)
        {
            if (packet is RtcpSenderReport)
            {
                var report = packet as RtcpSenderReport;
                if (this.ServerSsrc == 0)
                    this.ServerSsrc = report.Ssrc;
                else if (this.ServerSsrc != report.Ssrc)
                {
                    Trace.TraceWarning("Received RTCP Sender Report from wrong server. Received {0}, Expected {1}.", report.Ssrc, this.ServerSsrc);
                    return;
                }

                this.LastNtpTimestamp = report.NtpTimestamp;
                this.LastSenderReport = DateTime.UtcNow;
            }
        }

        #endregion

        #region Properties

        public RtpListener RtpClient
        {
            get;
            private set;
        }

        public UdpListener Listener
        {
            get;
            private set;
        }

        public bool UseShortReportFormat
        {
            get;
            set;
        }

        public uint Ssrc
        {
            get;
            set;
        }

        public IPEndPoint RemoteEndPoint
        {
            get;
            set;
        }
        
        public uint ServerSsrc
        {
            get;
            set;
        }

        public DateTime LastSenderReport
        {
            get;
            set;
        }

        public ulong LastNtpTimestamp
        {
            get;
            set;
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            try
            {
                this.SendBye();
            }
            catch (InvalidOperationException)
            {
            }

            this.Listener.Dispose();
        }

        #endregion

    }
}
