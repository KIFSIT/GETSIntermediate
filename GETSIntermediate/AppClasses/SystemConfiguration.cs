using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GETSIntermediate
{
    public class SystemConfiguration
    {
        [XmlElement]
        public string ApplicationName {get; set;}

        [XmlElement]
        public string RMSSendIP {get; set;}

        [XmlElement]
        public int RMSSendPort {get; set;}

        [XmlElement]
        public string RMSListenIP {get; set;}

        [XmlElement]
        public int RMSListenPort {get; set;}

        [XmlElement]
        public string ClientListenIP {get; set;}

        [XmlElement]
        public int ClientListenPort {get; set;}

        [XmlElement]
        public bool RmsConnect {get; set;}
    }
}
