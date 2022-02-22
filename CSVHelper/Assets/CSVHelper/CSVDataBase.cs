using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CSV
{
    //该类是描述CSV表的Key的通用基础类
    public class CSVDataBase
    {
        //子类里面定义属性
        //public int Id { get; set; }



    }


    /// <summary>
    /// CSV属性的描述信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DescriptionAttribute: Attribute
    {
        public string description { protected set; get; }
        public DescriptionAttribute(string description)
        {
            this.description = description;
        }
    }


    /// <summary>
    /// 跳过标签
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SkipAttribute : Attribute
    {

    }

}

