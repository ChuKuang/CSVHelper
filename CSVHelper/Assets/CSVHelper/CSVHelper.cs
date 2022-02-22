using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.IO;

namespace CSV
{
    public class CSVHelper
    {

        //最小行数
        //第一行备注
        //第二行类型
        //第三行属性名
        public const int Minlines = 3;

        //中文备注index 从0开始也就是第1行
        public const int PropertyDesIndex = 0;

        //类型index 从0开始也就是第2行
        public const int PropertyTypeIndex = 1;

        //属性key index 从0开始也就是第3行
        public const int PropertyKeyIndex = 2;

        //表数据读取index   从0 开始 也就是第4行
        public const int PropertyValueIndex = 3;

 



        public static Dictionary<int, T> ToTableObject<T>(string csvTableStr) where T : CSVDataBase, new()
        {
            Dictionary<int, T> dic = new Dictionary<int, T>();
            string content = csvTableStr.Replace("\r", "");
            string[] lines = content.Split('\n');
            if (lines.Length <= Minlines)
            {
                Debug.Log("the table is empty");
                return dic;
            }

            //先按key归类将值读取到集合中
            Dictionary<string,List<string>> strDic =  ReadToDic(lines);
         
            PropertyInfo[] pins = typeof(T).GetProperties();

            //从上面的集合中根据属性名取到对应值
            for (int i = PropertyValueIndex; i < lines.Length; i++)
            {
                T data = new T();

                int readDicListIndex = i - PropertyValueIndex;

                for (int j = 0; j < pins.Length; j++)
                {
                    if(strDic.ContainsKey(pins[j].Name))
                    {
                        Type type = pins[j].PropertyType;

                        if (type == typeof(int))
                        {
                            pins[j].SetValue(data, GetInt(strDic[pins[j].Name][readDicListIndex]), null);
                        }
                        else if (type == typeof(float))
                        {
                            pins[j].SetValue(data, GetFloat(strDic[pins[j].Name][readDicListIndex]), null);
                        }
                        else if(type == typeof(bool))
                        {
                            pins[j].SetValue(data, GetBool(strDic[pins[j].Name][readDicListIndex]), null);
                        }
                        else if(type == typeof(string[]))
                        {
                            pins[j].SetValue(data, GetStringArray(strDic[pins[j].Name][readDicListIndex]), null);
                        }
                        else if (type == typeof(int[]))
                        {
                            pins[j].SetValue(data, GetIntArray(strDic[pins[j].Name][readDicListIndex]), null);
                        }
                        else
                        {
                            pins[j].SetValue(data, GetString(strDic[pins[j].Name][readDicListIndex]), null);
                        }
                    }
                }

                int major = GetInt(lines[i].Split(',')[0]);
                dic.Add(major, data);
            }

            return dic;
        }


        public static string ToCSV<T>(Dictionary<int, T> tableData, string sourcesTablePath) where T : CSVDataBase, new()
        {
            string content = string.Empty;
            PropertyInfo[] keyProper = typeof(T).GetProperties();

            //拼接第一行备注
            Dictionary<string, string> noteDic = GetTableNote(sourcesTablePath);
            foreach (var item in keyProper)
            {
                if(noteDic.ContainsKey(item.Name))
                {
                    content += noteDic[item.Name] + ",";
                }
                else
                {
                    if(item.IsDefined(typeof(SkipAttribute)))
                    {
                        content += "skiprow,";
                    }
                    else if (item.IsDefined(typeof(DescriptionAttribute)))
                    {
                        content += item.GetCustomAttribute<DescriptionAttribute>().description;
                        content += ",";
                    }
                    else
                    {
                        content += " ,";
                    }
                }
            }

            content = content.Remove(content.Length - 1);
            content += "\n";

            //拼接第二行属性类型
            foreach (var item in keyProper)
            {
                if(item.IsDefined(typeof(SkipAttribute)))
                    content += "skiprow,";
                else
                    content += GetTypeName(item.PropertyType.Name) + ",";
            }

            content = content.Remove(content.Length - 1);
            content += "\n";

            //拼接第三行属性名
            foreach (var item in keyProper)
            {
                content += item.Name + ",";
            }

            content = content.Remove(content.Length - 1);
    
            content += ToCSV<T>(tableData);

            return content;
        }



       
        private static string ToCSV<T>(Dictionary<int, T> dictionary) where T : CSVDataBase, new()
        {
            string content = string.Empty;
            PropertyInfo[] keyProper = typeof(T).GetProperties();


            foreach (T data in dictionary.Values)
            {
                content += "\n";
                //这路通过反射，获取所有值拼接成字符串
                foreach (PropertyInfo item in keyProper)
                {
                    System.Object value = item.GetValue(data, null);
                    if (value == null)
                        value = string.Empty;
                    else if (item.PropertyType == typeof(string[]))
                        value = SetArrayToString(value);
                    else if (item.PropertyType == typeof(int[]))
                        value = SetArrayToInt(value);
                    else if (item.PropertyType == typeof(bool))
                        value = SetBoolToInt(value);
                    content += (value.ToString() + ",").Trim();
                }

                content = content.Remove(content.Length - 1);
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
            string[] keys = lines[PropertyKeyIndex].Split(',');
            for(int i = PropertyValueIndex; i < lines.Length; i++)
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


        public static Dictionary<string, string> GetTableNote(string tableSourcePath)
        {
            Dictionary<string, string> noteDic = new Dictionary<string, string>();

            if(File.Exists(tableSourcePath))
            {
                StreamReader sr = File.OpenText(tableSourcePath);
                string sourceContent = sr.ReadToEnd();
                sr.Close();
                sr.Dispose();
                sourceContent = sourceContent.Replace("\r", "");
                string[] lines = sourceContent.Split('\n');
                if(lines.Length > 3)
                {
                    string[] keyName = lines[PropertyKeyIndex].Split(',');
                    string[] note = lines[PropertyDesIndex].Split(',');
                    for (int i = 0; i < keyName.Length; i++)
                    {
                        noteDic.Add(keyName[i], note[i]);
                    }
                }
            }

            return noteDic;
        }

        /// <summary>
        /// 反序列化int
        /// </summary>
        /// <param name="dText"></param>
        /// <returns></returns>
        public static int GetInt(string dText)
        {
            if (string.IsNullOrEmpty(dText))
                return 0;
            else
                return (int)float.Parse(dText);
        }


        /// <summary>
        /// 反序列化string
        /// </summary>
        /// <param name="dText"></param>
        /// <returns></returns>
        public static string GetString(string dText)
        {
            if (string.IsNullOrEmpty(dText))
                return string.Empty;
            else
                return dText;
        }

        /// <summary>
        /// 反序列化float
        /// </summary>
        /// <param name="dText"></param>
        /// <returns></returns>
        public static float GetFloat(string dText)
        {
            if (string.IsNullOrEmpty(dText))
                return 0.0f;
            else
                return float.Parse(dText);
        }

        /// <summary>
        /// 反序列化bool
        /// 兼容字符串true false 和 0 ， 1
        /// </summary>
        /// <param name="dText"></param>
        /// <returns></returns>
        public static bool GetBool(string dText)
        {
            if (string.IsNullOrEmpty(dText))
                return false;
            else
            {
                bool result;    
                if(bool.TryParse(dText.ToLower(), out result))
                {
                    return result;
                }
                else
                {
                    return GetInt(dText) > 0;
                }
            }
        }


        /// <summary>
        /// 反序列化int[]
        /// </summary>
        /// <param name="dText"></param>
        /// <returns></returns>
        public static int[] GetIntArray(string dText)
        {
            string[] data;
            if (string.IsNullOrEmpty(dText))
                data = new string[] { };
            else
                data = dText.Split(';');

            int[] intArray = new int[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                intArray[i] = int.Parse(data[i]);
            }
            return intArray;
        }


        /// <summary>
        /// 反序列化string[]
        /// </summary>
        /// <param name="dText"></param>
        /// <returns></returns>
        public static string[] GetStringArray(string dText)
        {
            string[] data;
            if (string.IsNullOrEmpty(dText))
                data = new string[] { };
            else
                data = dText.Split(';');

            string[] attrsArray = new string[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                if (!string.IsNullOrEmpty(data[i]))
                    attrsArray[i] = data[i];
                else
                    attrsArray[i] = string.Empty;
            }

            return attrsArray;
        }



        /// <summary>
        /// 序列化位表里的string[],加#
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string SetArrayToString(object array)
        {
            string[] stringArray = (string[])array;
            string stringValue = string.Empty;
            if(stringArray != null &&  stringArray.Length > 0)
            {
                foreach(var item in stringArray)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;
                    stringValue += item + ";";
                }
                stringValue = stringValue.Remove(stringValue.Length - 1);
            }

            return stringValue;
        }

        /// <summary>
        /// 序列化位表里的Int[],加#
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string SetArrayToInt(object array)
        {
            int[] intArray = (int[])array;
            string stringValue = string.Empty;
            if(intArray != null && intArray.Length > 0)
            {
                foreach (var item in intArray)
                {
                    stringValue += item + ";";
                }
                stringValue = stringValue.Remove(stringValue.Length - 1);
            }
            return stringValue;
        }


        /// <summary>
        /// 序列化bool类型为0和1
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string SetBoolToInt(object data)
        {
            string stringValue = "0";
            bool value = (bool)data;
            if (value)
                stringValue = "1";
            return stringValue;

        }

        public static string GetTypeName(string type)
        {
            string name = string.Empty;
            switch (type)
            {
                case "Int32":
                    name = "Int";
                    break;
                case "Int32[]":
                    name = "List(Int)";
                    break;
                case "String":
                    name = "Str";
                    break;
                case "String[]":
                    name = "List(Str)";
                    break;
                case "Single":
                    name = "Float";
                    break;
                case "Boolean":
                    name = "Bool";
                    break;
                default:
                    name = type;
                    break;
                
            }

            return name;
        }
    }

}
