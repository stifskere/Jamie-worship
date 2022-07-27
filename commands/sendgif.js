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
        await interaction.deferReply({ephemeral: true});
        const jamieObject = client.guilds.cache.get('976149800447770624').members.cache.get("394127601398054912").user
        const photo = new MessageAttachment('./images/helloJamie.gif', 'helloJamie.gif')

        try{
            jamieObject.send({content: `**From:** ${interaction.user.tag}`, files: [photo]})
            await interaction.editReply({content: 'Your jamie gif was sent', ephemeral: true})
        }catch{
            await interaction.editReply({content: `There was an error on sending your jamie gif`})
        }

    }
}