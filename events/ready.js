const childProcess = require("node:child_process");
const fs = require("node:fs");
const path = require("node:path");
module.exports = (client) => {
    const statuses = ["Whatever Jamie is doing", "Jamie", "Jamie again", "Jamie my beloved"]

    let i = 0;

    setInterval(() => {
        if(i >= statuses.length) i = 0;
        client.user.setActivity(statuses[i], {type: 'WATCHING'});
        i++;
    }, 30000)

    childProcess.exec('node register.js', async (err, stdout) => {
        if(err) return console.log(`There was an error on registering commands:\n${err}`);
        await console.log(stdout)
        await console.info('Ready to worship jamie');
    })

    if(!fs.existsSync('./databases/global.db')) client.createDatabase();
}