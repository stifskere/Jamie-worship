const {SlashCommandBuilder} = require("@discordjs/builders");
const {MessageEmbed, MessageAttachment} = require("discord.js");
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
    async execute(interaction){
        await interaction.deferReply();

        const closeFriends = [];

        let role = interaction.guild.roles.cache.get('976175903371571220')

        if(typeof role === "object"){
            role.members.map(member => {
                closeFriends.push(member.user.tag)
            })
        }

        const jamieObject = interaction.guild.members.cache.get("394127601398054912").user
        if(interaction.options.getSubcommand() === 'profile'){
            const jamie = {
                heIS: (typeof jamieObject === "object"),
                tag: (typeof jamieObject === "object") ? jamieObject.tag : "Jamie#8409",
                id: (typeof jamieObject === "object") ? jamieObject.id : "394127601398054912",
                pfp: (typeof jamieObject === "object") ? jamieObject.avatarURL() : "https://cdn.discordapp.com/avatars/394127601398054912/c7a08756f08ff4fa9f51cf5f63f017d0.png?size=4096"
            }

            const embed = new MessageEmbed()
                .setTitle('Jamie')
                .setDescription(`**Jamie\'s tag:** ${jamie.tag}\n**Jamie\'s id:** ${jamie.id}\n\nHe lives in UK even tho he says he doesn\'t`)
                .addField('Jamie\'s close friends', (closeFriends.length !== 0) ? closeFriends.toString().replaceAll(',', '\n') : "No close friends on this server.")
                .setThumbnail(jamie.pfp)
                .setColor('RANDOM')

            if(jamie.heIS === false) embed.setFooter({text: 'The info may not be up to date since the user can\'t be fetched.'})

            interaction.editReply({embeds: [embed]})
        }else if(interaction.options.getSubcommand() === 'photo'){
            try{
                const photo = new MessageAttachment('./images/helloJamie.gif', 'helloJamie.gif')
                await interaction.channel.send({files: [photo]})
                interaction.deleteReply();
            }catch{
                interaction.editReply({content: `Can't send images in this channel`})
            }
        }
    }
}