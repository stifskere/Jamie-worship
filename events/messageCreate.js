const {MessageEmbed} = require("discord.js");
module.exports = async (client, message) => {
    if(message.author.bot) return;

    if(message.author.id === '394127601398054912'){
        const embed = new MessageEmbed()
            .setTitle('Jamie said something')
            .setDescription(`**He said:** ${message.content}\n**In:** ${message.guild.name}\n[Jump to message](${message.url})`)
            .setFooter({text: 'I\'l keep whatever our god says in here'})

        client.guilds.cache.get('976149800447770624').channels.cache.get('976832701015420998').send({embeds: [embed]})
        return;
    }

    //this will be for the moment
    if(message.content.toLowerCase().includes('jamie') && ['976149800447770624'].includes(message.guild.id)){
        const answers = ["Want to talk about Jamie with me?", "Did you say Jamie?", "I also like Jamie", "Jamie is a god for me", "You talking about jamie and not telling me!?"];
        await message.reply(answers[Math.floor(Math.random() * answers.length)]);
    }
}