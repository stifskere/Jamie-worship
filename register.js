import * as fs from "node:fs";
import { REST } from "@discordjs/rest";
import { Routes } from "discord-api-types/v10";
(await import("dotenv")).config();

const commands = [];
const commandFiles = fs.readdirSync('./commands').filter(file => file.endsWith('.js'));

for (const file of commandFiles) {
    const command = (await import(`./commands/${file}`)).default;
    console.info(`Command loaded: ${command.data.name}`)
    if(command.disabled) continue;
    commands.push(command.data.toJSON());
}

const rest = new REST({ version: '10' }).setToken(process.env.TOKEN);

export default async () => {
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