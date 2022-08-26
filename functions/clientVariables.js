import * as path from "node:path";
const sqlite = (await import("sqlite3")).default.verbose();

export default (client) => {
    client.db = new sqlite.Database(path.join(path.resolve('./databases/'), `global.db`), sqlite.OPEN_READWRITE | sqlite.OPEN_CREATE);
    client.botStats = {
        commandUsage: {},
        worshipsNum: 0,
        commandCount: 0
    }
    client.worshipModerators = ['394127601398054912', '189495219383697409', '463986224101588992', '423565487121629196'];
}