module.exports = async () => {
    const path = require("path");
    const sqlite = require('sqlite3').verbose()
    const fs = require('fs');
    if(!fs.existsSync('./databases')) fs.mkdirSync('./databases')
    let db = new sqlite.Database(path.join(path.resolve('./databases/'), `global.db`), sqlite.OPEN_READWRITE | sqlite.OPEN_CREATE)
    await db.run(`CREATE TABLE IF NOT EXISTS Worshippers(UserID VARCHAR NOT NULL, Worship VARCHAR NOT NULL, Guild VARCHAR NOT NULL)`)
    console.info('Global database created')
}