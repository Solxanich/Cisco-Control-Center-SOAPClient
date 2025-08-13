using SoapClient.com.jasperwireless.api7;
using SoapClient.JasperBillingService;
using System;
using System.Diagnostics;
using System.ServiceModel;
namespace SoapClient.ControlCenterWrappers
{
    internal static class InvoiceManager
    {
        internal static void GetInvoice(BillingService service, string licenseKey, long accountId, DateTime cycleStartDate)
        {
            GetInvoiceRequest request = new GetInvoiceRequest()
            {
                licenseKey = licenseKey,
                version = "1.0",
                messageId = $"Invoice_{accountId}_{cycleStartDate:yyyyMMdd}",
                accountId = accountId,
                cycleStartDate = cycleStartDate,
                cycleStartDateSpecified = true,
            };

            try
            {
                GetInvoiceResponse response = service.GetInvoice(request);
                LogInvoiceDetails(response);
            }
            catch (FaultException e)
            {
                Trace.WriteLine($"SOAP Fault: {e.Message}");
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Error: {e.Message}");
            }
        }

        private static void LogInvoiceDetails(GetInvoiceResponse response)
        {
            Trace.WriteLine("=== Invoice Details ===");
            Trace.WriteLine($"Account ID: {response.accountId}");
            Trace.WriteLine($"Invoice ID: {response.invoiceId}");
            Trace.WriteLine($"Currency: {response.currency}");
            Trace.WriteLine($"Invoice Date: {response.invoiceDate:yyyy-MM-dd}");
            Trace.WriteLine($"Due Date: {response.dueDate:yyyy-MM-dd}");
            Trace.WriteLine($"Billing Cycle: {response.cycleStartDate:yyyy-MM-dd} to {response.cycleEndDate:yyyy-MM-dd}");
            Trace.WriteLine($"Total Terminals: {response.totalTerminals}");
            Trace.WriteLine("");
            Trace.WriteLine("=== Charges ===");
            Trace.WriteLine($"Subscription Charge: {response.subscriptionCharge} {response.currency}");
            Trace.WriteLine($"Data Volume: {response.dataVolume} MB");
            Trace.WriteLine($"Data Overage Charge: {response.overageCharge} {response.currency}");
            Trace.WriteLine($"SMS Volume: {response.smsVolume}");
            Trace.WriteLine($"SMS Charge: {response.smsCharge} {response.currency}");
            Trace.WriteLine($"Voice Volume: {response.voiceVolume} sec");
            Trace.WriteLine($"Voice Charge: {response.voiceCharge} {response.currency}");
            Trace.WriteLine($"Other Charges: {response.otherCharge} {response.currency}");
            Trace.WriteLine($"Activation Charges: {response.activationCharge} {response.currency}");
            Trace.WriteLine($"Discount Applied: {response.discountApplied} {response.currency}");
            Trace.WriteLine("");
            Trace.WriteLine($"Total Charge: {response.totalCharge} {response.currency}");
            Trace.WriteLine("");
        }
    }
}