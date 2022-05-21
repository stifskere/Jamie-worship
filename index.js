const { Client, Collection } = require('discord.js')
require('dotenv').config()
const fs = require('node:fs')

const client = new Client({intents: 32767, partials: ['MESSAGE', 'CHANNEL', 'REACTION', 'USER'], restRequestTimeout: 500000})

client.login(process.env.TOKEN)

for (const file of fs.readdirSync('./functions').filter(file => file.endsWith(".js"))){
    client[file.slice(0, -3)] = require(`./functions/${file}`);
}

client.commands = new Collection()
for(const file of fs.readdirSync('./commands')){
    const command = require(`./commands/${file}`);
    client.commands.set(command.data.name, command);
}

fs.readdirSync('./events').filter(file => file.endsWith('.js')).forEach(file => {
    client.on(file.slice(0, -3), async (...args) => require(`./events/${file}`)(client, ...args));
});