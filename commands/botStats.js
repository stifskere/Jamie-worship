import { EmbedBuilder , SlashCommandBuilder } from "discord.js";
import axios from "axios";

export default {
    data: new SlashCommandBuilder()
        .setName("bot")
        .setDescription("Bot status command group")
        .setDMPermission(false)
        .addSubcommand(s => s
            .setName("stats")
            .setDescription("View the actual bot status.")),

    async execute (interaction, client) {
        let usedCommandsString = "  Command Name  |  Count\n--------------------------\n";

        if(client.botStats.commandCount > 0){
            Object.entries(client.botStats.commandUsage).forEach(entry => {
                const [key, value] = entry;
                let countAddition = ".........................";
                const parsedKey = key.replace(/((?<=\p{Upper})\p{Lower})|((?!^)\p{Upper}(?=\p{Lower}))/gu, ' $0');
                countAddition = `${parsedKey}${countAddition.substring(parsedKey.length)}`;
                countAddition = `${countAddition.substring(0, countAddition.length-value.toString().length)}${value.toString()}`;
                usedCommandsString += `${countAddition}\n`;
            })
        }else{
            usedCommandsString = "No commands used yet.";
        }

        const seconds = client.uptime / 1000, minutes = seconds / 60, hours = minutes / 60, days = hours / 24,
        fSeconds = Math.floor(seconds) % 60, fMinutes = Math.floor(minutes) % 60, fHours = Math.floor(hours) % 24, fDays = Math.floor(days),
        parsedString = `${fDays !== 0 ? `${fDays<10 ? "0"+fDays : fDays} day${fDays === 1 ? "" : "s"},` : ""} ${fHours !== 0 ? `${fHours<10 ? "0"+fHours : fHours} hour${fHours === 1 ? "" : "s"},` : ""} ${fMinutes !== 0 ? `${fMinutes<10 ? "0"+fMinutes : fMinutes} minute${fMinutes === 1 ? "" : "s"},` : ""} ${fSeconds<10 ? "0"+fSeconds : fSeconds} second${fSeconds === 1 ? "" : "s"}`

        let commits = [];
        for(let i = 1; i < Infinity; i++){
            const req = (await axios.get(`https://api.github.com/repos/stifskere/Jamie-worship/commits?per_page=100&page=${i}`, {auth: {
                    username: process.env.GITUSER,
                    password: process.env.GITTOKEN
                }})).data;
            commits = commits.concat(req);
            if(req.length !== 100) break;
        }

        const statsEmbed = new EmbedBuilder()
            .setTitle("Bot stats")
            .setDescription("This shows the current bot stats since the last bot restart.")
            .addFields({name: "🔹 General stats", value: `**Current up time:** ${parsedString}\n**Worships sent:** ${client.botStats.worshipsNum}\n**Total commands used:** ${client.botStats.commandCount}`},
                {name: "🔹 detailed command usage", value: `\`\`\`\n${usedCommandsString}\`\`\``},
                {name: "🔹 GitHub status", value: `**Commits:** ${commits.length}\n**Last commit name:** ${commits[0].commit.message}\n**Last commit author:** ${commits[0].commit.author.name}\n**Last commit content:** [click to view changes](${commits[0].html_url})`})
            .setColor(Math.floor(Math.random() * 0xFFFFFF))
            .setFooter({text: "Report any errors to Memw#6969"})
            .setTimestamp()

        await interaction.reply({embeds: [statsEmbed]});
    }
}