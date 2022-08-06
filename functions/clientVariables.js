const path = require("path");
const sqlite = require("sqlite3").verbose()

module.exports = (client) => {
    client.db = new sqlite.Database(path.join(path.resolve('./databases/'), `global.db`), sqlite.OPEN_READWRITE | sqlite.OPEN_CREATE);
    client.botStats = {
        commandUsage: {},
        worshipsNum: 0,
        commandCount: 0
    }
}