// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BotEnvAchrafTest.Dialogs;
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



        public EmptyBot(ConversationState conversationState, UserState userState,
            ILoggerFactory loggerFactory)
        {

            if (conversationState == null)
            {
                throw new System.ArgumentNullException(nameof(conversationState));
            }

            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _userState = userState;
            _converationState = conversationState;
            ConversationDialogState = _converationState.CreateProperty<DialogState>($"{nameof(EmptyBot)}.ConversationDialogState");
            UserSelectionsState = _converationState.CreateProperty<UserSelections>($"{nameof(EmptyBot)}.UserSelectionsState");

            _logger = loggerFactory.CreateLogger<EmptyBot>();
            _logger.LogTrace("Turn start.");

            //new dialog
            _dialogs = new DialogSet(ConversationDialogState);

            var dummySteps = new WaterfallStep[]
            {
                DummyStepAsync,
            };


            _dialogs.Add(new WhenNextDialog("whenNextIntent", UserSelectionsState));
            _dialogs.Add(new SetTimezoneDialog("setTimezoneIntent", UserSelectionsState));
            _dialogs.Add(new WaterfallDialog("dummy", dummySteps));

            //_dialogs.Add(new TextPrompt("User-name"));

        }

      


        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {

                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                var channelData = JObject.Parse(turnContext.Activity.ChannelData.ToString());

                if (channelData.ContainsKey("postBack"))
                {
                    // This is from an adaptive card postback
                    var activity = turnContext.Activity;
                    activity.Text = activity.Value.ToString();
                }

                var userChoice = turnContext.Activity.Text;
                var responseMessage = $"You chose: '{turnContext.Activity.Text}'\n";

                switch (results.Status)
                {
                    case DialogTurnStatus.Empty:

                        if (!string.IsNullOrWhiteSpace(userChoice))
                        {
                            switch (userChoice)
                            {
                                case "1":
                                    await dialogContext.BeginDialogAsync("dummy", null, cancellationToken);
                                    break;
                                case "2":
                                    await dialogContext.BeginDialogAsync("whenNextIntent", null, cancellationToken);
                                    break;
                                case "3":
                                    await dialogContext.BeginDialogAsync("dummy", null, cancellationToken);
                                    break;
                                case "4":
                                    await dialogContext.BeginDialogAsync("setTimezoneIntent", null, cancellationToken);
                                    break;
                                default:
                                    await turnContext.SendActivityAsync("Please select a menu option");
                                    await DisplayMainMenuAsync(turnContext, cancellationToken);
                                    break;
                            }
                        }

                        break;

                    case DialogTurnStatus.Cancelled:
                        await DisplayMainMenuAsync(turnContext, cancellationToken);
                        break;
                    case DialogTurnStatus.Waiting:
                        await dialogContext.ContinueDialogAsync(cancellationToken);
                        break;
                    case DialogTurnStatus.Complete:
                        await _userState.SaveChangesAsync(turnContext, false, cancellationToken);

                        await DisplayMainMenuAsync(turnContext, cancellationToken);
                        break;
                }

                // Save the new turn count into the conversation state.
                await _converationState.SaveChangesAsync(turnContext);
                await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
                // var userSelections = await UserSelectionsState.GetAsync(turnContext, () => new UserSelections(), cancellationToken);
            }
            else if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded != null)
                {
                    await SendWelcomeMessageAsync(turnContext, cancellationToken);
                }
            }
            else
            {
                await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
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
                    await DisplayMainMenuAsync(turnContext, cancellationToken);
                }
            }
        }







        //just a step to show user selection
        private async Task<DialogTurnResult> DummyStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var selection = stepContext.Context.Activity.Text;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You selected {selection}"), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }



        ////simple prompt Menu 
        //private static async Task DisplayMainMenuAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        //{
        //    var reply = turnContext.Activity.CreateReply("What would you like to do Today ?");
        //    reply.SuggestedActions = new SuggestedActions
        //    {
        //        Actions = new List<CardAction>
        //        {
        //            new CardAction { Title = "1. ", Type = ActionTypes.ImBack, Value = "1" },
        //            new CardAction { Title = "2. ", Type = ActionTypes.ImBack, Value = "2" },
        //            new CardAction { Title = "3. ", Type = ActionTypes.ImBack, Value = "3" },
        //            new CardAction { Title = "4. Help", Type = ActionTypes.ImBack, Value = "4" },
        //        }
        //    };

        //    await turnContext.SendActivityAsync(reply, cancellationToken);
        //}

        //simple prompt Menu 
        private static async Task DisplayMainMenuAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {


            var heroCard = new HeroCard
            {
                Title = "BotFramework Test",
                //Subtitle = "Microsoft Bot Framework",
                Text = "Build smart Bot" ,
                Images = new List<CardImage> { new CardImage("https://insomea.tn/wp-content/uploads/2018/02/LogoINSOMEA.png") },
                Buttons = new List<CardAction>
                {
                    new CardAction { Title = "Home. ", Type = ActionTypes.ImBack, Value = "1" },
                    new CardAction { Title = "Solutions. ", Type = ActionTypes.ImBack, Value = "2" },
                    new CardAction { Title = "Services ", Type = ActionTypes.ImBack, Value = "3" },
                    new CardAction { Title = "4. Help/contactUs", Type = ActionTypes.ImBack, Value = "4" },
                }
            };
            
            var reply = turnContext.Activity.CreateReply("What would you like to do Today ?");
            reply.Attachments=new List<Attachment>(){heroCard.ToAttachment()};
            
            //reply.SuggestedActions = new SuggestedActions
            //{
            //    Actions = new List<CardAction>
            //    {
            //        new CardAction { Title = "1. ", Type = ActionTypes.ImBack, Value = "1" },
            //        new CardAction { Title = "2. ", Type = ActionTypes.ImBack, Value = "2" },
            //        new CardAction { Title = "3. ", Type = ActionTypes.ImBack, Value = "3" },
            //        new CardAction { Title = "4. Help", Type = ActionTypes.ImBack, Value = "4" },
            //    }
            //};

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }



        public static HeroCard GetHeroCard()
        {
            var heroCard = new HeroCard
            {
                Title = "BotFramework Hero Card",
                Subtitle = "Microsoft Bot Framework",
                Text = "Build and connect intelligent bots to interact with your users naturally wherever they are," +
                       " from text/sms to Skype, Slack, Office 365 mail and other popular services.",
                Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://docs.microsoft.com/bot-framework") },
            };

            return heroCard;
        }



    }
}
