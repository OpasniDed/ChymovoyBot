using DiscordBot.config;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System.Data.Common;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordBot
{
    internal class Program
    {
        public static DiscordClient Client { get; set; }
        public static CommandsNextExtension Commands { get; set; }
        

        public static async Task Main(string[] args)
        {
            var jsonReader = new JSONreader();
            await jsonReader.ReadJson();

            var discordConfig = new DiscordConfiguration()
            {
                Token = jsonReader.token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                AutoReconnect = true
            };

            Client = new DiscordClient(discordConfig);

            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(2)
            });


            Client.Ready += ClientOnReady;
            Client.ComponentInteractionCreated += ClientOnComponentInteractionCreated;
            Client.ModalSubmitted += ModalSubmitted;

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { jsonReader.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = false
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<commands.Commands>();

            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static async Task ClientOnComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs ev)
        {
            switch (ev.Interaction.Data.CustomId)
            {
                case "mrp":
                    string last = "1/10 - Лор, знание админ панели, коммуникация. Сколько наигранно часов?";

                    string predlast = "Почему вы хотите стать администратором?";

                    var jsonReader3 = new JSONreader();
                    await jsonReader3.ReadJson();


                    if (ev.Interaction.ChannelId == jsonReader3.ChannelForMrp)
                    {
                        last = "1/10 - Лор, знание админ панели, коммуникация. Сколько наигранно часов?";
                        predlast = "Почему вы хотите стать администратором?";
                    }
                    if (ev.Interaction.ChannelId == jsonReader3.channelForNr)
                    {
                        last = "1/10 - Навык работы в коллективе, знание правил. Время в игре, время в дс сервере";
                        predlast = "Знание админ панели (1/10)";
                    }

                    var modal = new DiscordInteractionResponseBuilder()
                        .WithTitle("Анкета")
                        .WithCustomId("user_form")
                        .AddComponents(new TextInputComponent(
                            label: "Ваш возраст",
                            customId: "age_field",
                            placeholder: "Введите возраст",
                            required: true,
                            style: TextInputStyle.Short))
                        .AddComponents(new TextInputComponent(
                            label: "Ссылка на стим профиль",
                            customId: "url_field",
                            placeholder: "Ссылка",
                            required: true,
                            style: TextInputStyle.Short))
                        .AddComponents(new TextInputComponent(
                            label: predlast,
                            customId: "why_field",
                            placeholder: "Расскажите",
                            required: true,
                            style: TextInputStyle.Paragraph))
                        .AddComponents(new TextInputComponent(
                            label: "Есть ли у вас опыт администрирования?",
                            customId: "experience_field",
                            placeholder: "Если да, то где и какой?",
                            required: true,
                            style: TextInputStyle.Paragraph))
                        .AddComponents(new TextInputComponent(
                            label: "Другая информация",
                            customId: "knowledgelor_field",
                            placeholder: last,
                            required: true,
                            style: TextInputStyle.Paragraph));
                    await ev.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);

                    break;
                case "acceptZ":
                    ulong currentchannel = 1;
                    ulong currentroleid = 1;
                    string otdel = "Неизвестно";
                    
                    var jsonReader = new JSONreader();
                    await jsonReader.ReadJson();
                    var mem = await ev.Interaction.Guild.GetMemberAsync(ev.Interaction.User.Id);
                    if (ev.Interaction.ChannelId == jsonReader.channelsendnr)
                    {
                        currentchannel = jsonReader.channelsendnr;
                        currentroleid = jsonReader.roleidforbtnnr;
                        otdel = "No Rules";
                    }
                    if (ev.Interaction.ChannelId == jsonReader.channelsend)
                    {
                        currentchannel = jsonReader.channelsend;
                        currentroleid = jsonReader.roleidforbtn;
                        otdel = "Medium RP";
                    }
                        if (!mem.Roles.Any(r => r.Id == currentroleid))
                    {
                        await ev.Interaction.CreateResponseAsync(
                            InteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder()
                                .WithContent("❌ У вас нет прав для использования этой кнопки")
                                .AsEphemeral(true));
                        return;
                    }
                    try
                    {
                        await ev.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                        var buttonMessage = await ev.Interaction.Channel.GetMessageAsync(ev.Message.Id);
                        if (buttonMessage == null || buttonMessage.Embeds.Count == 0)
                        {
                            await ev.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                                .WithContent("Ошибка: не найдено сообщение с embed")
                                .AsEphemeral(true));
                            return;
                        }

                        var embed = buttonMessage.Embeds[0];
                        var userField = embed.Fields.FirstOrDefault(f => f.Name == "Заявку подал");
                        if (userField == null)
                        {
                            await ev.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                                .WithContent("Ошибка: не найдено поле с пользователем")
                                .AsEphemeral(true));
                            return;
                        }

                        var userMention = userField.Value;
                        var userIdMatch = Regex.Match(userMention, @"<@!?(\d+)>");
                        if (!userIdMatch.Success)
                        {
                            await ev.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                                .WithContent("Ошибка: неверный формат упоминания")
                                .AsEphemeral(true));
                            return;
                        }

                        var userId = ulong.Parse(userIdMatch.Groups[1].Value);

                        var editedEmbed = new DiscordEmbedBuilder(embed)
                            .WithColor(DiscordColor.Green)
                            .WithFooter($"✅ Заявка одобрена | {ev.Interaction.Guild.Name} | {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .AddField("Заявку рассмотрел", ev.Interaction.User.Mention, inline: true);

                        var disabledButtons = new List<DiscordButtonComponent>
                        {
                            new DiscordButtonComponent(ButtonStyle.Success, "acceptZ", "Принять",
                                emoji: new DiscordComponentEmoji("✅"), disabled: true),
                            new DiscordButtonComponent(ButtonStyle.Danger, "rejectZ", "Отклонить",
                                emoji: new DiscordComponentEmoji("❎"), disabled: true)
                        };


                        var tasks = new List<Task>();

                        try
                        {
                            var message = new DiscordEmbedBuilder()
                            {
                                Title = "Заявка на администрацию",
                                Description = $"Ваша заявка на администрацию в отделе {otdel} была рассмотрена и принята!\nС вами свяжуться в ближайшее время, если нет - попробуйте написать сами",
                                Color = DiscordColor.Green
                            };
                            var member = await ev.Interaction.Guild.GetMemberAsync(userId);
                            tasks.Add(member.SendMessageAsync(message));
                        }
                        catch (DSharpPlus.Exceptions.UnauthorizedException)
                        {
                            Console.WriteLine("Ошибка: нет прав для отправки сообщения пользователю");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при отправке сообщения пользователю: {ex.Message}");
                        }

                        tasks.Add(buttonMessage.ModifyAsync(new DiscordMessageBuilder()
                            .WithEmbed(editedEmbed.Build())
                            .AddComponents(disabledButtons)));

                        tasks.Add(ev.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                            .WithContent("Вы приняли заявку")
                            .AsEphemeral(true)));

                        await Task.WhenAll(tasks);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Произошла критическая ошибка: {ex}");
                        try
                        {
                            await ev.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                                .WithContent($"Произошла ошибка: {ex.Message}")
                                .AsEphemeral(true));
                        }
                        catch { }
                    }
                    break;
                case "rejectZ":
                    ulong currentchannel2 = 1;
                    ulong currentroleid2 = 1;

                    var jsonReader2 = new JSONreader();
                    await jsonReader2.ReadJson();
                    var mem2 = await ev.Interaction.Guild.GetMemberAsync(ev.Interaction.User.Id);
                    if (ev.Interaction.ChannelId == jsonReader2.channelsendnr)
                    {
                        currentchannel2 = jsonReader2.channelsendnr;
                        currentroleid2 = jsonReader2.roleidforbtnnr;
                    }
                    if (ev.Interaction.ChannelId == jsonReader2.channelsend)
                    {
                        currentchannel2 = jsonReader2.channelsend;
                        currentroleid2 = jsonReader2.roleidforbtn;
                    }
                    if (!mem2.Roles.Any(r => r.Id == currentroleid2))
                    {
                        await ev.Interaction.CreateResponseAsync(
                            InteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder()
                                .WithContent("❌ У вас нет прав для использования этой кнопки")
                                .AsEphemeral(true));
                        return;
                    }
                    try
                    {
                        var moda1 = new DiscordInteractionResponseBuilder()
                            .WithTitle("Укажите причину отказа")
                            .WithCustomId($"reject_reason_{ev.Message.Id}") 
                            .AddComponents(new TextInputComponent(
                                label: "Причина отказа",
                                customId: "reason_field",
                                placeholder: "Опишите причину отказа...",
                                required: true,
                                style: TextInputStyle.Paragraph));

                        await ev.Interaction.CreateResponseAsync(InteractionResponseType.Modal, moda1);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при создании модального окна: {ex}");
                        await ev.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                            .WithContent("Не удалось создать форму для указания причины отказа")
                            .AsEphemeral(true));
                    }
                    break;
            }
        }

        private static async Task ModalSubmitted(DiscordClient sender, ModalSubmitEventArgs ev)
        {
            if (ev.Interaction.Data.CustomId == "user_form")
            {
                ulong currentchannel = 1;
                ulong roleformention = 1;

                var jsonReader = new JSONreader();
                await jsonReader.ReadJson();
                string age = ev.Values["age_field"];
                string url = ev.Values["url_field"];
                string why = ev.Values["why_field"];
                string exc = ev.Values["experience_field"];
                string other = ev.Values["knowledgelor_field"];
                string fieldText = "Другая информация (Лор, знание админ панели, коммуникация, часы в игре)";
                string fieldText2 = "Почему вы хотите стать администратором";


                if (ev.Interaction.ChannelId == jsonReader.channelForNr)
                {
                    currentchannel = jsonReader.channelsendnr;
                    roleformention = jsonReader.rolementionnr;
                    fieldText = "Другая информация (Навык работы в коллективе, время в игре, знание правил, время в дс сервере)";
                    fieldText2 = "Знание админ панели (1/10)";
                }
                if (ev.Interaction.ChannelId == jsonReader.ChannelForMrp)
                {
                    currentchannel = jsonReader.channelsend;
                    roleformention = jsonReader.rolemention;
                    fieldText = "Другая информация (Лор, знание админ панели, коммуникация, часы в игре)";
                    fieldText2 = "Почему вы хотите стать администратором";
                }

                string guildName = ev.Interaction.Guild.Name;
                DateTime currentTime = DateTime.Now;
                string formattedtime = currentTime.ToString("dd.MM.yyyy HH:mm");
                var acceptButton = new DiscordButtonComponent(ButtonStyle.Success, customId: "acceptZ", label: "Принять", emoji: new DiscordComponentEmoji("✅"));
                var rejectButton = new DiscordButtonComponent(ButtonStyle.Danger, customId: "rejectZ", label: "Отклонить", emoji: new DiscordComponentEmoji("❎"));


                var embed = new DiscordEmbedBuilder()
                {
                    Title = $"Новая заявка",
                    Color = DiscordColor.Yellow,
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = $"{ev.Interaction.User.Username} подал новую заявку",
                        IconUrl = ev.Interaction.User.AvatarUrl
                    },
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"💡 Заявка ожидает рассмотрения | {ev.Interaction.Guild.Name} | {formattedtime}",
                        IconUrl = ev.Interaction.Guild.IconUrl
                    }
                };
                embed.AddField("Заявку подал", ev.Interaction.User.Mention, inline: false);
                embed.AddField("Возраст", age);
                embed.AddField("Ссылка на стим профиль", url);
                embed.AddField(fieldText2, why);
                embed.AddField("Был ли у вас опыт в администрировании", exc);
                embed.AddField(fieldText, other);
                var logChannel = await sender.GetChannelAsync(currentchannel);
                if (logChannel == null)
                {
                    await ev.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Вы не настроили канал").AsEphemeral(true));
                    return;
                }
                var MessageBuilder = new DiscordMessageBuilder()
                    .WithContent($"<@&{roleformention}>")
                    .AddEmbed(embed.Build())
                    .AddComponents(acceptButton, rejectButton)
                    .WithAllowedMentions(new IMention[] { new RoleMention(roleformention) });
                await logChannel.SendMessageAsync(MessageBuilder);
                await ev.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Ваша заявка отправлена ✅").AsEphemeral(true));
            }
            if (ev.Interaction.Data.CustomId.StartsWith("reject_reason_"))
            {
                var jsonReader = new JSONreader();
                await jsonReader.ReadJson();
                string otdel = "Неизвестно";
                var messageIdStr = ev.Interaction.Data.CustomId["reject_reason_".Length..];
                if (!ulong.TryParse(messageIdStr, out var messageId))
                {
                    await ev.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Ошибка обработки запроса").AsEphemeral(true));
                    return;
                }
                if (ev.Interaction.ChannelId == jsonReader.channelsend)
                {
                    otdel = "Medium RP";
                }
                if (ev.Interaction.ChannelId == jsonReader.channelsendnr)
                {
                    otdel = "No Rules";
                }
                

                var reason = ev.Values["reason_field"];
                await ev.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                var origMessage = await ev.Interaction.Channel.GetMessageAsync(messageId);
                if (origMessage == null || origMessage.Embeds.Count == 0)
                {
                    await ev.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                    .WithContent("Сообщение с заявкой не найдено")
                    .AsEphemeral(true));
                    return;
                }
                var embed = origMessage.Embeds[0];
                var userField = embed.Fields.FirstOrDefault(f => f.Name == "Заявку подал");
                if (userField == null)
                {
                    await ev.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                    .WithContent("Не найдена информация о пользователе")
                    .AsEphemeral(true));
                    return;
                }
                var userMention = userField.Value;
                var userIdMatch = Regex.Match(userMention, @"<@!?(\d+)>");
                if (!userIdMatch.Success)
                {
                    await ev.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                        .WithContent("Неверный формат упоминания пользователя")
                        .AsEphemeral(true));
                    return;
                }
                var userId = ulong.Parse(userIdMatch.Groups[1].Value);

                var editedEmbed = new DiscordEmbedBuilder(embed)
                    .WithColor(DiscordColor.Red)
                    .WithFooter($"❌ Заявка отклонена | {ev.Interaction.Guild.Name} | {DateTime.Now:dd.MM.yyyy HH:mm}", iconUrl: ev.Interaction.Guild.IconUrl)
                    .AddField("Заявку рассмотрел", ev.Interaction.User.Mention, inline: true)
                    .AddField("Причина отказа", reason)
                    ;

                var disabledButtons = new List<DiscordButtonComponent>
                {
                    new DiscordButtonComponent(ButtonStyle.Success, "acceptZ", "Принять",
                        emoji: new DiscordComponentEmoji("✅"), disabled: true),
                    new DiscordButtonComponent(ButtonStyle.Danger, "rejectZ", "Отклонить",
                        emoji: new DiscordComponentEmoji("❎"), disabled: true)
                };

                var tasks = new List<Task>();

                try
                {
                    var message = new DiscordEmbedBuilder()
                    {
                        Title = "Заявка на администрацию",
                        Description = $"Ваша заявка на администрацию в отделе {otdel} была отклонена.\nПричина: {reason}",
                        Color = DiscordColor.Red
                    };
                    var member = await ev.Interaction.Guild.GetMemberAsync(userId);
                    tasks.Add(member.SendMessageAsync(message));
                }
                catch (DSharpPlus.Exceptions.UnauthorizedException)
                {
                    Console.WriteLine("Нет прав для отправки сообщения пользователю");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при отправке сообщения пользователю: {ex.Message}");
                }
                tasks.Add(origMessage.ModifyAsync(new DiscordMessageBuilder()
                .WithEmbed(editedEmbed.Build())
                .AddComponents(disabledButtons)));

                tasks.Add(ev.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                    .WithContent($"Вы отклонили заявку. Причина: {reason}")
                    .AsEphemeral(true)));

                await Task.WhenAll(tasks);
                
            } 
        }
        
        private static Task ClientOnReady(DiscordClient sender, ReadyEventArgs ev)
        {
            return Task.CompletedTask;
        }
    } 
}