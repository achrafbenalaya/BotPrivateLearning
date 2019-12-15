using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace BotEnvAchrafTest.Dialogs
{
    public class WhenNextDialog :ComponentDialog
    {
        private IStatePropertyAccessor<UserSelections> _UserSelectionsState;

        //
        public WhenNextDialog(string dialogid, IStatePropertyAccessor<UserSelections> UserSelectionsState):base(dialogid)
        {

            _UserSelectionsState = UserSelectionsState;

            var whenNextSteps = new WaterfallStep[]
            {
                GetNameStepAsync,
                GetUserInfoStepAsync
            };

            AddDialog(new WaterfallDialog("whenNextIntent", whenNextSteps));
            AddDialog(new TextPrompt("User-name"));
        }


        //get name
        private async Task<DialogTurnResult> GetNameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userSelections = await _UserSelectionsState.GetAsync(stepContext.Context, () => new UserSelections(), cancellationToken);
            await _UserSelectionsState.SetAsync(stepContext.Context, userSelections, cancellationToken);

            return await stepContext.PromptAsync("User-name", new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter your Name")
                },
                cancellationToken);
        }





        //GetUserSelection
        private async Task<DialogTurnResult> GetUserInfoStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userSelections = await _UserSelectionsState.GetAsync(stepContext.Context, () => new UserSelections(), cancellationToken);
            userSelections.UserName = (string)stepContext.Result;

            // ToDo: get the data from GraphQL endpoint

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"You selected {userSelections.UserName}"),
                cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }


    }
}
