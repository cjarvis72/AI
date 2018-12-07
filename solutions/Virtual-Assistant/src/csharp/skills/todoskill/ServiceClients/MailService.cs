﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace ToDoSkill
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Graph;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Mail service used to call real apis.
    /// </summary>
    public class MailService : IMailService
    {
        private readonly string graphBaseUrl = "https://graph.microsoft.com/v1.0/me";
        private HttpClient httpClient;
        private IGraphServiceClient graphServiceClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="MailService"/> class.
        /// Init service use token.
        /// </summary>
        /// <param name="token">access token.</param>
        /// <returns>Mail service itself.</returns>
        public async Task<IMailService> InitAsync(string token)
        {
            httpClient = ServiceHelper.GetHttpClient(token);
            graphServiceClient = ServiceHelper.GetAuthenticatedClient(token);
            return await Task.Run(() => this);
        }

        /// <summary>
        /// Send an email message.
        /// </summary>
        /// <param name="content">Email Body.</param>
        /// <param name="subject">Eamil Subject.</param>
        /// <returns>Completed Task.</returns>
        public async Task SendMessageAsync(string content, string subject)
        {
            var userObject = await ExecuteGraphFetchAsync(graphBaseUrl);
            List<Recipient> re = new List<Recipient>();
            re.Add(new Recipient
            {
                EmailAddress = new EmailAddress
                {
                    Address = userObject["userPrincipalName"],
                },
            });

            // Create the message.
            Message email = new Message
            {
                Body = new ItemBody
                {
                    Content = content,
                    ContentType = BodyType.Html,
                },
                Subject = subject,
                ToRecipients = re,
            };

            // Send the message.
            await this.graphServiceClient.Me.SendMail(email, true).Request().PostAsync();
        }

        private async Task<dynamic> ExecuteGraphFetchAsync(string url)
        {
            var result = await this.httpClient.GetStringAsync(url);
            dynamic content = JObject.Parse(result);
            return content;
        }
    }
}
