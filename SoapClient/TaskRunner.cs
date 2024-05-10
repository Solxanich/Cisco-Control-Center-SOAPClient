using CsvHelper;
using SoapClient.com.jasperwireless.api7;
using SoapClient.ControlCenterWrappers;
using System.Globalization;
using System.IO;


namespace SoapClient
{
    internal static class TaskRunner
    {
        internal static void SetIp(string filePath, TerminalService service, string licenseKey)
        {
            using (var reader = new StreamReader(filePath))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<SetIpArgs>();
                    foreach (var item in records)
                    {
                        PeakJasperRapper.SetNewIP(service, licenseKey, iccId: item.iccid, ip: item.ip, pdpId: item.pdpid, apn: item.apn);
                        
                        // Delay between calls per Cisco SPEC
                        System.Threading.Thread.Sleep(30000);
                    }
                }
            }
        }
    }
}
