using Microsoft.Exchange.WebServices.Data;
using System;

class Program
{
    static void Main(string[] args)
    {
        // Set the security protocol to TLS 1.2
        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

        // Create an instance of ExchangeService
        ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2013_SP1)
        {
            Credentials = new WebCredentials("your_email@domain.com", "your_password"),
            Url = new Uri("https://outlook.office365.com/EWS/Exchange.asmx")
        };

        // Create an email message
        EmailMessage email = new EmailMessage(service)
        {
            Subject = "Test Email",
            Body = new MessageBody("This is a test email sent using EWS Managed API."),
            ToRecipients = { "recipient@domain.com" }
        };

        // Send the email and save a copy in the Sent Items folder
        email.SendAndSaveCopy();

        Console.WriteLine("Email sent successfully.");
    }
}
