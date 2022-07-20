using System;
using System.CodeDom;

namespace CodeMacker
{
    public class FieldEntity
    {
        public FieldEntity(string fieldName,Type fieldType)
        {
            this.Name = fieldName;
            this.Type = fieldType;
        }
        /// <summary>
        /// 字段名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 字段类型
        /// </summary>
        public Type Type { get; private set; }
        /// <summary>
        /// 默认初始参数
        /// </summary>
        public object DefaultValue { get; set; } = default;
        /// <summary>
        /// 字段注释
        /// </summary>
        public string Comment { get; set; } = null;
        /// <summary>
        /// 字段类型，默认公开
        /// </summary>
        public MemberAttributes Attr { get; set; } = MemberAttributes.Public | MemberAttributes.Final;

    }
}
