import { ChannelType } from "discord.js";

export default async (client, interaction) => {
    if(process.env.PRODUCTION === "false" && !client.worshipModerators.includes(interaction.user.id)) return interaction.reply({content: "The bot is on testing mode, currently only it's developers can use slash commands", ephemeral: true});
    if(!interaction) console.error('Unknown interaction, couldn\'t reply to the interaction');
    const command = client.commands.get(interaction.commandName);
    if(!command) return;
    if(command.disabled === true) return interaction.reply({content: 'This command is disabled.', ephemeral: true});
    await command.execute(interaction, client);
    let interactionOptions = interaction.options.getSubcommand(false)
    client.botStats.commandUsage[`${interaction.commandName} ${interactionOptions ? interactionOptions : ""}`] = (client.botStats.commandUsage[`${interaction.commandName} ${interactionOptions ? interactionOptions : ""}`] || 0) + 1;
    client.botStats.commandCount++;
}