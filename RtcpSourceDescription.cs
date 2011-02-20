using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace RtpLib
{
    public class RtcpSourceDescription : RtcpPacket
    {

        #region Public Methods

        public override void ParseData(Stream stream)
        {
            byte[] bytes = new byte[4];
            if (stream.Read(bytes, 0, bytes.Length) < bytes.Length)
                throw new InvalidDataException();

            this.Ssrc = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(bytes, 0));

            // Build a list of setters to make things easy for us
            List<Action<string>> list = new List<Action<string>>
            {
                value => this.CannonicalName = value,
                value => this.Name = value,
                value => this.Email = value,
                value => this.Phone = value,
                value => this.Location = value,
                value => this.Tool = value,
                value => this.Note = value,
                value => this.Private = value

            };

            // Reset everything before reading in the data
            foreach (var action in list)
                action(string.Empty);
            
            while (true)
            {
                // Get the value representing which string we'll be setting
                int next = stream.ReadByte();
                if (next == -1)
                    break;

                // Get the length of the string
                int length = stream.ReadByte();
                if (length == -1)
                    break;

                // Read the string bytes
                byte[] stringBytes = new byte[length];
                int read = stream.Read(stringBytes, 0, stringBytes.Length);

                // Only set the value if we have a setter and the string length matches what we read
                if (next < list.Count && read == stringBytes.Length)
                    list[next - 1](Encoding.ASCII.GetString(stringBytes));
            }
        }

        protected override int GetByteCount()
        {
            // TODO: Should we just only strings with data?
            return 4 +
                2 + (this.CannonicalName ?? string.Empty).Length +
                2 + (this.Name ?? string.Empty).Length +
                2 + (this.Email ?? string.Empty).Length +
                2 + (this.Phone ?? string.Empty).Length +
                2 + (this.Location ?? string.Empty).Length +
                2 + (this.Tool ?? string.Empty).Length +
                2 + (this.Note ?? string.Empty).Length +
                2 + (this.Private ?? string.Empty).Length;
        }

        protected override void ToStreamInternal(Stream stream)
        {
            stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)this.Ssrc)), 0, 4);
            Dictionary<byte, string> strings = new Dictionary<byte, string>
            {
                {1, this.CannonicalName},
                {2, this.Name},
                {3, this.Email},
                {4, this.Phone},
                {5, this.Location},
                {6, this.Tool},
                {7, this.Note},
                {8, this.Private}
            };

            // TODO: Should we just only strings with data?
            foreach (var pair in strings)
            {
                stream.WriteByte(pair.Key);
                stream.WriteByte((byte)(pair.Value ?? string.Empty).Length);

                if (!string.IsNullOrEmpty(pair.Value))
                    stream.Write(Encoding.ASCII.GetBytes(pair.Value), 0, pair.Value.Length);
            }
        }

        #endregion

        #region Properties

        public override Rtcp.PacketType PacketType
        {
            get { return Rtcp.PacketType.SourceDescription; }
        }

        public uint Ssrc
        {
            get;
            set;
        }

        public string CannonicalName
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public string Email
        {
            get;
            set;
        }

        public string Phone
        {
            get;
            set;
        }

        public string Location
        {
            get;
            set;
        }
        
        public string Tool
        {
            get;
            set;
        }
        
        public string Note
        {
            get;
            set;
        }
        
        public string Private
        {
            get;
            set;
        }

        #endregion

        #region ICloneable Implementation

        public override object Clone()
        {
            var packet = this.MemberwiseClone() as RtcpSourceDescription;
            packet.Header = packet.Header.Clone() as RtcpHeader;
            return packet;
        }

        #endregion

    }
}
