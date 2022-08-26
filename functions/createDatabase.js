import * as path from "node:path";
import * as fs from "node:fs";
const sqlite = (await import("sqlite3")).default.verbose();

export default async () => {
    if(!fs.existsSync('./databases')) {
        fs.mkdirSync('./databases')
        console.warn('There aren\'t any databases folder')
        console.info('Global databases folder and database created')
    }
    const db = new sqlite.Database(path.join(path.resolve('./databases/'), `global.db`), sqlite.OPEN_READWRITE | sqlite.OPEN_CREATE)
    await db.run(`CREATE TABLE IF NOT EXISTS Worshippers(UserID VARCHAR NOT NULL, Worship VARCHAR NOT NULL, Guild VARCHAR NOT NULL, Id INTEGER PRIMARY KEY AUTOINCREMENT)`)
    await db.run(`CREATE TABLE IF NOT EXISTS JamieInfo(Key VARCHAR NOT NULL, Value VARCHAR NOT NULL, unique(Key))`)
    await db.run(`CREATE TABLE IF NOT EXISTS BlackListedUsers(id VARCHAR NOT NULL, unique(id))`)
    await fillJamieInfoDataBase(db);
}

async function fillJamieInfoDataBase(db) {
    await db.run(`INSERT OR IGNORE INTO JamieInfo(Key, Value) VALUES('messageNum', '0')`);
    await db.run(`INSERT OR IGNORE INTO JamieInfo(Key, Value) VALUES('LastMessage', 'Any "last message" sent yet')`);
}