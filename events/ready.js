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
    }, 3000)

    childProcess.exec('node register.js', (err, stdout) => {
        if(err) return console.log(`There was an error on registering commands:\n${err}`);
        console.log(stdout)
    })

    client.guilds.cache.forEach(guild => {
            client.createDatabase(guild.id)
    })

    fs.readdirSync(path.resolve('./databases')).forEach(file => {
        if(!client.guilds.cache.has(file.slice(0, -3))){
            fs.rmSync(path.join(path.resolve('./databases/'), file))
        }
    })
}