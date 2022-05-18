const {SlashCommandBuilder} = require("@discordjs/builders");
const path = require("path");
const {MessageEmbed, MessageButton, MessageActionRow} = require("discord.js");
const ms = require("ms");
const sqlite = require('sqlite3').verbose();

module.exports = {
    data: new SlashCommandBuilder()
        .setName('worships')
        .setDescription('Check other\'s worships'),
    async execute(interaction, client){
        await interaction.deferReply();

        let db = new sqlite.Database(path.join(path.resolve('./databases/'), `${interaction.guild.id}.db`), sqlite.OPEN_READWRITE | sqlite.OPEN_CREATE)

        const backId = 'back'
        const forwardId = 'forward'
        const backButton = new MessageButton({
            style: 'SECONDARY',
            label: 'Back',
            emoji: '⬅️',
            customId: backId
        })
        const forwardButton = new MessageButton({
            style: 'SECONDARY',
            label: 'Forward',
            emoji: '➡️',
            customId: forwardId
        })

        await db.all(`SELECT * FROM Worshippers`, async (err, rows) => {
            if(err) console.log(err);

            if(rows.length === 0){
                const embed = new MessageEmbed()
                    .setTitle('No worships :(')
                    .setDescription('Sadly jamie doesn\'t have any worshippers yet\nUser /worship to worship jamie')
                    .setColor('#ff0000')

                interaction.editReply({embeds: [embed]})
                return
            }

            let gEmbed = async (start) => {
                const current = rows.slice(start, start + 10)

                return new MessageEmbed({
                    title: 'Here are all the worshippers',
                    fields: await Promise.all(
                        current.map(async worship => ({
                            name: `User: ${(typeof  client.users.cache.find(user => user.id === worship.UserID) === "object") ? client.users.cache.find(user => user.id === worship.UserID).tag : 'Unknown user'}`,
                            value: `Worship: ${worship.Worship}`
                        }))
                    ),
                })
                    .setFooter({text: `Showing ${start + 1}-${start + current.length} out of ${rows.length} worships`})
            }

            const canFitOnOnePage = rows.length <= 10
            const embedMessage = await interaction.editReply({embeds: [await gEmbed(0)], components: canFitOnOnePage ? [] : [new MessageActionRow({components: [forwardButton]})], fetchReply: true})

            if(canFitOnOnePage) return;

            const collector = embedMessage.createMessageComponentCollector({filter: ({user}) => user.id === interaction.user.id})

            let currentIndex = 0;

            collector.on('collect', async interaction => {
                interaction.customId === backId ? (currentIndex -= 10) : (currentIndex += 10)
                await interaction.update({
                    embeds: [await gEmbed(currentIndex)],
                    components: [new MessageActionRow({
                        components: [
                            ...(currentIndex ? [backButton] : []),
                            ...(currentIndex + 10 < rows.length ? [forwardButton] : [])
                        ]
                    })]
                })
            })
        })
        db.close();
    }
}