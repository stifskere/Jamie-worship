const {SlashCommandBuilder} = require("@discordjs/builders");
const {MessageAttachment} = require("discord.js");
module.exports = {
    data: new SlashCommandBuilder()
        .setName('send')
        .setDescription('send')
        .addSubcommand(desc => desc
            .setName('gif')
            .setDescription('gif')),

    async execute(interaction, client){
        const jamieObject = client.guilds.cache.get('976149800447770624').members.cache.get("394127601398054912").user
        const photo = new MessageAttachment('./images/helloJamie.gif', 'helloJamie.gif')

        try{
            jamieObject.send({content: `**From:** ${interaction.user.tag}`, files: [photo]})
            interaction.reply({content: 'Your jamie gif was sent', ephemeral: true})
        }catch{
            interaction.reply({content: `There was an error on sending your jamie gif`})
        }

    }
}