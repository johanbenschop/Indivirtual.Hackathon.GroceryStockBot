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

namespace Indivirtual.Hackathon.GroceryStockBot // Indivirtual_Hackathon_GroceryStockBot
{
    public class LuisBot : IBot
    {
        private const double LUIS_INTENT_THRESHOLD = 0.2d;

        private readonly DialogSet dialogs;

        public LuisBot()
        {
            dialogs = new DialogSet();
            dialogs.Add("None", new WaterfallStep[] { DefaultDialog });
            dialogs.Add("AddToCart", new WaterfallStep[] { AddToCart });
            dialogs.Add("RemoveFromCart", new WaterfallStep[] { RemoveFromCart });
            //dialogs.Add("None", new WaterfallStep[] { DefaultDialog });
            //dialogs.Add("Calendar_Add", new WaterfallStep[] { AskReminderTitle, SaveReminder });
            //dialogs.Add("Calendar_Find", new WaterfallStep[] { ShowReminders, ConfirmShow });
            //dialogs.Add("TitlePrompt", new TextPrompt(TitleValidator));
            //dialogs.Add("ShowReminderPrompt", new ChoicePrompt(Culture.English));
        }

        public async Task OnTurn(ITurnContext turnContext)
        {
            if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate && turnContext.Activity.MembersAdded.FirstOrDefault()?.Id == turnContext.Activity.Recipient.Id)
            {
                await turnContext.SendActivity("Hi!");
            }
            else if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var userState = turnContext.GetUserState<UserState>();
                if (userState.Reminders == null)
                {
                    userState.Reminders = new List<ReminderState>();
                }

                var state = turnContext.GetConversationState<Dictionary<string, object>>();
                var dialogContext = dialogs.CreateContext(turnContext, state);

                var utterance = turnContext.Activity.Text.ToLowerInvariant();
                if (utterance == "cancel")
                {
                    if (dialogContext.ActiveDialog != null)
                    {
                        dialogContext.EndAll();
                        await turnContext.SendActivity("Ok... Cancelled");
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
            return dialogContext.Context.SendActivity("Hey! Ik ben Indivirtual's boodschappen manager. Je kan mij vragen om iets op het boodschappen lijstje te zetten of melden wanneer iets op is.");
        }
        
        private async Task AddToCart(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            await dialogContext.Context.SendActivity($"Ojee, ik ben bang dat ik het boodschappen lijstje kwijt ben...");
        }

        private async Task RemoveFromCart(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            await dialogContext.Context.SendActivity($"Ojee, ik ben bang dat ik het boodschappen lijstje kwijt ben...");
        }

        private async Task ShowOverviewBonus(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            await dialogContext.Context.SendActivity($"Ojee, ik ben bang dat ik het boodschappen lijstje kwijt ben...");
        }

        private async Task WhatIsCheapest(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            await dialogContext.Context.SendActivity($"Ojee, ik ben bang dat ik het boodschappen lijstje kwijt ben...");
        }

        private async Task OrderDate(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            await dialogContext.Context.SendActivity($"Ojee, ik ben bang dat ik het boodschappen lijstje kwijt ben...");
        }

        private async Task CheckAmountofProduct(DialogContext dialogContext, object args, SkipStepFunction next)
        {
            await dialogContext.Context.SendActivity($"Ojee, ik ben bang dat ik het boodschappen lijstje kwijt ben...");
        }
    }
}
