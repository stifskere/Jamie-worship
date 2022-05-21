const {MessageEmbed} = require("discord.js");
module.exports = async (client, message) => {
    if(message.author.bot) return;

    if(message.author.id === '394127601398054912'){
        const embed = new MessageEmbed()
            .setTitle('Jamie said something')
            .setDescription(`**He said:** ${message.content}`)
            .setFooter({text: 'I\'l keep whatever our god says in here'})

        message.guild.channels.cache.get('976832701015420998').send({embeds: [embed]})
        return;
    }

    if(message.content.toLowerCase().includes('jamie')){
        const answers = ["Want to talk about jamie with me?", "Did you say jamie?", "I also like Jamie", "Jamie is a god for me", "You talking about jamie and not telling me!?"];
        await message.reply(answers[Math.floor(Math.random() * answers.length)]);
    }
}