using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BotEnvAchrafTest.Cards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;

namespace BotEnvAchrafTest.Dialogs
{
    public class SetTimezoneDialog : ComponentDialog
        {
            public IStatePropertyAccessor<UserSelections> UserSelectionsState;

            public SetTimezoneDialog(string dialogId, IStatePropertyAccessor<UserSelections> userSelectionsState) : base(dialogId)
            {
                UserSelectionsState = userSelectionsState;


                //set time zone for user
                var setTimezoneSteps = new WaterfallStep[]
                {
                GetUsersCountryStepAsync,
                GetUsersTimezoneStepAsync,
                ConfirmationStepAsync,
                };

                AddDialog(new WaterfallDialog("setTimezoneIntent", setTimezoneSteps));
                AddDialog(new TextPrompt("country"));
                AddDialog(new TextPrompt("timezone"));
            }


            //first Step
            private async Task<DialogTurnResult> GetUsersCountryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            {
                var cardAttachment = CountryCard.Create();
                var reply = stepContext.Context.Activity.CreateReply();
                reply.Attachments = new List<Attachment> { cardAttachment };
                await stepContext.Context.SendActivityAsync(reply, cancellationToken);

                return await stepContext.PromptAsync("country",
                    new PromptOptions
                    {
                        Prompt = new Activity
                        {
                            Text = string.Empty,
                            Type = ActivityTypes.Message,
                        }
                    },
                    cancellationToken);
            }

            //second Step
            private async Task<DialogTurnResult> GetUsersTimezoneStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            {
                var userSelections = await UserSelectionsState.GetAsync(stepContext.Context, () => new UserSelections(), cancellationToken);
                var countryJson = JObject.Parse((string)stepContext.Result);
                if (countryJson.ContainsKey("country"))
                {
                    userSelections.CountryCode = countryJson["country"].ToString();
                }

                await UserSelectionsState.SetAsync(stepContext.Context, userSelections, cancellationToken);

                var cardAttachment = TimezoneCard.Create(userSelections.CountryCode);
                var reply = stepContext.Context.Activity.CreateReply();
                reply.Attachments = new List<Attachment> { cardAttachment };
                await stepContext.Context.SendActivityAsync(reply, cancellationToken);

                return await stepContext.PromptAsync("timezone",
                    new PromptOptions
                    {
                        Prompt = new Activity
                        {
                            Text = string.Empty,
                            Type = ActivityTypes.Message,
                        }
                    },
                    cancellationToken);
            }

            //last step
            private async Task<DialogTurnResult> ConfirmationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
            {
                var userSelections = await UserSelectionsState.GetAsync(stepContext.Context, () => new UserSelections(), cancellationToken);
                var timezoneJson = JObject.Parse((string)stepContext.Result);

                if (timezoneJson.ContainsKey("tz"))
                {
                    userSelections.TimeZone = timezoneJson["tz"].ToString();
                }

                await UserSelectionsState.SetAsync(stepContext.Context, userSelections, cancellationToken);

                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You selected time zone {userSelections.TimeZone}"),
                    cancellationToken);

                return await stepContext.EndDialogAsync();
            }
        }




    }

