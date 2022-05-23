const {SlashCommandBuilder} = require("@discordjs/builders");
const path = require("path");
const sqlite = require('sqlite3').verbose();
module.exports = {
    data: new SlashCommandBuilder()
        .setName('reply')
        .setDescription('Jamie can reply to its worships')
        .addIntegerOption(id => id
            .setName('id')
            .setDescription('id of the worship you want to reply')
            .setRequired(true))
        .addStringOption(content => content
            .setName('content')
            .setDescription('What you want to reply with?')
            .setRequired(true)),
    async execute(interaction, client){
        await interaction.deferReply({ephemeral: true})

        if(interaction.user.id !== '463986224101588992'){
            await interaction.editReply({content: `Only jamie can use this command.`})
        }

        const worshipId = await interaction.options.getInteger('id')
        const replyContent = await interaction.options.getString('content')

        let db = new sqlite.Database(path.join(path.resolve('./databases/'), `global.db`), sqlite.OPEN_READWRITE | sqlite.OPEN_CREATE)

        db.all(`SELECT * FROM Worshippers WHERE Id = ${worshipId}`, async (err, row) => {
            if(!row){
                await interaction.editReply({content: `A worship for this ID doesn\'t exist.`})
                return;
            }

            try{
                const user = await client.users.cache.find(user => user.id === row[0].UserID)

                if(typeof user !== "object") return await interaction.editReply({content: 'This user couldn\'t be found, so your reply was not sent'})

                await user.send({content: `**Jamie replied to your worship with:**\n${replyContent}`})
                await interaction.editReply({content: `Your reply was sent to ${user}`})
            }catch{
                await interaction.editReply({content: `This user DMS are disabled, your reply was not sent`})
            }
        })
    }
}