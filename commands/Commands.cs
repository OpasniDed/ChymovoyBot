using DiscordBot.games;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.commands
{
    public class Commands : BaseCommandModule
    {


        [Command(name: "createbtn")]
        [RequireRoles(RoleCheckMode.MatchIds, roleIds: new ulong[] { 1162457149365026866, 1287447456505790557, 1287447467679551572, 1383096589601476718, 1287447473744252978, 1383087406479183902, 1383079478779183135, 1287447515087634545 })]
        public async Task CreateBtn(CommandContext ctx)
        {


            var jsonReader = new DiscordBot.config.JSONreader();
            await jsonReader.ReadJson();
            var button = new DiscordButtonComponent(ButtonStyle.Primary, customId: "mrp", label: "Подать заявку", emoji: new DiscordComponentEmoji("📩"));
            var embed = new DiscordEmbedBuilder()
            {
                Title = "Подача заявок",
                Color = DiscordColor.Azure
            };
            string text = "1. Не менее 14 лет\n2. Вы обязаны знать базовый лор, правила нашего сервера и уметь отыгрывать\n3. Вы обязаны быть адекватным и терпеливым\n 4. Пробыть на дискорд сервере не меньше 15-и дней\n 5. Если вы активный в дискорд сервере/SCP:SL сервере, требование по дискорду снимается\n 6. Вы не должны быть данный момент администратором на другом проекте\n***В случае отказа, следующую заявку можно подать только через 1 неделю***\n**Нажмите на кнопку ниже, чтобы подать заявку**";
            if (ctx.Channel.Id == jsonReader.channelForNr)
            {
                text = "1. Не менее 14 лет\n2. 200 часов в игре\n 3. Времени на дс сервере минимум месяц\n***В случае отказа, следующую заявку можно подать только через 1 неделю***\n**Нажмите на кнопку ниже, чтобы подать заявку**";
            }

            embed.AddField("Критерии", text, inline: true);
            var messageBuilder = new DiscordMessageBuilder()
                .AddEmbed(embed.Build())
                .AddComponents(button);
            await ctx.Channel.SendMessageAsync(messageBuilder);
        }

    }
}
