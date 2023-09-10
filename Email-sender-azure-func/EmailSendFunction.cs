using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Email_sender_azure_func
{
    [StorageAccount("BlobConnectionString")]
    public class EmailSendFunction
    {
        [FunctionName("EmailSendFunction")]
        public void Run([BlobTrigger("docx/{name}")] Stream myBlob,
            string name,
            IDictionary<string, string> metadata,
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            if (metadata != null)
            {
                string email = metadata.ContainsKey("email") ? metadata["email"] : null;
                string sasUri = metadata.ContainsKey("sasUri") ? metadata["sasUri"] : null;



                using var emailMessage = new MimeMessage();

                emailMessage.From.Add(new MailboxAddress("Thanks for using docxToBlob", "eugenecorporative@outlook.com"));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = "File upload";
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = $"Thanks for using our system youre file have been uploaded to storage. Your file {sasUri}"
                };

                string username = Environment.GetEnvironmentVariable("Email");
                string password = Environment.GetEnvironmentVariable("Password");

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.outlook.com", 587, false);
                    client.Authenticate(username, password);
                    client.Send(emailMessage);

                    client.Disconnect(true);
                }
            }

        }
    }
}
