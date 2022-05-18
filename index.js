const {Client, Collection, MessageEmbed } = require('discord.js')
const childProcess = require('node:child_process')
require('dotenv').config()
const fs = require('node:fs')
const path = require('node:path')

const client = new Client({
    intents: 32767,
    partials: ['MESSAGE', 'CHANNEL', 'REACTION', 'USER'],
    restRequestTimeout: 500000
})

client.login(process.env.TOKEN)

client.on('ready', () => {
    const statuses = ["Whatever Jamie is doing", "Jamie", "Jamie again", "Jamie my beloved"]

    let i = 0;

    setInterval(() => {
        if(i >= statuses.length) i = 0;
        client.user.setActivity(statuses[i], {type: 'WATCHING'});
        i++;
    }, 3000)

    childProcess.exec('npm run register', (err, stdout) => {
        if(err) return console.log(`There was an error on registering commands:\n${err}`);
        console.log(stdout)
    })

    client.guilds.cache.forEach(guild => {
        if(!fs.existsSync(path.join(path.resolve('./databases/'), `${guild.id}.db`))){
            client.createDatabase(guild.id);
        }
    })

    console.log('Ready to worship jamie')
});

const functionFiles = fs.readdirSync(path.resolve('./functions')).filter(file => file.endsWith(".js"));
for (const file of functionFiles){
    client[file.slice(0, -3)] = require(path.resolve(`./functions/${file}`));
}

client.commands = new Collection();
const commandFiles = fs.readdirSync(path.resolve('./commands'))
for(const file of commandFiles){
    const command = require(path.resolve(`./commands/${file}`));
    client.commands.set(command.data.name, command);
}

client.on('interactionCreate', async (interaction) => {
    if(!interaction.isCommand()) return;
    if(!interaction.inGuild()) return interaction.reply({content: 'No worshipping in DM allowed.', ephemeral: true});

    const command = client.commands.get(interaction.commandName);

    if(!command) return;

    if(command.disabled === true) return interaction.reply({content: 'This command is disabled.', ephemeral: true});

    try{
        await command.execute(interaction, client);
    }catch(error){
        console.error(error);
    }
})

client.on('messageCreate', async (message) => {
    if(message.author.bot) return;
    if(message.content.toLowerCase().includes('jamie')){
        const answers = ["Want to talk about jamie with me?", "Did you say jamie?", "I also like Jamie", "Jamie is a god for me", "You talking about jamie and not telling me!?"];
        await message.reply(answers[Math.floor(Math.random() * answers.length)]);
    }
})

client.on('guildMemberAdd', async (member) => {
    const wlcEmbed = new MessageEmbed()
        .setTitle(`Welcome ${member.user.username}`)
        .setDescription('Check <#976150169542348842> or run /worship')
        .setColor('RANDOM')
        .setThumbnail('https://cdn.discordapp.com/avatars/394127601398054912/c7a08756f08ff4fa9f51cf5f63f017d0.png?size=4096')

    member.guild.channels.cache.get('976149800447770628').send({embeds: [wlcEmbed]});
})