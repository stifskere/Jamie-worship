const {SlashCommandBuilder, EmbedBuilder} = require("@discordjs/builders");
const {MessageAttachment, MessageEmbed} = require("discord.js");
module.exports = {
    data: new SlashCommandBuilder()
        .setName('send')
        .setDescription('send')
        .addSubcommand(desc => desc
            .setName('gif')
            .setDescription('gif')),

    async execute(interaction, client){
        client.db.all(`SELECT id FROM BlackListedUsers WHERE id = '${interaction.user.id}'`, async (err, rows) => {
            if(rows.length !== 0){
                await interaction.reply({content: "You cannot send gif to jamie, you are in the blacklist", ephemeral: true});
            }else{
                await interaction.deferReply();
                const jamieObject = await client.users.fetch("394127601398054912");
                const photo = new MessageAttachment('./images/helloJamie.gif', 'helloJamie.gif');

                const jamieEmbed = new MessageEmbed()
                    .setTitle("Jamie gif sent to jamie")
                    .setDescription("Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie")
                    .setColor('RANDOM');

                try{
                    jamieObject.send({content: `**From:** ${interaction.user.tag}`, files: [photo]});
                    await interaction.editReply({embeds: [jamieEmbed]});
                }catch{
                    jamieEmbed
                        .setTitle("Gif was not sent to jamie :(")
                        .setDescription("No jamie No jamie No jamie No jamie No jamie No jamie No jamie No jamie No jamie No jamie No jamie No jamie")
                        .setFooter({text: "Not enough perms."});

                    await interaction.editReply({embeds: [jamieEmbed]});
                }
            }
        })
    }
}