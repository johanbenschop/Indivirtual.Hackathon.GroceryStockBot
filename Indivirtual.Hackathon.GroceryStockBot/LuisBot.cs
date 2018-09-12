using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Ai.LUIS;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using Prompts = Microsoft.Bot.Builder.Prompts;
using Microsoft.Recognizers.Text;
using System.Linq;
using Indivirtual.Hackathon.GroceryStockBot.States;

namespace Indivirtual.Hackathon.GroceryStockBot
{
    public class LuisBot : IBot
    {
        private const double LUIS_INTENT_THRESHOLD = 0.2d;

        private readonly DialogSet dialogs;

        public LuisBot()
        {
            dialogs = new DialogSet();
            dialogs.Add("None", new WaterfallStep[] { DefaultDialog });
            dialogs.Add("Calendar_Add", new WaterfallStep[] { AskReminderTitle, SaveReminder });
            dialogs.Add("Calendar_Find", new WaterfallStep[] { ShowReminders, ConfirmShow });
            dialogs.Add("TitlePrompt", new TextPrompt(TitleValidator));
            dialogs.Add("ShowReminderPrompt", new ChoicePrompt(Culture.English));
        }

        public async Task OnTurn(ITurnContext turnContext)
        {
            if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate && turnContext.Activity.MembersAdded.FirstOrDefault()?.Id == turnContext.Activity.Recipient.Id)
            {
                await turnContext.SendActivity("Hi! I'm a simple reminder bot. I can add reminders and show them.");
            }
            else if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var userState = turnContext.GetUserState<UserState>();
                if (userState.Reminders == null)
                {
                    userState.Reminders = new List<Reminder>();
                }

                var state = turnContext.GetConversationState<Dictionary<string, object>>();
                var dialogContext = dialogs.CreateContext(turnContext, state);

                var utterance = turnContext.Activity.Text.ToLowerInvariant();
                if (utterance == "cancel")
                {
                    if (dialogContext.ActiveDialog != null)
                    {
                        await turnContext.SendActivity("Ok... Cancelled");
                        dialogContext.EndAll();
                    }
                    else
                    {
                        await turnContext.SendActivity("Nothing to cancel.");
                    }
                }

                if (!turnContext.Responded)
                {
                    await dialogContext.Continue();

                    if (!turnContext.Responded)
                    {
                        var luisResult = turnContext.Services.Get<RecognizerResult>(LuisRecognizerMiddleware.LuisRecognizerResultKey);
                        var (intent, score) = luisResult.GetTopScoringIntent();
                        var intentResult = score > LUIS_INTENT_THRESHOLD ? intent : "None";

                        await dialogContext.Begin(intent);
                    }
                }
            }
        }

        private Task DefaultDialog(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            return dialogContext.Context.SendActivity("Hi! I'm a simple reminder bot. I can add reminders and show them.");
        }

        private async Task TitleValidator(ITurnContext context, Prompts.TextResult result)
        {
            if (string.IsNullOrWhiteSpace(result.Value) || result.Value.Length < 3)
            {
                result.Status = Prompts.PromptStatus.NotRecognized;
                await context.SendActivity("Title should be at least 3 characters long.");
            }
        }
        
        private async Task AskReminderTitle(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            var reminder = new Reminder(dialogContext.ActiveDialog.State);
            dialogContext.ActiveDialog.State = reminder;
            if (!string.IsNullOrEmpty(reminder.Title))
            {
                await dialogContext.Continue();
            }
            else
            {
                await dialogContext.Prompt("TitlePrompt", "What would you like to call your reminder?");
            }
        }

        private async Task SaveReminder(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            var reminder = new Reminder(dialogContext.ActiveDialog.State);
            if (args is Prompts.TextResult textResult)
            {
                reminder.Title = textResult.Value;
            }
            await dialogContext.Context.SendActivity($"Your reminder named '{reminder.Title}' is set.");
            var userContext = dialogContext.Context.GetUserState<UserState>();
            userContext.Reminders.Add(reminder);
            await dialogContext.End();
        }

        private async Task ShowReminders(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            var userContext = dialogContext.Context.GetUserState<UserState>();
            if (userContext.Reminders.Count == 0)
            {
                await dialogContext.Context.SendActivity("No reminders found.");
                await dialogContext.End();
            }
            else
            {
                var choices = userContext.Reminders.Select(x => new Prompts.Choices.Choice() { Value = x.Title.Length < 15 ? x.Title : x.Title.Substring(0, 15) + "..." }).ToList();
                await dialogContext.Prompt("ShowReminderPrompt", "Select the reminder to show: ", new ChoicePromptOptions() { Choices = choices });
            }
        }

        private async Task ConfirmShow(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            var userContext = dialogContext.Context.GetUserState<UserState>();
            if (args is Prompts.ChoiceResult choice)
            {
                var reminder = userContext.Reminders[choice.Value.Index];
                await dialogContext.Context.SendActivity($"Reminder: {reminder.Title}");
            }
            await dialogContext.End();
        }
    }
}
