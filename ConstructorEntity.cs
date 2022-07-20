using System;
using System.CodeDom;
using System.Collections.Generic;

namespace CodeMacker
{
    public class ConstructorEntity
    {
        /// <summary>
        /// 参数级
        /// </summary>
        public List<ParamInfo> Params { get; set; }

        /// <summary>
        /// 构造函数类型
        /// </summary>
        public MemberAttributes Attr { get; set; } = MemberAttributes.Public | MemberAttributes.Final;
    }

    public class ParamInfo
    {
        public ParamInfo(Type paramType, string name)
        {
            ParamType = paramType;
            Name = name;
        }
        /// <summary>
        /// 类型
        /// </summary>
        public Type ParamType { get; private set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 赋值名称
        /// </summary>
        public string ReferenceName { get; private set; }
    }
}
