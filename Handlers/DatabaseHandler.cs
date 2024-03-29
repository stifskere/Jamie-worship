﻿using System.Data;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace JamieWorshipper.Handlers;

public class DatabaseHandler
{
    private readonly SQLiteConnection _con;
    
    public DatabaseHandler(string dbPath)
    {
        string folderPath = string.Join('/', Regex.Split(dbPath, @"[/\\]", RegexOptions.Multiline)[..^1]);
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(string.Join('/', folderPath));
        if(!File.Exists(dbPath)) SQLiteConnection.CreateFile(dbPath);
        _con = new SQLiteConnection($"URI=file:{dbPath}");
        _con.Open();
    }
    
    ~DatabaseHandler() => _con.Dispose();

    public List<List<object>> RunSqliteCommandAllRows(string command, params string[] parameters)
    {
        SQLiteCommand cmd = new SQLiteCommand(command, _con);
        for (int i = 0; i < parameters.Length; i++)
        {
            cmd.Parameters.Add($"@{i}", DbType.String);
            cmd.Parameters[$"@{i}"].Value = parameters[i];
        }
        List<List<object>> returnList = new();
        SQLiteDataReader reader = cmd.ExecuteReader();
        if(reader.HasRows) while (reader.Read())
        {
            List<object> insertList = new List<object>(reader.FieldCount);
            for(int i = 0; i < reader.FieldCount; i++) insertList.Add(reader[i]);
            returnList.Add(insertList);
        }
        return returnList;
    }

    public List<object> RunSqliteCommandFirstRow(string command, params string[] parameters) {
        List<List<object>> data = RunSqliteCommandAllRows(command, parameters);
        return data.Count == 0 ? new List<object>() : data[0];
    }
}