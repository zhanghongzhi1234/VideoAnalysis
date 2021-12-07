/************************************************************************
 * This file is the Core singleton class, 
 * provide operation to mysql database
 * Read config file start.ini for db connection info
 * author: zhang hongzhi
*************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections;
using System.IO;

namespace Core
{
    public sealed class DAIHelper
    {
        public static log4net.ILog log;
        private static volatile DAIHelper instance;        //singleton
        private static object syncRoot = new Object();

        private static string server;           //数据库服务器名或ip
        private static string userid;           //用户名
        private static string password;         //密码
        private static string database;         //数据库名
        private static string connString;       //连接字符串
        private static MySqlConnection mySqlConn;       //数据库连接

        private DataTable dataTable;            //数据表
        private MySqlDataAdapter adapter;       //Mysql数据adapter
        public int test;
        public MySqlConnection MySqlConn
        {
            get { return mySqlConn; }
            set { mySqlConn = value; }
        }
        public MySqlDataAdapter Adapter
        {
            get { return adapter; }
            set { adapter = value; }
        }

        private DAIHelper() 
        {
            log = DebugUtil.Instance.LOG;
            try
            {
                ReadDBConnectionFile();
                mySqlConn = new MySqlConnection(connString);
                mySqlConn.Open();
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                //System.Environment.Exit(0);
            }
            //mySqlConn.ChangeDatabase("ems");
        }

        public static DAIHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new DAIHelper();
                    }
                }

                return instance;
            }
        }

        // Read start.ini config file to parse the db connection parameter
        private static bool ReadDBConnectionFile()
        {
            // 读取文件的源路径及其读取流
            string strReadFilePath = "DBConnection.ini";
            if (File.Exists(strReadFilePath))
            {
                StreamReader srReadFile = new StreamReader(strReadFilePath);

                // 开始解析文件
                try
                {
                    while (!srReadFile.EndOfStream)
                    {
                        string strReadLine = srReadFile.ReadLine();
                        string[] sArray = strReadLine.Split('=');
                        switch (sArray[0])
                        {
                            case "server":      //服务器名
                                server = sArray[1];
                                break;
                            case "userid":      //用户名
                                userid = sArray[1];
                                break;
                            case "password":    //密码
                                password = sArray[1];
                                break;
                            case "database":    //数据库名，默认为ems
                                database = sArray[1];
                                break;
                            default:
                                break;

                        }
                    }
                }
                catch (System.Exception e)
                {
                    log.Error(e.ToString());
                    return false;
                }
                // 关闭读取流文件
                srReadFile.Close();
            }
            else
            {
                return false;
            }
            //charset=utf8避免插入中文乱码，创建连接字符串
            connString = "server="+server+";user id="+userid+";password="+password+";database="+database+";charset=utf8";
            log.Info(connString);
            return true;
        }

        // Read start.ini config file to parse the db connection parameter
        public Dictionary<string, string> ReadConfigFile()
        {
            // 读取文件的源路径及其读取流
            string strReadFilePath = "start.ini";
            Dictionary<string, string> ret = new Dictionary<string, string>();
            if (File.Exists(strReadFilePath))
            {
                StreamReader srReadFile = new StreamReader(strReadFilePath);

                // 开始解析文件
                try
                {
                    while (!srReadFile.EndOfStream)
                    {
                        string strReadLine = srReadFile.ReadLine();
                        int index = strReadLine.IndexOf('=');
                        //string[] sArray = strReadLine.Split('=');
                        string name = strReadLine.Substring(0, index);  //sArray[0];
                        string value = strReadLine.Substring(index + 1, strReadLine.Length - index - 1); //sArray[1];
                        ret[name] = value;
                    }
                }
                catch (System.Exception e)
                {
                    log.Error(e.ToString());
                }
                // 关闭读取流文件
                srReadFile.Close();
            }
            return ret;
        }

        //得到所有Entity pkey
        public ArrayList GetAllEntityPkeys()
        {
            ArrayList items = new ArrayList();
            MySqlDataReader reader = null;
            string sqlstr = "select pkey from entity";
            log.Debug(sqlstr);
            MySqlCommand cmd = new MySqlCommand(sqlstr, mySqlConn);
            try
            {
                reader = cmd.ExecuteReader();
                items.Clear();
                while (reader.Read())
                {
                    items.Add(reader.GetString(0));
                }
            }
            catch (MySqlException ex)
            {
                log.Error(ex.ToString());
                return null;
            }
            finally
            {
                if (reader != null) reader.Close();
            }
            return items;
        }

        //得到所有Entity
        public Dictionary<int, Entity> GetAllEntities()
        {
            Dictionary<int, Entity> items = new Dictionary<int, Entity>();
            MySqlDataReader reader = null;
            string sqlstr = "select e.*, st.name stationname,sub.name subsystemname from entity e, station st, subsystem sub where e.stationkey=st.pkey and e.subsystemkey=sub.pkey order by e.pkey asc";
            log.Debug(sqlstr);
            MySqlCommand cmd = new MySqlCommand(sqlstr, mySqlConn);
            try
            {
                reader = cmd.ExecuteReader();
                items.Clear();
                while (reader.Read())
                {
                    Entity item = new Entity(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetString(4), reader.GetString(5));
                    items[item.pkey] = item;
                }
            }
            catch (MySqlException ex)
            {
                log.Error(ex.ToString());
                return null;
            }
            finally
            {
                if (reader != null) reader.Close();
            }
            return items;
        }

        //得到所有Template
        public Dictionary<int, Template> GetAllTemplates()
        {
            Dictionary<int, Template> items = new Dictionary<int, Template>();
            MySqlDataReader reader = null;
            string sqlstr = "select * from template order by pkey asc";
            log.Debug(sqlstr);
            MySqlCommand cmd = new MySqlCommand(sqlstr, mySqlConn);
            try
            {
                reader = cmd.ExecuteReader();
                items.Clear();
                while (reader.Read())
                {
                    Template item = new Template(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
                    items[item.pkey] = item;
                }
            }
            catch (MySqlException ex)
            {
                log.Error(ex.ToString());
                return null;
            }
            finally
            {
                if (reader != null) reader.Close();
            }
            return items;
        }

        //得到所有Station
        public Dictionary<int, string> GetAllStations()
        {
            Dictionary<int, string> items = new Dictionary<int, string>();
            MySqlDataReader reader = null;
            string sqlstr = "select * from station";
            log.Debug(sqlstr);
            MySqlCommand cmd = new MySqlCommand(sqlstr, mySqlConn);
            try
            {
                reader = cmd.ExecuteReader();
                items.Clear();
                while (reader.Read())
                {
                    items[reader.GetInt32(0)] = reader.GetString(1);
                }
            }
            catch (MySqlException ex)
            {
                log.Error(ex.ToString());
                return null;
            }
            finally
            {
                if (reader != null) reader.Close();
            }
            return items;
        }

        //得到所有Subsystem
        public Dictionary<int, string> GetAllSubsystems()
        {
            Dictionary<int, string> items = new Dictionary<int, string>();
            MySqlDataReader reader = null;
            string sqlstr = "select * from subsystem";
            log.Debug(sqlstr);
            MySqlCommand cmd = new MySqlCommand(sqlstr, mySqlConn);
            try
            {
                reader = cmd.ExecuteReader();
                items.Clear();
                while (reader.Read())
                {
                    items[reader.GetInt32(0)] = reader.GetString(1);
                }
            }
            catch (MySqlException ex)
            {
                log.Error(ex.ToString());
                return null;
            }
            finally
            {
                if (reader != null) reader.Close();
            }
            return items;
        }

        //根据名字查找配置项
        public string GetConfigValueByName(string name)
        {
            string sqlstr = "select value from config where name='" + name + "'";
            log.Debug(sqlstr);
            MySqlCommand cmd = new MySqlCommand(sqlstr, mySqlConn);
            string ret = "";
            try
            {
                object val = cmd.ExecuteScalar();
                ret = Convert.ToString(val);
            }
            catch (MySqlException ex)
            {
                log.Error(ex.ToString());
                ret = "";
            }

            return ret;
        }

        //得到所有Config
        public Dictionary<int, Config> GetAllConfig()
        {
            Dictionary<int, Config> items = new Dictionary<int, Config>();
            MySqlDataReader reader = null;
            string sqlstr = "select * from config order by pkey asc";
            log.Debug(sqlstr);
            MySqlCommand cmd = new MySqlCommand(sqlstr, mySqlConn);
            try
            {
                reader = cmd.ExecuteReader();
                items.Clear();
                while (reader.Read())
                {
                    Config item = new Config(reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
                    items[item.pkey] = item;
                }
            }
            catch (MySqlException ex)
            {
                log.Error(ex.ToString());
                return null;
            }
            finally
            {
                if (reader != null) reader.Close();
            }
            return items;
        }

        //得到所有Entity
        public List<AlarmNumber> GetAllAlarmNumbers()
        {
            List<AlarmNumber> items = new List<AlarmNumber>();
            MySqlDataReader reader = null;
            string sqlstr = "select e.*, st.name stationname from alarm_number e, station st where e.stationkey=st.pkey order by e.pkey asc";
            log.Debug(sqlstr);
            MySqlCommand cmd = new MySqlCommand(sqlstr, mySqlConn);
            try
            {
                reader = cmd.ExecuteReader();
                items.Clear();
                while (reader.Read())
                {
                    AlarmNumber item = new AlarmNumber(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetString(4));
                    items.Add(item);
                }
            }
            catch (MySqlException ex)
            {
                log.Error(ex.ToString());
                return null;
            }
            finally
            {
                if (reader != null) reader.Close();
            }
            return items;
        }

        //得到所有PowerArchive
        public List<PowerArchive> GetAllPowerArchive()
        {
            List<PowerArchive> items = new List<PowerArchive>();
            MySqlDataReader reader = null;
            string sqlstr = "select * from power_archive";
            log.Debug(sqlstr);
            MySqlCommand cmd = new MySqlCommand(sqlstr, mySqlConn);
            try
            {
                reader = cmd.ExecuteReader();
                items.Clear();
                while (reader.Read())
                {
                    PowerArchive item = new PowerArchive(reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4), reader.GetInt32(5));
                    items.Add(item);
                }
            }
            catch (MySqlException ex)
            {
                log.Error(ex.ToString());
                return null;
            }
            finally
            {
                if (reader != null) reader.Close();
            }
            return items;
        }

        //根据输入的sql语句，返回数据表
        public DataTable GetDataTable(string sqlstr)
        {
            log.Debug(sqlstr);
            adapter = new MySqlDataAdapter(sqlstr, Core.DAIHelper.Instance.MySqlConn);
            dataTable = new DataTable();
            adapter.Fill(dataTable);
            return dataTable;
        }

        //保存修改过的数据表
        public bool SaveChanges(DataTable changes)
        {
            try
            {
                adapter.Update(changes);
            }
            catch (DBConcurrencyException ex)
            {
                log.Error(ex.ToString());
                return false;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToString());
                return false;
            }
            return true; ;
        }

        //执行sql语句，修改数据库, 为Insert,Update,Delete中的一种
        public int ExecuteNonQuery(string sqlstr)
        {
            log.Debug(sqlstr);
            MySqlCommand mySqlCmd = new MySqlCommand(sqlstr, mySqlConn);
            return mySqlCmd.ExecuteNonQuery();
        }

        //得到最后插入的记录主键
        public int getLastId(string tablename, string IDColumnName = "id")
        {
            string sqlstr = "select " + IDColumnName + " from " + tablename + " order by " + IDColumnName + " desc limit 0,1";
            log.Debug(sqlstr);
            MySqlCommand cmd = new MySqlCommand(sqlstr, mySqlConn);
            int nRet = 0;
            try
            {
                object val = cmd.ExecuteScalar();
                nRet = Convert.ToInt32(val);
            }
            catch (MySqlException ex)
            {
                log.Error(ex.ToString());
                return 0;
            }

            return nRet;
        }

    }
}
