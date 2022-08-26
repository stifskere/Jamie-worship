export default async (text) => {
    if(text && text.constructor.name === "Promise") text = await text
    if(typeof  text !== "string") text = (await import("util")).inspect(text, {depth: 1});
    return text.replace(/`/g, "`" + String.fromCharCode(8203)).replace(/@/g, "@" + String.fromCharCode(8203));
}