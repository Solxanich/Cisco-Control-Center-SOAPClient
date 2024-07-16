using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CsvHelper.Configuration.Attributes;

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
        [Name("ip address")]
        public string ip { get; set; }
        [Name("custom_CellularPdpID")]
        public string pdpid { get; set; }
        [Name("custom_CellularAPN")]
        public string apn { get; set; }
        [Name("custom_SIMNumber")]
        public string iccid { get; set; }
        [Name("hostname")]
        public string deviceid { get; set; }
        [Name("custom_AdminGroup")]
        public string customer { get; set; }
    }
}
