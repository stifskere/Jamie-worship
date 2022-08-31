import {AttachmentBuilder, EmbedBuilder, SlashCommandBuilder} from "discord.js";

export default {
    data: new SlashCommandBuilder()
        .setName('send')
        .setDescription('send')
        .setDMPermission(false)
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
                const photo = new AttachmentBuilder('./images/helloJamie.gif', 'helloJamie.gif');

                const jamieEmbed = new EmbedBuilder()
                    .setTitle("Jamie gif sent to jamie")
                    .setDescription("Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie Jamie")
                    .setColor(Math.floor(Math.random() * 0xFFFFFF));

                try{
                    jamieObject.send({content: `**From:** ${interaction.user.tag}`, files: [photo]});
                    await interaction.editReply({embeds: [jamieEmbed]});
                }catch{
                    jamieEmbed
                        .setTitle("Gif was not sent to jamie :(")
                        .setDescription("No jamie No jamie No jamie No jamie No jamie No jamie No jamie No jamie No jamie No jamie No jamie No jamie")
                        .setFooter({text: "Not enough perms."})
                        .setColor(0xff0000);

                    await interaction.editReply({embeds: [jamieEmbed]});
                }
            }
        })
    }
}