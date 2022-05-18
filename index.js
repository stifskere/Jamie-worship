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
    console.log('Ready to worship jamie')
    client.user.setActivity('Jamie', {type: 'WATCHING'});
    childProcess.exec('npm run register', (err, stdout) => {
        if(err) return console.log(`There was an error on registering commands:\n${err}`);
        console.log(stdout)
    })
});

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