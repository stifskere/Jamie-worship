const { SlashCommandBuilder } = require('@discordjs/builders');
const {MessageEmbed} = require("discord.js");
const ms = require('ms')


module.exports = {
    data: new SlashCommandBuilder()
        .setName('worship')
        .setDescription('worship Jamie')
        .addStringOption(worship => worship
            .setName('worship')
            .setDescription('The text you want to say to our god')
            .setRequired(true)),
    async execute(interaction, client){
        await interaction.deferReply();

        let worship = interaction.options.getString('worship').replace(/[*`]/g, '');

        if(worship.length > 100){
            await interaction.editReply({content: `Your worship can't be more than 100 characters long`});
            return;
        }

        worship = worship.split(" ");

        for(let i = 0; i < worship.length; i++) {
            if (worship[i].includes("https://cdn.discordapp.com")) worship[i] = `[click to view attachment](${worship[i]})`;
        }

        worship = worship.join("");

        const jamie = await client.users.fetch("394127601398054912");

        const obj = 1653156000 * 1000

        const embed = new MessageEmbed()
            .setTitle('Not yet')
            .setDescription('It is not time to say hello Jamie yet')
            .addFields({"name": 'You will be able in:', "value": ms(obj - new Date().getTime(), {long: true})})
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
                    client.db.run(`INSERT INTO Worshippers VALUES (?, ?, ?, null)`, [interaction.user.id, worship, interaction.guild.name])
                    await interaction.editReply({embeds: [embed2]})
                    if(typeof jamie === 'object'){
                        const arrPhrases = ["You got worshipped my lord", "Someone worshipped you", "They are glad you exist", "Hello god, I got a worship"]
                        try{
                            jamie.send(`${arrPhrases[Math.floor(Math.random() * arrPhrases.length)]}\nThey said: ${worship}`)
                        }catch{
                            console.warn("jamie blocked the bot, couldn't send the worship message")
                        }
                    }
                    client.botStats.worshipsNum++;
                }catch{
                    await interaction.editReply({embeds: [embed3]})
                }
            }else{
                await interaction.editReply({embeds: [embed]})
            }
    }
}