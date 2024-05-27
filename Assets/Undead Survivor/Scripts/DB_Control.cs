using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using MySql.Data.MySqlClient;
using UnityEditor.MemoryProfiler;

public class DB_Control : MonoBehaviour
{
    public static MySqlConnection SqlConn;

    static string ipAddress = "127.0.0.1";
    static string db_id = "root";
    static string db_pw = "5213";
    static string db_name = "undead";

    string strConn = string.Format("Server={0};Port=3306;Database={1};Uid={2};Pwd={3};", ipAddress, db_name, db_id, db_pw);
    //string strConn = string.Format("Server=localhost;Database=game;Uid=root;Pwd=5213;charset=utf8;SslMode=None;");

    private void Awake()
    {
        try
        {
            SqlConn = new MySqlConnection(strConn);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    void Start()
    {
        //string query = "select * from player";
        //DataSet ds = OnSelectRequest(query, "player");

        //Debug.Log(ds.GetXml());
    }

    // 중복 체크용
    public static bool OnCheckDuplicate(string playerName)
    {
        try
        {
            SqlConn.Open();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = SqlConn;
            cmd.CommandText = string.Format("SELECT * FROM `undead`.`player` where nickname = '{0}';", playerName);

            MySqlDataReader reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                int id = (int)reader["id"];
                string nickname = reader["nickname"].ToString();
                SqlConn.Close();
                return true;
            }
            else
            {
                SqlConn.Close();
                return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e.ToString());
            return false;
        }
    }

    public static void OnInsertNewPlayer(string playerName)
    {
        try
        {
            SqlConn.Open();

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = SqlConn;
            cmd.CommandText = string.Format("insert into player(`nickname`) values('{0}');", playerName);

            cmd.ExecuteNonQuery();
            SqlConn.Close();
        }
        catch (System.Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    //데이터 삽입,업데이트 쿼리시 사용 함수
    public static bool OnInsertOrUpdateRequest(string str_query)
    {
        try
        {
            MySqlCommand sqlCommand = new MySqlCommand();
            sqlCommand.Connection = SqlConn;
            sqlCommand.CommandText = str_query;

            SqlConn.Open();

            sqlCommand.ExecuteNonQuery();

            SqlConn.Close();

            return true;
        }
        catch (System.Exception e)
        {
            Debug.Log(e.ToString());
            return false;
        }
    }

    //select 조회 쿼리시 사용
    //2번째 파라미터 table_name은 Dataset 이름을 정의하기 위함
    public static DataSet OnSelectRequest(string p_query, string table_name)
    {
        try
        {
            SqlConn.Open();   //DB 연결

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = SqlConn;
            cmd.CommandText = p_query;

            MySqlDataAdapter sd = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            sd.Fill(ds, table_name);

            SqlConn.Close();  //DB 연결 해제

            return ds;
        }
        catch (System.Exception e)
        {
            Debug.Log(e.ToString());
            return null;
        }
    }

    private void OnApplicationQuit()
    {
        SqlConn.Close();
    }
}