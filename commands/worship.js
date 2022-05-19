const { SlashCommandBuilder } = require('@discordjs/builders');
const {MessageEmbed} = require("discord.js");
const ms = require('ms')
const sqlite = require('sqlite3').verbose();
const path = require('path');

module.exports = {
    data: new SlashCommandBuilder()
        .setName('worship')
        .setDescription('worship jamie')
        .addStringOption(worship => worship
            .setName('worship')
            .setDescription('The text you want to say to our god')
            .setRequired(true)),
    async execute(interaction, client){
        await interaction.deferReply();

        const worship = interaction.options.getString('worship').replace(/[^a-zA-Z,.]/g, '');

        const jamie = client.users.cache.find(user => user.id === '39412760139805491');

        let db = new sqlite.Database(path.join(path.resolve('./databases/'), `${interaction.guild.id}.db`), sqlite.OPEN_READWRITE | sqlite.OPEN_CREATE)

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

        const embed4 = new MessageEmbed()
            .setTitle('You already worshipped')
            .setDescription('Jamie already have received your worship')
            .setFooter({text: 'You can\'t send more than 1 worship'})
            .setColor('#ff0000')

        db.all(`SELECT * FROM Worshippers WHERE UserID = (?)`, [interaction.user.id], async (err, row) => {
            if(new Date().getTime() > obj){
                if(row.length > 0){
                    interaction.editReply({embeds: [embed4]})
                    return;
                }
                try{
                    db.run(`INSERT INTO Worshippers VALUES (?, ?)`, [interaction.user.id, worship])
                    await interaction.editReply({embeds: [embed2]})
                    if(typeof jamie === 'object'){
                        const arrPhrases = ["You got worshiped my lord", "Someone worshiped you", "They are glad you exist", "Hello god, i got a worship"]
                        jamie.send(`${arrPhrases[Math.floor(Math.random() * arrPhrases.length)]}\nThey said: ${worship}`)
                    }
                }catch{
                    await interaction.editReply({embeds: [embed3]})
                }
            }else{
                await interaction.editReply({embeds: [embed]})
            }
        })
        db.close();
    }
}