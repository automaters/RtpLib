using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace RtpLib
{
    public class RtcpAppDefined : RtcpUnknown
    {

        public override Rtcp.PacketType PacketType
        {
            get { return Rtcp.PacketType.AppDefined; }
        }

    }
}
