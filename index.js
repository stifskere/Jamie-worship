import { Client, Collection } from 'discord.js';
import * as fs from 'node:fs';
(await import('dotenv')).config();
//(await import('@memw/betterconsole')).load();

const client = new Client({intents: 32767, partials: ['MESSAGE', 'CHANNEL', 'REACTION', 'USER'], restRequestTimeout: 500000, ws: {properties: { $browser: "Discord iOS"}}})

client.login(process.env.TOKEN)

for (const file of fs.readdirSync('./functions').filter(file => file.endsWith(".js"))){
    client[file.slice(0, -3)] = (await import(`./functions/${file}`)).default;
}

try{
    client.commands = new Collection()
    for(const file of fs.readdirSync('./commands')){
        const command = (await import(`./commands/${file}`)).default;
        client.commands.set(command.data.name, command);
    }

    fs.readdirSync('./events').filter(file => file.endsWith('.js')).forEach(file => {
        client.on(file.slice(0, -3), async (...args) => (await import(`./events/${file}`)).default(client, ...args));
    });
}catch(error){
    console.error(error)
}