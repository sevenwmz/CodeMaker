using System;
using System.CodeDom;

namespace CodeMacker
{
    public class PropertyEntity 
    {
        public PropertyEntity(string propertyName, Type propertyType)
        {
            this.Name = propertyName;
            this.Type = propertyType;
        }
        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// 属性类型
        /// </summary>
        public Type Type { get; private set; }
        /// <summary>
        /// 注释
        /// </summary>
        public virtual string Comment { get; set; } = null;
        /// <summary>
        /// 包含Set
        /// </summary>
        public bool HasSet { get; set; } = true;
        /// <summary>
        /// 包含Get
        /// </summary>
        public bool HasGet { get; set; } = true;
        /// <summary>
        /// 属性类型，默认公开
        /// </summary>
        public MemberAttributes Attr { get; set; } = MemberAttributes.Public | MemberAttributes.Final;
    }
}
