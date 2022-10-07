import { ShardingManager } from "discord.js";
(await import("dotenv")).config();
(await import("@memw/betterconsole")).load();

const manager = new ShardingManager('./index.js', {
    token: process.env.TOKEN,
    respawn: true
});

(async () => {
    manager.on('shardCreate', shard => {
        console.info(`Launched shard ${shard.id}`)
    });

    await manager.spawn();
})();