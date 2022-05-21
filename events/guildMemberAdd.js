const {MessageEmbed} = require("discord.js");
module.exports = (client, member) => {
    const wlcEmbed = new MessageEmbed()
        .setTitle(`Welcome ${member.user.username}`)
        .setDescription('Check <#976150169542348842> or run /worship')
        .setColor('RANDOM')
        .setThumbnail('https://cdn.discordapp.com/avatars/394127601398054912/c7a08756f08ff4fa9f51cf5f63f017d0.png?size=4096')

    member.guild.channels.cache.get('976149800447770628').send({embeds: [wlcEmbed]});
}