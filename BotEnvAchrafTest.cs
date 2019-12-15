// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace BotEnvAchrafTest
{
    public class EmptyBot : IBot
    {
        public IStatePropertyAccessor<DialogState> ConversationDialogState { get; set; }
        public IStatePropertyAccessor<UserSelections> UserSelectionsState { get; set; }

        //private readonly ChatBoxAccessors _accessors;
        private readonly ConversationState _converationState;
        private readonly UserState _userState;
        private readonly ILogger _logger;
        private DialogSet _dialogs;


        //just a welcome Msg
        private const string WelcomeMessage = @"Welcome to the ChatBox.  This bot can help you find out about   live coding streams on Twitch!";




        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {

            if (turnContext.Activity.MembersAdded != null)
            {
                await SendWelcomeMessageAsync(turnContext, cancellationToken);
            }

            else
            {

                var ResponsefromUser = turnContext.Activity.Text.ToLower();
                //  await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");

                if (turnContext.Activity.Type == ActivityTypes.Message)

                {

                    switch (ResponsefromUser)
                    {

                        case "a":
                            await turnContext.SendActivityAsync($"Hi there! this Is A", cancellationToken: cancellationToken);

                            break;
                        case "b":
                            await turnContext.SendActivityAsync($"Hi there! this Is B", cancellationToken: cancellationToken);
                            break;
                        case "c":
                            await turnContext.SendActivityAsync($"Hi there! this Is C", cancellationToken: cancellationToken);
                            break;
                        default:
                            await turnContext.SendActivityAsync($"Hi there! this Is test ignore", cancellationToken: cancellationToken);
                            break;

                    }

                }


            }


        }

        private static async Task SendWelcomeMessagePrivateAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // Not the bot!
                    await turnContext.SendActivityAsync($"Hi there! " + turnContext.Activity.Text + "is uknowin form me ", cancellationToken: cancellationToken);
                    //  await DisplayMainMenuAsync(turnContext, cancellationToken);
                }
            }
        }





        private static async Task SendWelcomeMessageAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // Not the bot!
                    await turnContext.SendActivityAsync($"Hi there! {WelcomeMessage}",
                        cancellationToken: cancellationToken);
                    //  await DisplayMainMenuAsync(turnContext, cancellationToken);
                }
            }
        }











    }
}
