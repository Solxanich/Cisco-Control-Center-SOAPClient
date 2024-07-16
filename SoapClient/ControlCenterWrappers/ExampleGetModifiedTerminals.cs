using SoapClient.com.jasperwireless.api7;
using System;

// This is a heavily modified example class from Cisco. 

namespace SoapClient.ControlCenterWrappers
{
    class GetModifiedTerminals
    {
        internal static void GetModifiedTerminal(TerminalService service, string licenseKey)
        {
            GetModifiedTerminalsRequest request = new GetModifiedTerminalsRequest()
            {
                licenseKey = licenseKey,
                version = "1.0",
                messageId = "message-1",
                since = new DateTime(2010, 9, 10),
                sinceSpecified = true
            };

            try
            {
                GetModifiedTerminalsResponse response = service.GetModifiedTerminals(request);
                Console.WriteLine("ICCIDs size: " + response.iccids.Length);
                Console.WriteLine("ICCIDs[0]: " + response.iccids[0]);
            }
            catch (System.Web.Services.Protocols.SoapException e)
            {
                Shared.LogException(e);
            }
        }
    }
}