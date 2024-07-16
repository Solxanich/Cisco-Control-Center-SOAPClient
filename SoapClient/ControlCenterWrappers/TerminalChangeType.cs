namespace SoapClient.ControlCenterWrappers
{

    // See Terminal Change type under 'Simple Types' documentation
    internal enum TerminalChangeType
    {
        locationid = 17, // The 'Location ID' Account Custom 1, modifiable by customer
        deviceid = 1,
        customer = 6,
        rateplan = 4,
        status = 3,
        projectstatus = 19, // the 'Project vs O&M' tracking field Account custom 3, modifiable by customer
    }
}
