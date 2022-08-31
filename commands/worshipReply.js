import { SlashCommandBuilder } from "discord.js";

export default {
    data: new SlashCommandBuilder()
        .setName('reply')
        .setDescription('Jamie can reply to his worships')
        .addIntegerOption(id => id
            .setName('id')
            .setDescription('id of the worship you want to reply to')
            .setRequired(true))
        .addStringOption(content => content
            .setName('content')
            .setDescription('What do you want to reply with?')
            .setRequired(true)),
    async execute(interaction, client){
        await interaction.deferReply({ephemeral: true})
        if(interaction.user.id !== '394127601398054912'){
            await interaction.editReply({content: `Only jamie can use this command, you don't look like jamie`})
            return;
        }

        const worshipId = interaction.options.getInteger('id')
        const replyContent = interaction.options.getString('content')

        if(replyContent.length > 300){
            await interaction.editReply({content: `Your answer can't be more than 300 characters long`})
            return;
        }
        client.db.all(`SELECT * FROM Worshipers WHERE Id = ${worshipId}`, async (err, row) => {
            if(!row) return await interaction.editReply({content: `A worship for this ID doesn\'t exist.`});
            const user = await client.users.cache.find(user => user.id === row[0].UserID);
            if(typeof user !== "object") return await interaction.editReply({content: 'This user couldn\'t be found, so your reply was not sent'});
            await user.send({content: `**Jamie replied to your worship with:**\n${replyContent}`}).catch(async () => await interaction.editReply({content: `This user's DMS are disabled, your reply was not sent`}));
            await interaction.editReply({content: `Your reply was sent to ${user}`});

        })
    }
}