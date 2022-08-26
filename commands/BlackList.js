const { ContextMenuCommandBuilder, ApplicationCommandType } = require("discord.js");
module.exports = {
    data: new ContextMenuCommandBuilder()
        .setName('Black list')
        .setType(ApplicationCommandType.User),

    async execute(interaction, client){
        if(!client.worshipModerators.includes(interaction.user.id)){
            await interaction.reply({content: "You cannot blacklist worshippers, you need to be a JamieWorship moderator.", ephemeral: true});
            return;
        }
        if(interaction.user.id === interaction.targetUser.id || client.worshipModerators.includes(interaction.targetUser.id)){
            await interaction.reply({content: "You cannot blacklist yourself or any moderator.", ephemeral: true})
            return;
        }
        client.db.all(`SELECT id FROM BlackListedUsers WHERE id = '${interaction.targetUser.id}'`, async (err, rows) => {
            if(rows.length === 0){
                client.db.run(`INSERT OR IGNORE INTO BlackListedUsers VALUES (?)`, [interaction.targetUser.id]);
                await interaction.reply({content: `${interaction.targetUser} was blacklisted successfully, he won't be able to send worships anymore`, ephemeral: true});
            }else{
                await interaction.reply({content: "User is already blacklisted", ephemeral: true});
            }
        })
    }
}