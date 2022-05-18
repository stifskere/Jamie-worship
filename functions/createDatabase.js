module.exports = (id) => {
    const path = require("path");
    const sqlite = require('sqlite3').verbose()
    let db = new sqlite.Database(path.join(path.resolve('./databases/'), `${id}.db`), sqlite.OPEN_READWRITE | sqlite.OPEN_CREATE)
    db.run(`CREATE TABLE IF NOT EXISTS Worshippers(UserID VARCHAR NOT NULL, Worship VARCHAR NOT NULL)`)
}