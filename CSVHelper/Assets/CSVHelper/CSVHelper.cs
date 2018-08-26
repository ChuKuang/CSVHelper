﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;


namespace WZFrame
{
    public class CSVHelper
    {
        public static Dictionary<int, T> GetDataTable<T>(string csvTableStr) where T : CSVDataBase, new()
        {
            string content = csvTableStr.Replace("\r", "");
            string[] lines = content.Split('\n');
            if (lines.Length < 3)
            {
                Debug.Log("the table is empty");
                return null;
            }

            Dictionary<int, T> dic = new Dictionary<int, T>();

            //先按key归类将值读取到集合中
            Dictionary<string,List<string>> strDic =  ReadToDic(lines);
         
            PropertyInfo[] pins = typeof(T).GetProperties();

            //从上面的集合中根据属性名取到对应值
            for (int i = 2; i < lines.Length; i++)
            {
                T data = new T();

                for (int j = 0; j < pins.Length; j++)
                {
                    if(strDic.ContainsKey(pins[j].Name))
                    {
                        Type type = pins[j].PropertyType;

                        if (type == typeof(int))
                        {
                            pins[j].SetValue(data, GetInt(strDic[pins[j].Name][i -2]), null);
                        }
                        if (type == typeof(float))
                        {
                            pins[j].SetValue(data, GetFloat(strDic[pins[j].Name][i - 2]), null);
                        }
                        if (type == typeof(bool))
                        {
                            pins[j].SetValue(data, GetBool(strDic[pins[j].Name][i - 2]), null);
                        }
                        else
                        {
                            pins[j].SetValue(data, GetString(strDic[pins[j].Name][i - 2]), null);
                        }
                    }
                }

                int major = GetInt(lines[i].Split(',')[0]);
                dic.Add(major, data);
            }

            return dic;
        }

        // 生成的第一行是类型（int， string等）
        //第二行属性名字，对应类属性名
        //第三行开始是属性值
        //将返回的字符串保存文本就是完整csv格式了
        public static string GetCSVContent<T>(Dictionary<int, T> dictionary) where T : CSVDataBase, new()
        {
            string content = string.Empty;
            PropertyInfo[] keyProper = typeof(T).GetProperties();

            foreach (PropertyInfo item in keyProper)
            {
                content += GetTypeName(item.PropertyType.Name) + ",";
            }
            content.Trim();
            content += "\n";

            foreach (PropertyInfo item in keyProper)
            {
                content += item.Name + ",";
            }
            content.Trim();

            foreach (T data in dictionary.Values)
            {
                content += "\n";

                //这路通过反射，获取所有值拼接成字符串
                foreach (PropertyInfo item in keyProper)
                {
                    System.Object value = item.GetValue(data, null);
                    if (data == null)
                        value = string.Empty;

                    content += (value.ToString() + ",").Trim();
                }

                content += content.Remove(content.Length - 1);
            }
            return content;
        }


        /// <summary>
        /// 根据key将对应的值读入到list中
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, List<string>> ReadToDic(string[] lines)
        {
            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
            string[] keys = lines[1].Split(',');
            for(int i = 2; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');
                for(int j = 0; j < values.Length; j++)
                {
                    if(dic.ContainsKey(keys[j]))
                    {
                        dic[keys[j]].Add(values[j]);
                    }
                    else
                    {
                        dic.Add(keys[j], new List<string>() { values[j] });
                    }
                }
            }

            return dic;
        }


        public static int GetInt(string dText)
        {
            if (string.IsNullOrEmpty(dText))
                return 0;
            else
                return (int)float.Parse(dText);
        }

        public static string GetString(string dText)
        {
            if (string.IsNullOrEmpty(dText))
                return string.Empty;
            else
                return dText;
        }

        public static float GetFloat(string dText)
        {
            if (string.IsNullOrEmpty(dText))
                return 0.0f;
            else
                return float.Parse(dText);
        }


        //这里我当表里的是"true", "false"
        //如果是用0 和1代替的需要更改
        public static bool GetBool(string dText)
        {
            if (string.IsNullOrEmpty(dText))
                return false;
            else
                return bool.Parse(dText.ToLower());
        }


        public static string GetTypeName(string type)
        {
            string name = string.Empty;
            switch (type)
            {
                case "Int32":
                    name = "int";
                    break;
                case "String":
                    name = "string";
                    break;
                case "Single":
                    name = "float";
                    break;
                case "Boolean":
                    name = "bool";
                    break;
                default:
                    name = type;
                    break;
            }

            return name;
        }
    }

}
