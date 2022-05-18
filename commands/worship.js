const { SlashCommandBuilder } = require('@discordjs/builders');
const {MessageEmbed} = require("discord.js");
const ms = require('ms')
const sqlite = require('sqlite3').verbose();
const path = require('path');

module.exports = {
    data: new SlashCommandBuilder()
        .setName('worship')
        .setDescription('worship jamie'),
    async execute(interaction){
        await interaction.deferReply();

        let db = new sqlite.Database(path.join(path.resolve('../databases/'), `${interaction.guild.id}.db`), sqlite.OPEN_READWRITE | sqlite.OPEN_CREATE)

        const obj = 1653134400 * 1000

        const embed = new MessageEmbed()
            .setTitle('Not yet')
            .setDescription('It is not time to say hello Jamie yet')
            .addField('You will be able in:', ms(obj - new Date().getTime(), {long: true}))
            .setFooter({text: 'we appreciate your attempt tho'})
            .setColor('#ff0000')

        await interaction.editReply({embeds: [embed]})
    }
}