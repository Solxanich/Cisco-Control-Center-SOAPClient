﻿using CsvHelper;
using SoapClient.com.jasperwireless.api7;
using SoapClient.ControlCenterWrappers;
using System;
using System.Globalization;
using System.IO;


namespace SoapClient
{
    internal static class TaskRunner
    {
        internal static void ProvisionSim(string filePath, TerminalService service, string licenseKey)
        {
            using (var reader = new StreamReader(filePath))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<ProvisioningArgs>();
                    foreach (var item in records)
                    {
                        System.DateTime currTime = System.DateTime.Now;

                        var sim = Shared.RemoveNonNumbersFromString(item.iccid);

                        if (!string.IsNullOrEmpty(item.ip))
                            PeakJasperRapper.SetNewIP(service, licenseKey, iccId: sim, ip: item.ip, pdpId: item.pdpid, apn: item.apn);

                        PeakJasperRapper.EditTerminal(service, licenseKey, iccid: sim, TerminalChangeType.customer, item.customer);
                        PeakJasperRapper.EditTerminal(service, licenseKey, iccid: sim, TerminalChangeType.deviceid, item.deviceid); // For Billing
                        PeakJasperRapper.EditTerminal(service, licenseKey, iccid: sim, TerminalChangeType.locationid, item.deviceid); // For Customer Accounts Reference
                        PeakJasperRapper.EditTerminal(service, licenseKey, iccid: sim, TerminalChangeType.projectstatus, $"Project-{DateTime.Now.Year}"); // For Billing O&M vs Projects
                        PeakJasperRapper.EditTerminal(service, licenseKey, iccid: sim, TerminalChangeType.onboarded, "Onboarded"); // For whether this SIM has been provisioned

                        // Delay between calls per Cisco SPEC
                        while (System.DateTime.Now < currTime.AddSeconds(30)) { }
                    }
                }
            }
        }
    }
}
