const {SlashCommandBuilder} = require("@discordjs/builders");
const {MessageEmbed} = require("discord.js");
require('dotenv').config()
module.exports = {
    data: new SlashCommandBuilder()
        .setName('eval')
        .setDescription('Evaluates a javascript expression')
        .addStringOption(expression => expression
            .setName('expression')
            .setDescription('The evaluated expression')
            .setRequired(true)),
    async execute(interaction, client){
        await interaction.deferReply();

        const expression = interaction.options.getString('expression')

        const evalers = ['189495219383697409', '463986224101588992']

        if(!evalers.includes(interaction.user.id)){
            const errEmbed = new MessageEmbed()
                .setTitle('ERROR')
                .setDescription('You do not have permission to use this command')

            interaction.editReply({embeds: [errEmbed]})
            return;
        }

        async function clean (text) {
            if(text && text.constructor.name === "Promise") text = await text
            if(typeof  text !== "string") text = require("util").inspect(text, {depth: 1});

            text = text.replace(/`/g, "`" + String.fromCharCode(8203)).replace(/@/g, "@" + String.fromCharCode(8203));

            return text;
        }

        try{
            let cleaned = await clean(eval(expression));

            if(cleaned.length > 1020) cleaned = cleaned.substring(0, 1000) + '...';

            if(cleaned.includes(process.env.TOKEN)) cleaned = 'bingus won\'t see the token';

            const evalEmbed = new MessageEmbed()
                .setTitle('EVAL')
                .setColor('#2bde00')
                .addField('Expression', `\`\`\`js\n${expression}\n\`\`\``)
                .addField('Result', `\`\`\`js\n${cleaned}\n\`\`\``)
                .setTimestamp()

            await interaction.editReply({embeds: [evalEmbed]});
        }catch(error){
            const evalEmbed = new MessageEmbed()
                .setTitle('ERROR')
                .setColor('#ff0000')
                .addField('Expression', `\`\`\`js\n${expression}\n\`\`\``)
                .addField('Result', `\`\`\`xl\n${error}\n\`\`\``)
                .setTimestamp()

            await interaction.editReply({embeds: [evalEmbed]});
        }
    }
}