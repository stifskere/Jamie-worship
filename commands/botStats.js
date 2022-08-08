const {SlashCommandBuilder, EmbedBuilder} = require("@discordjs/builders");
const {MessageEmbed} = require("discord.js");
const moment = require("moment")
module.exports = {
    data: new SlashCommandBuilder()
        .setName("bot")
        .setDescription("Bot status command group")
        .addSubcommand(s => s.setName("stats").setDescription("View the actual bot status.")),

    async execute (interaction, client) {
        let usedCommandsString = "  Command Name  |  Count\n--------------------------\n";

        if(client.botStats.commandCount > 0){
            Object.entries(client.botStats.commandUsage).forEach(entry => {
                const [key, value] = entry;
                console.log(key)
                let countAddition = ".........................";
                const parsedKey = key.replace(/((?<=\p{Upper})\p{Lower})|((?!^)\p{Upper}(?=\p{Lower}))/gu, ' $0');
                countAddition = `${parsedKey}${countAddition.substring(parsedKey.length)}`;
                countAddition = `${countAddition.substring(0, countAddition.length-value.toString().length)}${value.toString()}`;
                usedCommandsString += `${countAddition}\n`;
            })
        }else{
            usedCommandsString = "No commands used yet.";
        }

        const statsEmbed = new MessageEmbed()
            .setTitle("Bot stats")
            .setDescription("This shows the current bot stats since the last bot restart.")
            .addFields({name: "🔹 General stats", value: `**Current up time:** ${new Date(client.uptime).toISOString().slice(11, 19)}\n**Worships sent:** ${client.botStats.worshipsNum}\n**Total commands used:** ${client.botStats.commandCount}`},
                {name: "🔹 detailed command usage", value: `\`\`\`\n${usedCommandsString}\`\`\``})
            .setColor('RANDOM')
            .setFooter({text: "Report any errors to Memw#6969"})
            .setTimestamp()

        await interaction.reply({embeds: [statsEmbed]});
    }
}