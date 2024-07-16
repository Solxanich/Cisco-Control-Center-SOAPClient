using System;
using System.Xml;
using System.Xml.Serialization;
using System.Web.Services.Protocols;
using System.Text;

/*
//outgoing SoapHeader will look something like this
<wsse:Security soapenv:mustUnderstand="1" xmlns:wsse="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd">
 <wsse:UsernameToken wsu:Id="UsernameToken-8164742" xmlns:wsu="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd">
    <wsse:Username>username</wsse:Username>
    <wsse:Password Type="http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText">password</wsse:Password>
 </wsse:UsernameToken>
</wsse:Security>

//add public member to the auto generated Web Reference proxy, say TerminalService
public com.jaspersystems.api.SecurityHeader securityHeader;

//add to WebMethod that will be called on auto generated Web Reference proxy, say TerminalService.GetModifiedTerminals(request)
[System.Web.Services.Protocols.SoapHeaderAttribute( "securityHeader")]

//change client code to set UsernameToken info
com.jaspersystems.api.securityHeader = new com.jaspersystems.api.SecurityHeader();
com.jaspersystems.api.securityHeader.UsernameToken.SetUserPass(username, password, PasswordOption.SendPlainText);

*/

namespace com.jaspersystems.api
{
    [XmlRoot(Namespace = NsConstants.wsse, ElementName = "Security")]
    public class SecurityHeader : SoapHeader
    {
        public SecurityHeader()
        {
            this.MustUnderstand = true;
            this.UsernameToken = new UsernameToken();
        }

        [XmlElement(Namespace = NsConstants.wsse)]
        public UsernameToken UsernameToken;
    }

    public class UsernameToken
    {
        public UsernameToken() { }

        public void SetUserPass(string username, string password, PasswordOption passType)
        {
            this.Username = username;

            System.Guid g = Guid.NewGuid();
            this.Id = "SecurityToken-" + g.ToString("D");

            if (passType == PasswordOption.SendNone)
            {
                this.Password = null;
            }

            this.Password = new Password();

            if (passType == PasswordOption.SendPlainText)
            {
                this.Password.Type = NsConstants.passwdType;
                this.Password.Text = password;
            }

        }

        //required
        [XmlElement(Namespace = NsConstants.wsse)]
        public string Username;
        [XmlAttribute(Namespace = NsConstants.wsu)]
        public string Id;

        //optional
        [XmlElement(Namespace = NsConstants.wsse)]
        public Password Password;
    }


    public class Password
    {
        public Password() { }

        [XmlText()]
        public string Text;
        [XmlAttribute()]
        public string Type;
    }

    public class NsConstants
    {
        public const string wsse = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
        public const string wsu = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
        public const string passwdType = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText";
    }

    public enum PasswordOption
    {
        SendNone,
        SendPlainText
    }
}