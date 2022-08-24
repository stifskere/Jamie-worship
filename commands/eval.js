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

        try{
            if(expression.includes('process.env')) throw new Error('Environment variables are protected, no interaction with them is allowed trough eval.');
            
            let cleaned = await client.cleanEval(eval(expression));

            if(cleaned.length > 1020) cleaned = cleaned.substring(0, 1000) + '...';

            const evalEmbed = new MessageEmbed()
                .setTitle('EVAL')
                .setColor('#2bde00')
                .addFields(
                    {"name": 'Expression', "value": `\`\`\`js\n${expression}\n\`\`\``},
                    {"name": 'Result', "value": `\`\`\`js\n${cleaned}\n\`\`\``}
                )
                .setTimestamp()

            await interaction.editReply({embeds: [evalEmbed]});
        }catch(error){
            const evalEmbed = new MessageEmbed()
                .setTitle('ERROR')
                .setColor('#ff0000')
                .addFields(
                    {"name": 'Expression', "value": `\`\`\`js\n${expression}\n\`\`\``},
                    {"name": 'Result', "value": `\`\`\`xl\n${error}\n\`\`\``}
                )
                .setTimestamp()

            await interaction.editReply({embeds: [evalEmbed]});
        }
    }
}