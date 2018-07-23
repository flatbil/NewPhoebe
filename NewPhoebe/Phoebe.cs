using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts;
using System.Collections.Generic;
using System;
using Microsoft.Recognizers.Text;
using System.Linq;

namespace NewPhoebe
{
    public class Phoebe : IBot
    {
        private DialogSet dialogsT;
        private DialogSet dialogSas;

        static DateTime reservationDate;
        static int partySize;
        static string reservationName;
        static string nameResult;
        static string partyResult;
        static string reasonGiven;
        bool messageSent;

        public Phoebe()
        {
            dialogsT = new DialogSet();
            dialogSas = new DialogSet();

            
            reserveTable(dialogsT);
            sassy(dialogSas);


        }
        public void sassy(DialogSet dialogs)
        {
            // Define our dialog
            dialogs.Add("sassyText", new WaterfallStep[]
            {
                async (dcS, args, next) =>
                {
                    // Prompt for the guest's name.
                    await dcS.Context.SendActivity("Hello you ;-)");
                    try
                    {
                        await dcS.Prompt("textPrompt", "What's your name?");
                    }
                    catch
                    {
                        await dcS.Context.SendActivity("Sorry, didn't get that textPrompt.");
                    }
                    
                },
                async(dcS, args, next) =>
                {
                    await dcS.Context.SendActivity("I'm in the next activity now. about to record the nameResult." +
                        "");
                    nameResult = (string)args["Text"];

                    await dcS.Context.SendActivity($"{nameResult}... I like that name!");

            
                    // Ask for next info
                    await dcS.Prompt("partyPrompt", $"Do you like to party {nameResult}?");

                },
                async(dcS, args, next) =>
                {
                    partyResult = (string)args["Text"];

                    if(partyResult.ToLowerInvariant().Contains("no")) {
                        await dcS.Prompt("rejectionPrompt", "Awe, Why not?");
                    } else
                    {
                        await dcS.Context.SendActivity("See ya soon!");
                        await dcS.End();
                    }
                },
                async(dcS, args, next) =>
                {
                    reasonGiven = (string)args["Text"];
                    string msg = $"{reasonGiven}?..... Well... Hope you have a nice day {nameResult}";

                    //var convo = ConversationState<PhoebeState>.Get(dc.Context);

                    //// In production, you may want to store something more helpful
                    //convo[$"{dc.ActiveDialog.State["name"]} reservation"] = msg;

                    await dcS.Context.SendActivity(msg);
                    await dcS.End();
                }
            });

            // Add a prompt for the reservation date
            dialogs.Add("textPrompt", new Microsoft.Bot.Builder.Dialogs.TextPrompt());
            // Add a prompt for the party size
            dialogs.Add("partyPrompt", new Microsoft.Bot.Builder.Dialogs.TextPrompt());
            // Add a prompt for the user's name
            dialogs.Add("rejectionPrompt", new Microsoft.Bot.Builder.Dialogs.TextPrompt());
        }
        public void reserveTable(DialogSet dialogs)
        {
            // Define our dialog
            dialogs.Add("reserveTable", new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                    // Prompt for the guest's name.
                    await dc.Context.SendActivity("Welcome to the reservation service.");

                    await dc.Prompt("dateTimePrompt", "Please provide a reservation date and time.");
                },
                async(dc, args, next) =>
                {
                    var dateTimeResult = ((DateTimeResult)args).Resolution.First();

                    reservationDate = Convert.ToDateTime(dateTimeResult.Value);
            
                    // Ask for next info
                    await dc.Prompt("partySizePrompt", "How many people are in your party?");

                },
                async(dc, args, next) =>
                {
                    partySize = (int)args["Value"];

                    // Ask for next info
                    await dc.Prompt("textPrompt", "Whose name will this be under?");
                },
                async(dc, args, next) =>
                {
                    reservationName = (string)args["Text"];
                    string msg = "Reservation confirmed. Reservation details - " +
                    $"\nDate/Time: {reservationDate.ToString()} " +
                    $"\nParty size: {partySize.ToString()} " +
                    $"\nReservation name: {reservationName}";

                    //var convo = ConversationState<PhoebeState>.Get(dc.Context);

                    //// In production, you may want to store something more helpful
                    //convo[$"{dc.ActiveDialog.State["name"]} reservation"] = msg;

                    await dc.Context.SendActivity(msg);
                    await dc.End();
                }
            });

            // Add a prompt for the reservation date
            dialogs.Add("dateTimePrompt", new Microsoft.Bot.Builder.Dialogs.DateTimePrompt(Culture.English));
            // Add a prompt for the party size
            dialogs.Add("partySizePrompt", new Microsoft.Bot.Builder.Dialogs.NumberPrompt<int>(Culture.English));
            // Add a prompt for the user's name
            dialogs.Add("textPrompt", new Microsoft.Bot.Builder.Dialogs.TextPrompt());
        }
        /// <summary>

        /// </summary>
        /// <param name="context">Turn scoped context containing all the data needed
        /// for processing this conversation turn. </param>        
        public async Task OnTurn(ITurnContext context)
        {
            messageSent = false;
            
            if (context.Activity.Type == ActivityTypes.Message)
            {
                // The type parameter PropertyBag inherits from 
                // Dictionary<string,object>
                //var dialogs = new DialogSet();
                var state = ConversationState<Dictionary<string, object>>.Get(context);
                //var state2 = ConversationState<Dictionary<string, object>>.Get(context);
                var dc = dialogsT.CreateContext(context, state);
                var dcS = dialogSas.CreateContext(context, state);
                await dc.Continue();
                await dcS.Continue();

                // Additional logic can be added to enter each dialog depending on the message received

                if (!context.Responded)
                {
                    var stuff = context.Activity.Text.ToLowerInvariant();
                    if (stuff.Contains("reserve table"))
                    {
                        await dc.Begin("reserveTable");
                    }
                    else if (stuff.Contains("party"))
                    {
                        try
                        {
                            await dcS.Begin("sassyText");
                        }
                        catch
                        {
                            await context.SendActivity("Sorry, had trouble in sassyText");
                        }
                    }
                    else
                    {
                        await context.SendActivity($"You said '{context.Activity.Text}'");
                    }
                }
            }
            //else
            //{
            //    if (!messageSent)
            //        await context.SendActivity("Hello! Type 'reserve table' to begin your reservation.");
            //    messageSent = true;

            //}
        }
    }    
}
