using SoapClient.com.jasperwireless.api7;
using SoapClient.JasperBillingService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;

namespace SoapClient.ControlCenterWrappers
{
    internal static class DeviceInfoWrapper
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

        public class TerminalDetails
        {
            public string Customer { get; set; }
            public string GisLocation { get; set; }
            public string Comments { get; set; }
            public string ProjectStatus { get; set; }
            public string Onboarded { get; set; }

            internal static Dictionary<string, TerminalDetails> deviceDetailsCache = new Dictionary<string, TerminalDetails>();
        }

        internal static void UpdateDetailsForIccIdList(TerminalService service, string licenseKey, string[] iccids)
        {
            if (iccids.Length > 50)
                throw new Exception("More Terminals were requested than the API allows. Length of ICCIDs array must be less than or equal to 50");

            GetTerminalDetailsRequest request = new GetTerminalDetailsRequest() //Probably need a different function since this one doesn't grab the Customer field
            {
                licenseKey = licenseKey,
                version = "1.0",
                messageId = $"Details_{iccids[0]}",
                iccids = iccids
            };

            var response = service.GetTerminalDetails(request);

            foreach (var terminal in response.terminals)
            {
                var details = new TerminalDetails
                {
                    Customer = terminal.customer,
                    GisLocation = terminal.custom1,
                    Comments = terminal.custom2,
                    ProjectStatus = terminal.custom3,
                    Onboarded = terminal.custom4
                };

                TerminalDetails.deviceDetailsCache[terminal.iccid] = details;
            }
        }

        internal static List<string> GetActiveIccIdList(TerminalService service, string licenseKey)
        {
            GetModifiedTerminalsRequest request = new GetModifiedTerminalsRequest()
            {
                licenseKey = licenseKey,
                version = "1.0",
                messageId = "message-Sept2010",
                since = new DateTime(2010, 9, 10),
                sinceSpecified = true
            };

            try
            {
                GetModifiedTerminalsResponse response = service.GetModifiedTerminals(request);

                Trace.WriteLine("ICCIDs size: " + response.iccids.Length);
                return response.iccids?.ToList() ?? new List<string>();
            }
            catch (System.Web.Services.Protocols.SoapException e)
            {
                Shared.LogException(e);
                throw e;
            }
        }
    }
}
