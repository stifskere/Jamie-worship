const { SlashCommandBuilder } = require('@discordjs/builders');
const {MessageEmbed} = require("discord.js");
module.exports = {
    data: new SlashCommandBuilder()
        .setName('worship')
        .setDescription('worship jamie'),
    async execute(interaction, client){
        await interaction.deferReply();

        const embed = new MessageEmbed()
            .setTitle('Not yet')
            .setDescription('It is not time to say hello Jamie yet')
            .setFooter({text: 'we appreciate your attempt tho'})
            .setColor('#ff0000')

        await interaction.editReply({embeds: [embed]})
    }
}