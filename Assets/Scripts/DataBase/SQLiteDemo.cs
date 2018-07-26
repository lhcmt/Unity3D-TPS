using UnityEngine;
using System.Collections;
using System.IO;
using Mono.Data.Sqlite;

public class SQLiteDemo : MonoBehaviour
{
    /// <summary>
    /// SQLite数据库辅助类
    /// </summary>
    private SQLiteHelper sql;

    void Start()
    {
        //创建名为sqlite4unity的数据库
        sql = new SQLiteHelper("data source = sqlite3unity.db");
        
        //创建名为table1的数据表
        sql.CreateTable("table1", new string[] { "ID", "Name", "Age", "Email" }, new string[] { "INTEGER", "TEXT", "INTEGER", "TEXT" });
        sql.InsertValues("table1", new string[] { "'1'", "'张三'", "'22'", "'Zhang3@163.com'" });
        sql.InsertValues("table1", new string[] { "'2'", "'李四'", "'25'", "'Li4@163.com'" });

        //更新数据，将Name="张三"的记录中的Name改为"Zhang3"
        sql.UpdateValues("table1", new string[] { "Name" }, new string[] { "'Zhang3'" }, "Name", "=", "'张三'");

        //插入3条数据
        sql.InsertValues("table1", new string[] { "3", "'王五'", "25", "'Wang5@163.com'" });
        sql.InsertValues("table1", new string[] { "4", "'王五'", "26", "'Wang5@163.com'" });
        sql.InsertValues("table1", new string[] { "5", "'王五'", "27", "'Wang5@163.com'" });

        //删除Name="王五"且Age=26的记录,DeleteValuesOR方法类似
        sql.DeleteValuesAND("table1", new string[] { "Name", "Age" }, new string[] { "=", "=" }, new string[] { "'王五'", "'26'" });

        //读取整张表
        SqliteDataReader reader = sql.ReadFullTable("table1");
        while (reader.Read())
        {
            //读取ID
            Debug.Log(reader.GetInt32(reader.GetOrdinal("ID")));
            //读取Name
            Debug.Log(reader.GetString(reader.GetOrdinal("Name")));
            //读取Age
            Debug.Log(reader.GetInt32(reader.GetOrdinal("Age")));
            //读取Email
            Debug.Log(reader.GetString(reader.GetOrdinal("Email")));
        }

        //读取数据表中Age>=25的所有记录的ID和Name
        reader = sql.ReadTable("table1", new string[] { "ID", "Name" }, new string[] { "Age" }, new string[] { ">=" }, new string[] { "'25'" });
        while (reader.Read())
        {
            //读取ID
            Debug.Log(reader.GetInt32(reader.GetOrdinal("ID")));
            //读取Name
            Debug.Log(reader.GetString(reader.GetOrdinal("Name")));
        }

        //自定义SQL,删除数据表中所有Name="王五"的记录
        sql.ExecuteQuery("DELETE FROM table1 WHERE NAME='王五'");

        //关闭数据库连接
        sql.CloseConnection();
    }
}