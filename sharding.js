const { ShardingManager } = require('discord.js');
require('dotenv').config();

const manager = new ShardingManager('./index.js', { token: process.env.TOKEN });

(async () => {
    manager.on('shardCreate', shard => console.log(`Launched shard ${shard.id}`));
    await manager.spawn();
})();