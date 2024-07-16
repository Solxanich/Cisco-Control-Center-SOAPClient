using SoapClient.com.jasperwireless.api7;
using System;
using System.Diagnostics;

namespace SoapClient.ControlCenterWrappers
{
    internal static class PeakJasperRapper
    {
        internal static void SetNewIP(TerminalService service, string licenseKey, string iccId, string ip, string pdpId, string apn)
        {
            AssignOrUpdateIPAddressRequest request = new AssignOrUpdateIPAddressRequest()
            {
                licenseKey = licenseKey,
                version = "1.0",
                messageId = ip,
                iccid = iccId,
                pdpId = pdpId,
                apn = apn,
                ipAddress = ip
            };

            try
            {
                AssignOrUpdateIPAddressResponse response = service.AssignOrUpdateIPAddress(request);
                Trace.WriteLine($"New IP for {iccId} is: {response.correlationId}");
            }
            catch (System.Web.Services.Protocols.SoapException e)
            {
                Shared.LogException(e);
            }
        }

        internal static void EditTerminal(TerminalService service, string licenseKey, string iccid, TerminalChangeType attribute, string value)
        {
            EditTerminalRequest request = new EditTerminalRequest()
            {
                licenseKey = licenseKey,
                version = "1.0",
                messageId = iccid + attribute,
                iccid = iccid,
                targetValue = value,
                changeType = (int)attribute
            };

            try
            {
                EditTerminalResponse response = service.EditTerminal(request);
                Trace.WriteLine($"Modified {attribute} for {iccid} is: {value}");
            }
            catch (System.Web.Services.Protocols.SoapException e)
            {
                Shared.LogException(e);
            }
        }
    }
}
