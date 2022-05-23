module.exports = async (client, interaction) => {
    if(!interaction) console.error('Unknown interaction, couldn\'t reply to the interaction')

    if(!interaction.inGuild()) return interaction.reply({content: 'No worshipping in DM allowed.', ephemeral: true});

    const command = client.commands.get(interaction.commandName);

    if(!command) return;

    if(command.disabled === true) return interaction.reply({content: 'This command is disabled.', ephemeral: true});

    try{
        await command.execute(interaction, client);
    }catch(error){
        console.error(error);
    }
}