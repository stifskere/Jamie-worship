const { SlashCommandBuilder } = require('@discordjs/builders');
const {MessageEmbed} = require("discord.js");
const ms = require('ms')
const mysql = require("mysql");
module.exports = {
    data: new SlashCommandBuilder()
        .setName('worship')
        .setDescription('worship jamie'),
    async execute(interaction){
        const mysql = require('mysql')
        const connection = mysql.createConnection({
            host: 'localhost',
            user: 'u9_Vbk78R9vyM',
            password: '.vH4IcH+mQgd0B1X7NXQMWD9'
        })

        connection.connect((error) => {
            if(error) return console.log(error);

            console.log('Connection established')
        })

        await interaction.deferReply();

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