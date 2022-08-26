const { EmbedBuilder, AttachmentBuilder, SlashCommandBuilder} = require("discord.js");
module.exports = {
    data: new SlashCommandBuilder()
        .setName('jamie')
        .setDescription('Gives Jamie info')
        .addSubcommand(profile => profile
            .setName('profile')
            .setDescription('Shows Jamie info or something'))
        .addSubcommand(photo => photo
            .setName('photo')
            .setDescription('sends the Jamie photo')),
    async execute(interaction, client){
        await interaction.deferReply();

        const closeFriends = [];

        let role = client.guilds.cache.get('976149800447770624').roles.cache.get('976175903371571220')
        let interactionGuildPromise = await interaction.guild.members.fetch()

        if(typeof role === "object"){
            role.members.map(async member => {
                if(typeof await interactionGuildPromise.find(f => f.id === member.id) === 'object') closeFriends.push(member.user.tag)
            })
        }

        setTimeout(async () => {
            const jamieObject = await client.users.fetch("394127601398054912");
            const isJamieObject = typeof jamieObject === "object";
            if(interaction.options.getSubcommand() === 'profile'){
                let jamie = {
                    heIS: (isJamieObject),
                    tag: (isJamieObject) ? jamieObject.tag : "Jamie#8409",
                    id: (isJamieObject) ? jamieObject.id : "394127601398054912",
                    pfp: (isJamieObject) ? jamieObject.avatarURL() : "https://cdn.discordapp.com/avatars/394127601398054912/c7a08756f08ff4fa9f51cf5f63f017d0.png?size=4096"
                }

                client.db.all(`SELECT count(*) FROM Worshippers`, async (err, WorshippersCountRow) => {
                    client.db.all(`SELECT * FROM JamieInfo WHERE Key = 'messageNum'`, async (err, MessageNumRow) => {
                        client.db.all(`SELECT * FROM JamieInfo WHERE Key = 'LastMessage'`, async (err, LastMessageRow) => {
                            const embed = new EmbedBuilder()
                                .setTitle('Jamie')
                                .setDescription(`**Jamie\'s tag:** ${jamie.tag}\n**Jamie\'s id:** ${jamie.id}\n\nHe lives in UK even tho he says he doesn\'t`)
                                .addFields(
                                    {"name": 'Jamie\'s close friends', "value": (closeFriends.length !== 0) ? closeFriends.toString().replaceAll(',', '\n') : "No close friends on this server."},
                                    {"name": 'Jamie\'s bot stats', "value": `**Worships:** ${WorshippersCountRow[0]['count(*)']}\n**Counted messages:** ${MessageNumRow[0].Value}\n**Last message:** ${LastMessageRow[0].Value}`}
                                )
                                .setThumbnail(jamie.pfp)
                                .setColor(Math.floor(Math.random() * 0xFFFFFF))

                            if(jamie.heIS === false) embed.setFooter({text: 'The info may not be up to date since the user can\'t be fetched.'})

                            interaction.editReply({embeds: [embed]})
                        })
                    })
                })
            }else if(interaction.options.getSubcommand() === 'photo'){
                try{
                    const photo = new AttachmentBuilder('./images/helloJamie.gif', 'helloJamie.gif')
                    await interaction.channel.send({files: [photo]})
                    interaction.deleteReply();
                }catch{
                    interaction.editReply({content: `Can't send images in this channel`})
                }
            }
        }, 500)
    }
}