import {EmbedBuilder, SlashCommandBuilder} from "discord.js";

export default {
    data: new SlashCommandBuilder()
        .setName("leaderboard")
        .setDescription("Leaderboard of worships by user"),

    async execute(interaction, client){
        client.db.all("SELECT * FROM Worshipers", async (err, rows) => {
            const worshipersObject = {};
            for(let i = 0; i < rows.length; i++) worshipersObject[rows[i].UserID] = worshipersObject[rows[i].UserID] + 1 || 1;
            const worshipersArray = [];
            for(let i in worshipersObject) worshipersArray.push([i, worshipersObject[i]]);
            worshipersArray.sort((a, b) => b[1] - a[1]);
            const thisUserPos = worshipersArray.findIndex(x => x.id === interaction.user.id);
            const embed = new EmbedBuilder()
                .setTitle("Leaderboard")
                .setDescription("these are the top 5 worshipers who worshiped the most")
                .setFooter({text: thisUserPos === -1 ? "You don't have any worship yet, use /worship to worship jamie" : `You are ${thisUserPos < 3 ? `${thisUserPos}st` : `${thisUserPos}th`} in a descending list of all the worshippers`})
                .setColor(Math.floor(Math.random() * 0xFFFFFF));
            for(let i = 1; i <= 5; i++) embed.addFields({name: (i < 3 ? `${i}st. ` : `${i}th. `) +  client.getTag(await client.users.fetch(worshipersArray[i - 1][0])) ?? "Unknown user#0000", value: `**With:** ${worshipersArray[i - 1][1]} worships`});
            await interaction.reply({embeds: [embed]});
        });
    },
}