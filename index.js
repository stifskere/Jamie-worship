const { Client, Collection } = require('discord.js')
require('dotenv').config()
const fs = require('node:fs')
const path = require('node:path')

const client = new Client({intents: 32767, partials: ['MESSAGE', 'CHANNEL', 'REACTION', 'USER'], restRequestTimeout: 500000})

client.login(process.env.TOKEN)

for (const file of fs.readdirSync(path.resolve('./functions')).filter(file => file.endsWith(".js"))){
    client[file.slice(0, -3)] = require(path.resolve(`./functions/${file}`));
}

for(const file of fs.readdirSync(path.resolve('./commands'))){
    const command = require(path.resolve(`./commands/${file}`));
    client.commands = new Collection().set(command.data.name, command);
}

fs.readdirSync(path.resolve('./events')).filter(file => file.endsWith('.js')).forEach(file => {
    client.on(file.slice(0, -3), async (...args) => require(`./events/${file}`)(client, ...args));
});