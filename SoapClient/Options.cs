using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace SoapClient
{
    internal class Options
    {
        // Common Parameters
        [Option]
        public string uname {  get; set; }

        [Option]
        public string apikey { get; set; }

        [Option]
        public string license { get; set; }

        [Option]
        public string task { get; set; }

        // Parameters for Changing IPs
        [Option]
        public string ipfile { get; set; }
    }

    internal class ProvisioningArgs
    {
        public string ip { get; set; }
        public string pdpid { get; set; }
        public string apn { get; set; }
        public string iccid { get; set; }
        public string deviceid { get; set; }
        public string customer { get; set; }
    }
}
