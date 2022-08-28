import {SlashCommandBuilder} from "discord.js";

export default {
    data: new SlashCommandBuilder()
        .setName("leaderboard")
        .setDescription("Leaderboard of worships by user"),

    async execute(interaction, client){
        client.db.all("SELECT UserID, COUNT(UserID) FROM Worshipers GROUP BY UserID ORDER BY COUNT(UserID) DESC", (err, rows) => {
            const rowsCopy = rows;
            const worshipersArray = [];
            for(let i = 0; i <= 5; i++){
                const filter = rows.filter(x => x.UserID === rows[0].UserID);
                worshipersArray.push({UserID: filter[0].UserID, Count: filter.length});
                rows = rows.filter(x => x.UserID !== rows[0].UserID);
            }
            console.log(worshipersArray)
        })
    },

    disabled: true
}