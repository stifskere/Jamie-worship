import {ActivityType} from "discord-api-types/v10";

export default async (client) => {
    const statuses = ["Whatever Jamie is doing", "Jamie", "Jamie again", "Jamie my beloved"]

    let statusInterval = 0;
    setInterval(async () => {
        if(statusInterval >= statuses.length) statusInterval = 0;
        await client.user.setPresence({activities: [{name: statuses[statusInterval], type: ActivityType.Watching}]});
        statusInterval++;
    }, 3600000);

    await (await import("../register.js")).default();

    await console.info('Ready to worship jamie');

    client.createDatabase();
    client.clientVariables(client);
}