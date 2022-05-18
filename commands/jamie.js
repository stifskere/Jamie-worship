const {SlashCommandBuilder} = require("@discordjs/builders");
const {MessageEmbed} = require("discord.js");
module.exports = {
    data: new SlashCommandBuilder()
        .setName('jamie')
        .setDescription('Gives jamie info'),
    async execute(interaction){
        await interaction.deferReply({ephemeral: true});

        const closeFriends = [];

        interaction.guild.roles.cache.get('976175903371571220').members.map(member => {
            closeFriends.push(member.user.tag)
        })

        const embed = new MessageEmbed()
            .setTitle('Jamie')
            .setDescription('**Jamie\'s tag:** Jamie#8409\n**Jamie\'s id:** 394127601398054912\n\nHe lives in UK even tho he says he doesn\'t')
            .addField('Jamie\'s close friends', closeFriends.toString().replaceAll(',', '\n'))
            .setThumbnail('https://cdn.discordapp.com/avatars/394127601398054912/c7a08756f08ff4fa9f51cf5f63f017d0.png?size=4096')
            .setFooter({text: 'The info may not be up to date since the user can\'t be fetched.'})
            .setColor('RANDOM')

        interaction.editReply({embeds: [embed]})
    }
}