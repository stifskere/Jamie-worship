const { SlashCommandBuilder } = require('@discordjs/builders');
const {MessageEmbed} = require("discord.js");
const ms = require('ms')
const sqlite = require('sqlite3').verbose();
const path = require('path');

module.exports = {
    data: new SlashCommandBuilder()
        .setName('worship')
        .setDescription('worship Jamie')
        .addStringOption(worship => worship
            .setName('worship')
            .setDescription('The text you want to say to our god')
            .setRequired(true)),
    async execute(interaction){
        await interaction.deferReply();

        const worship = interaction.options.getString('worship').replace(/[^a-zA-Z,. ]/g, '');

        if(worship.length > 100){
            await interaction.editReply({content: `Your answer can't be more than 100 characters long`})
            return;
        }

        const jamie = interaction.guild.members.cache.get("394127601398054912");

        let db = new sqlite.Database(path.join(path.resolve('./databases/'), `global.db`), sqlite.OPEN_READWRITE | sqlite.OPEN_CREATE)

        const obj = 1653156000 * 1000

        const embed = new MessageEmbed()
            .setTitle('Not yet')
            .setDescription('It is not time to say hello Jamie yet')
            .addField('You will be able in:', ms(obj - new Date().getTime(), {long: true}))
            .setFooter({text: 'we appreciate your attempt tho'})
            .setColor('#ff0000')

        const embed2 = new MessageEmbed()
            .setTitle('Worship sent')
            .setDescription(`Your worship was sent successfully\nyou sent: ${worship}`)
            .setFooter({text: 'You can check others worships using /worships'})
            .setColor('#2bde00')

        const embed3 = new MessageEmbed()
            .setTitle('There was an error')
            .setDescription('Your worship failed, sorry about that')
            .setFooter({text: 'You can retry, you will lose nothing.'})
            .setColor('#ff0000')

            if(new Date().getTime() > obj){
                try{
                    db.run(`INSERT INTO Worshippers VALUES (?, ?, ?, null)`, [interaction.user.id, worship, interaction.guild.name])
                    await interaction.editReply({embeds: [embed2]})
                    if(typeof jamie === 'object'){
                        const arrPhrases = ["You got worshipped my lord", "Someone worshipped you", "They are glad you exist", "Hello god, I got a worship"]
                        jamie.send(`${arrPhrases[Math.floor(Math.random() * arrPhrases.length)]}\nThey said: ${worship}`)
                    }
                }catch{
                    await interaction.editReply({embeds: [embed3]})
                }
            }else{
                await interaction.editReply({embeds: [embed]})
            }

        db.close();
    }
}