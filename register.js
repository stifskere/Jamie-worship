const fs = require('node:fs');
const { REST } = require('@discordjs/rest');
const { Routes } = require('discord-api-types/v9');
require('dotenv').config()

const commands = [];
const commandFiles = fs.readdirSync('./commands').filter(file => file.endsWith('.js'));

for (const file of commandFiles) {
    const command = require(`./commands/${file}`);
    console.log(`Command loaded: ${command.data.name}`)
    if(command.disabled) continue;
    commands.push(command.data.toJSON());
}

const rest = new REST({ version: '9' }).setToken(process.env.TOKEN);

(async () => {
    try {
        console.log('started Registering slash commands');

        await rest.put(
            Routes.applicationCommands(process.env.APPID),
            {body: commands},
        );

        console.log('success on registering slash commands.');
        console.log('Ready to worship jamie');
    } catch (error) {
        console.error(error);
    }
})();