// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.ComposerBot.Json;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class MessageTests
    {
        private static string getOsPath(string path) => Path.Combine(path.TrimEnd('\\').Split('\\'));

        private static readonly string samplesDirectory = getOsPath(@"..\..\..\..\..\..\..\Composer\packages\server\assets\projects");

        private static ResourceExplorer resourceExplorer = new ResourceExplorer();


        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
            string path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, samplesDirectory, "RespondingWithTextSample"));
            resourceExplorer.AddFolder(path);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            resourceExplorer.Dispose();
        }

        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task MessageTest()
        {
            await BuildTestFlow()
            .SendConversationUpdate()
                .AssertReply("What type of message would you like to send?\n\n   1. Simple Text\n   2. Text With Memory\n   3. Text With LG\n   4. LGWithParam\n   5. LGComposition\n   6. Structured LG\n   7. MultiLineText\n   8. IfElseCondition\n   9. SwitchCondition")
            .Send("1")
                .AssertReplyOneOf(new string[] { "Hello, this is a text with LG", "Hi, this is a text with LG", "Hey, this is a text with LG" })
                .AssertReply("What type of message would you like to send?\n\n   1. Simple Text\n   2. Text With Memory\n   3. Text With LG\n   4. LGWithParam\n   5. LGComposition\n   6. Structured LG\n   7. MultiLineText\n   8. IfElseCondition\n   9. SwitchCondition")
            .Send("2")
                .AssertReply("This is a text saved in memory.")
                .AssertReply("What type of message would you like to send?\n\n   1. Simple Text\n   2. Text With Memory\n   3. Text With LG\n   4. LGWithParam\n   5. LGComposition\n   6. Structured LG\n   7. MultiLineText\n   8. IfElseCondition\n   9. SwitchCondition")
            .Send("3")
                .AssertReplyOneOf(new string[] { "Hello, this is a text with LG", "Hi, this is a text with LG", "Hey, this is a text with LG" })
                .AssertReply("What type of message would you like to send?\n\n   1. Simple Text\n   2. Text With Memory\n   3. Text With LG\n   4. LGWithParam\n   5. LGComposition\n   6. Structured LG\n   7. MultiLineText\n   8. IfElseCondition\n   9. SwitchCondition")
            .Send("4")
                .AssertReply("Hello, I'm Zoidberg. What is your name?")
            .Send("luhan")
                .AssertReply("Hello luhan, nice to talk to you!")
                .AssertReply("What type of message would you like to send?\n\n   1. Simple Text\n   2. Text With Memory\n   3. Text With LG\n   4. LGWithParam\n   5. LGComposition\n   6. Structured LG\n   7. MultiLineText\n   8. IfElseCondition\n   9. SwitchCondition")
            .Send("5")
                .AssertReply("luhan nice to talk to you!")
                .AssertReply("What type of message would you like to send?\n\n   1. Simple Text\n   2. Text With Memory\n   3. Text With LG\n   4. LGWithParam\n   5. LGComposition\n   6. Structured LG\n   7. MultiLineText\n   8. IfElseCondition\n   9. SwitchCondition")
            .StartTestAsync();
        }

        private TestFlow BuildTestFlow(bool sendTrace = false)
        {
            TypeFactory.Configuration = new ConfigurationBuilder().Build();
            var storage = new MemoryStorage();
            var convoState = new ConversationState(storage);
            var userState = new UserState(storage);
            var adapter = new TestAdapter(TestAdapter.CreateConversation(TestContext.TestName), sendTrace);
            adapter
                .UseStorage(storage)
                .UseState(userState, convoState)
                .UseAdaptiveDialogs()
                .UseLanguageGeneration(resourceExplorer, "common.lg")
                .UseResourceExplorer(resourceExplorer)
                .Use(new TranscriptLoggerMiddleware(new FileTranscriptLogger()));

            var resource = resourceExplorer.GetResource("Main.dialog");
            var dialog = DeclarativeTypeLoader.Load<AdaptiveDialog>(resource, resourceExplorer, DebugSupport.SourceMap);
            DialogManager dm = new DialogManager(dialog);

            return new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                if (dialog is AdaptiveDialog planningDialog)
                {
                    await dm.OnTurnAsync(turnContext, cancellationToken).ConfigureAwait(false);
                }
            });
        }
    }
}
