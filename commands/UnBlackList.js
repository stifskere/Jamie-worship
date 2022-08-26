import { ContextMenuCommandBuilder, ApplicationCommandType } from "discord.js";

export default {
    data: new ContextMenuCommandBuilder()
        .setName('Remove from blacklist')
        .setType(ApplicationCommandType.User),

    async execute(interaction, client){
        if(!client.worshipModerators.includes(interaction.user.id)){
            await interaction.reply({content: "You cannot remove worshippers from blacklist, you need to be a JamieWorship moderator.", ephemeral: true});
            return;
        }
        if(client.worshipModerators.includes(interaction.targetUser.id)){
            await interaction.reply({content: "This user is a moderator, which is not blacklisted.", ephemeral: true});
            return;
        }
        client.db.all(`SELECT id FROM BlackListedUsers WHERE id = '${interaction.targetUser.id}'`, async (err, rows) => {
            if(rows.length !== 0){
                client.db.run(`DELETE FROM BlackListedUsers WHERE id = '${interaction.targetUser.id}'`);
                await interaction.reply({content: `${interaction.targetUser} was removed from the blacklist successfully, the user will now be able to worship.`, ephemeral: true});
            }else{
                await interaction.reply({content: "User is not blacklisted", ephemeral: true});
            }
        })
    }
}