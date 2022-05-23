const {SlashCommandBuilder} = require("@discordjs/builders");
module.exports = {
    data: new SlashCommandBuilder()
        .setName('config')
        .setDescription('Server configuration for the bot')
        .addSubcommand(help => help
            .setName('help')
            .setDescription('Sends an embed with help for the command'))
        .addSubcommand(change => change
            .setName('update')
            .setDescription('Updates the actual config')
            .addStringOption(option => option
                .setName('option')
                .setDescription('the option to change')
                .addChoices(
                    {name: 'Welcome channel - the channel to send the welcome message', value: 'wlcChannel'},
                    {name: 'Jamie praise - Enable or disable random replies when a message contains jamie', value: 'jamiePraise'},
                    {name: 'Welcome enabled - Enable or disable the welcome message to be sent', value: 'wlcEnabled'})
                .setRequired(true))
            .addStringOption(value => value
                .setName('value')
                .setDescription('The value to change the configuration value to')
                .setRequired(true))),
    async execute(interaction, client){
        interaction.deferReply({ephemeral: true});

    }
}