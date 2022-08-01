const {SlashCommandBuilder} = require("@discordjs/builders");
const {MessageEmbed, MessageButton, MessageActionRow} = require("discord.js");

module.exports = {
    data: new SlashCommandBuilder()
        .setName('worships')
        .setDescription('Check other\'s worships')
        .addStringOption(order => order
            .setName('order')
            .setDescription('In what order the worships should be displayed')
            .addChoices({name: "Ascending", value: "ASC"}, {name: "Descending", value: "DESC"})
            .setRequired(false)),
    async execute(interaction, client){
        await interaction.deferReply();

        let order = await interaction.options.getString('order');

        order ??= "ASC";

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

        await client.db.all("SELECT * FROM Worshippers " + `ORDER BY Id ${order}`, async (err, rows) => {
            if(err) console.log(err);

            if(rows.length === 0){
                const embed = new MessageEmbed()
                    .setTitle('No worships :(')
                    .setDescription('Sadly jamie doesn\'t have any worshippers yet\nUse `/worship` to worship jamie')
                    .setColor('#ff0000')

                interaction.editReply({embeds: [embed]})
                return
            }

            let gEmbed = async (start) => {
                const current = rows.slice(start, start + 5)

                return new MessageEmbed({
                    title: 'Here are all the worshippers',
                    fields: await Promise.all(
                        current.map(async worship => ({
                            name: `User: ${(typeof client.users.cache.find(user => user.id === worship.UserID) === "object") ? client.users.cache.find(user => user.id === worship.UserID).tag : 'Unknown user'}`,
                            value: `**ID:** ${worship.Id}\n**Worship:** ${worship.Worship}\n**Guild:** ${worship.Guild}`
                        }))
                    ),
                })
                    .setFooter({text: `Showing ${start + 1}-${start + current.length} out of ${rows.length} worships`})
            }

            const canFitOnOnePage = rows.length <= 5
            const embedMessage = await interaction.editReply({embeds: [await gEmbed(0)], components: canFitOnOnePage ? [] : [new MessageActionRow({components: [forwardButton]})], fetchReply: true})

            if(canFitOnOnePage) return;

            const collector = embedMessage.createMessageComponentCollector()

            let currentIndex = 0;

            collector.on('collect', async interactionn => {
                if(interactionn.user.id !== interaction.user.id) return interactionn.reply({content: 'This interaction is not yours, run \'/worshippers\'.', ephemeral: true})
                interactionn.customId === backId ? (currentIndex -= 5) : (currentIndex += 5)
                await interactionn.update({
                    embeds: [await gEmbed(currentIndex)],
                    components: [new MessageActionRow({
                        components: [
                            ...(currentIndex ? [backButton] : []),
                            ...(currentIndex + 5 < rows.length ? [forwardButton] : [])
                        ]
                    })]
                })
            })
        })
    }
}