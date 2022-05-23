module.exports = () => {
    require('colors')
    const moment = require('moment')
    const fs = require("node:fs");

    if(!fs.existsSync('./logs/logs.txt')){
        fs.mkdirSync('./logs')
        fs.writeFileSync('./logs/logs.txt', '')
    }

    const log_file = fs.createWriteStream('./logs/logs.txt', {flags : 'w'});
    const log_stdout = process.stdout;

    console = {
        log(d){
            log_file.write(`[${moment().format('h:mm:ss')}] [log] - ${d}\n`);
            log_stdout.write(`[${moment().format('h:mm:ss')}] [log] - `.green + `${d}\n`);
        },
        trace(d){
            log_file.write(`[${moment().format('h:mm:ss')}] [trace] - ${d}\n`);
            log_stdout.write(`[${moment().format('h:mm:ss')}] [trace] - `.magenta + `${d}\n`);
        },
        debug(d){
            log_file.write(`[${moment().format('h:mm:ss')}] [debug] - ${d}\n`);
            log_stdout.write(`[${moment().format('h:mm:ss')}] [debug] - ` + `${d}\n`);
        },
        info(d){
            log_file.write(`[${moment().format('h:mm:ss')}] [info] - ${d}\n`);
            log_stdout.write(`[${moment().format('h:mm:ss')}] [info] - `.cyan + `${d}\n`);
        },
        error(d){
            log_file.write(`[${moment().format('h:mm:ss')}] [error] - ${d}\n`);
            log_stdout.write(`[${moment().format('h:mm:ss')}] [error] - `.red + `${d}\n`);
        },
        warn(d){
            log_file.write(`[${moment().format('h:mm:ss')}] [warning] - ${d}\n`);
            log_stdout.write(`[${moment().format('h:mm:ss')}] [warning] - `.yellow + `${d}\n`);
        },
        normalLog(d){
            log_file.write(`${d}\n`)
            log_stdout.write(`${d}\n`)
        }
    }
}