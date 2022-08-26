const childProcess = require("node:child_process");

module.exports = async (client) => {
    const statuses = ["Whatever Jamie is doing", "Jamie", "Jamie again", "Jamie my beloved"]

    let i = 0;

    setInterval(async () => {
        if(i >= statuses.length) i = 0;
        await client.user.setActivity(statuses[i], {type: 'WATCHING'}).catch();
        i++;
    }, 3600000);

    await require("../register.js")();

    await console.info('Ready to worship jamie');

    client.createDatabase();
    client.clientVariables(client);
}