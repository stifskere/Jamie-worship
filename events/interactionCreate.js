module.exports = async (client, interaction) => {
    if(!interaction) console.error('Unknown interaction, couldn\'t reply to the interaction')
    if(!interaction.inGuild()) return interaction.reply({content: 'No worshipping in DM allowed.', ephemeral: true});
    const command = client.commands.get(interaction.commandName);
    if(!command) return;
    if(command.disabled === true) return interaction.reply({content: 'This command is disabled.', ephemeral: true});
    await command.execute(interaction, client);
    let interactionOptions;
    try{interactionOptions = interaction.options.getSubcommand();}catch{interactionOptions = null;}
    client.botStats.commandUsage[`${interaction.commandName}${interactionOptions ? interactionOptions : ""}`] = (client.botStats.commandUsage[`${interaction.commandName}${interactionOptions ? interactionOptions : ""}`] || 0) + 1;
    client.botStats.commandCount++;
}