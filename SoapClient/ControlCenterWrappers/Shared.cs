using com.jaspersystems.api;
using SoapClient.com.jasperwireless.api7;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace SoapClient.ControlCenterWrappers
{
    internal static class Shared
    {
        internal static TerminalService GetTerminalService(string username, string apiKey)
        {
            SecurityHeader header = new SecurityHeader();
            header.UsernameToken.SetUserPass(username, apiKey, PasswordOption.SendPlainText);

            return new TerminalService() { securityHeader = header };
        }

        internal static void LogException(System.Web.Services.Protocols.SoapException e)
        {
            Console.WriteLine("=== Exception in SOAP request ===");
            Console.WriteLine("Error code: " + e.Message);
            Console.WriteLine("Details: " + e.Detail.InnerXml);
            Console.WriteLine();
            Console.WriteLine("Please see the 'Error Messages' Appendix in the Jasper API documentation.");
        }
    }
}
