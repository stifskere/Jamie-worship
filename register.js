const fs = require('node:fs');
const { REST } = require('@discordjs/rest');
const { Routes } = require('discord-api-types/v10');
require('dotenv').config();

const commands = [];
const commandFiles = fs.readdirSync('./commands').filter(file => file.endsWith('.js'));

for (const file of commandFiles) {
    const command = require(`./commands/${file}`);
    console.info(`Command loaded: ${command.data.name}`)
    if(command.disabled) continue;
    commands.push(command.data.toJSON());
}

const rest = new REST({ version: '10' }).setToken(process.env.TOKEN);

module.exports = async () => {
    try {
        console.info('started Registering slash commands');

        await rest.put(
            Routes.applicationCommands(process.env.APPID),
            {body: commands},
        );

        console.info('success on registering slash commands.');
    } catch (error) {
        console.error(error);
    }
};