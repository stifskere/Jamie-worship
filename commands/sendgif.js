const {SlashCommandBuilder, EmbedBuilder} = require("@discordjs/builders");
const {MessageAttachment} = require("discord.js");
module.exports = {
    data: new SlashCommandBuilder()
        .setName('send')
        .setDescription('send')
        .addSubcommand(desc => desc
            .setName('gif')
            .setDescription('gif')),

    async execute(interaction, client){
        await interaction.deferReply();
        const jamieObject = await client.users.fetch("394127601398054912")
        const photo = new MessageAttachment('./images/helloJamie.gif', 'helloJamie.gif')

        const jamieEmbed = new EmbedBuilder()
            .setTitle("Jamie gif sent to jamie")
            .setDescription("Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie")
            .setColor("RANDOM")

        try{
            jamieObject.send({content: `**From:** ${interaction.user.tag}`, files: [photo]})
            await interaction.editReply({embeds: [jamieEmbed]})
        }catch{
            jamieEmbed
                .setTitle("Gif was not sent to jamie :(")
                .setDescription("No jamie No jamie No jamie No jamie No jamie No jamie No jamie No jamie No jamie No jamie No jamie No jamie")
                .setFooter({text: "Not enough perms."})

            await interaction.editReply({embeds: [jamieEmbed]})
        }

    }
}