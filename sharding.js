const { ShardingManager } = require('discord.js');
require('dotenv').config();
require('@memw/betterconsole')();

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