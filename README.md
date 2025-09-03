Jasper / IoT Control Center Open-Source .NET Framework SOAP Client.
For use with fixed IPv4-only APNs.
Copyright (C) 2024 Solxan

This program is free software:
you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program.
If not, see [http://www.gnu.org/licenses/](http://www.gnu.org/licenses/).



How To Use ----------------------



Bulk Provisioning Including Setting IPs::

```

soapclient.exe --task="provision" --uname="{your username}" --apikey="{your api key}" --license="{your license key}" --ipfile="{Provisioning Template File}"

```



Retrieve an Invoice::

```

soapclient.exe --task="invoice" --uname="{your username}" --apikey="{your api key}" --license="{your license key}" --accountid="{your account id}" --cyclestart="YYYY-Mmm-DD"

```



Retrieve Billing Information By Device

```

soapclient.exe --task="invoice" --uname="{your username}" --apikey="{your api key}" --license="{your license key}" --accountid="{your account id}" --cyclestart="YYYY-Mmm-DD"

```





