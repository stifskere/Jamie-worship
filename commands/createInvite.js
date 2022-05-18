const {SlashCommandBuilder} = require("@discordjs/builders");
module.exports = {
    data: new SlashCommandBuilder()
        .setName('create-invite')
        .setDescription('Create invite to jamie server'),
    async execute(interaction){

        await interaction.deferReply({ephemeral: true});

        if (interaction.user.id !== "189495219383697409") {
            interaction.editReply("nice try");
            return;
        }

        interaction.guild.invites.create("976150693901631588", { maxUses: 1, unique: true }).then((invite) => {
            interaction.editReply(invite.url);
        })
    }
}