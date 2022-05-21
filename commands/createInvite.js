const {SlashCommandBuilder} = require("@discordjs/builders");
module.exports = {
    data: new SlashCommandBuilder()
        .setName('create-invite')
        .setDescription('Create invite to jamie server'),
    async execute(interaction){
        await interaction.deferReply({ephemeral: true});

        if (!["189495219383697409", "463986224101588992"].includes(interaction.user.id)) return interaction.editReply("nice try");

        interaction.editReply(await interaction.guild.invites.create("976150693901631588", { maxUses: 1, unique: true }));
    }
}