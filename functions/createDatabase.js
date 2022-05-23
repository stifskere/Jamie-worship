module.exports = async (id) => {
    const path = require("path");
    const sqlite = require('sqlite3').verbose()
    const sleep = require('sleep')
    let db = new sqlite.Database(path.join(path.resolve('./databases/'), `${id}.db`), sqlite.OPEN_READWRITE | sqlite.OPEN_CREATE)
    await db.run(`CREATE TABLE IF NOT EXISTS Worshippers(UserID VARCHAR NOT NULL, Worship VARCHAR NOT NULL)`)
    await db.run(`CREATE TABLE IF NOT EXISTS Configuration(Option VARCHAR NOT NULL, Value VARCHAR NOT NULL)`)
    await db.run(`INSERT INTO Configuration VALUES (?, ?)`, ['wlcChannel', 'false'])
    await db.run(`INSERT INTO Configuration VALUES (?, ?)`, ['jamiePraise', 'true'])
    await db.run(`INSERT INTO Configuration VALUES (?, ?)`, ['wlcEnabled', 'false'])
}