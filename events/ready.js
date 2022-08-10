const childProcess = require("node:child_process");

module.exports = (client) => {
    const statuses = ["Whatever Jamie is doing", "Jamie", "Jamie again", "Jamie my beloved"]

    let i = 0;

    setInterval(() => {
        if(i >= statuses.length) i = 0;
        try{client.user.setActivity(statuses[i], {type: 'WATCHING'});}catch{}
        i++;
    }, 3600000)

    childProcess.exec('node register.js', async (err, stdout) => {
        if(err) return console.log(`There was an error on registering commands:\n${err}`);
        await console.log(stdout)
        await console.info('Ready to worship jamie');
    })

    client.createDatabase();
    client.clientVariables(client);
}