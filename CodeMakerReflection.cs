using CodeMaker;
using System.Collections.Generic;
using System.Reflection;

namespace CodeMacker
{
    public static class CodeMackerReflection
    {
        #region 反射获取字段
        /// <summary>
        /// 反射获取字段
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fieldName">字段名称</param>
        /// <param name="bindingAttr">字段公开属性</param>
        /// <returns></returns>
        public static FieldInfo GetField(this object obj, string fieldName, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            => obj.GetType().GetField(fieldName, bindingAttr);
        /// <summary>
        /// 反射获取字段值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fieldName">字段名称</param>
        /// <param name="bindingAttr">字段公开属性</param>
        /// <returns></returns>
        public static object GetFieldValue(this object obj, string fieldName, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            => GetField(obj, fieldName, bindingAttr).GetValue(obj);
        /// <summary>
        /// 反射获取字段值转换成指定类型
        /// </summary>
        /// <typeparam name="T">转换类型</typeparam>
        /// <param name="obj"></param>
        /// <param name="fieldName">字段名称</param>
        /// <param name="bindingAttr">字段公开属性</param>
        /// <returns></returns>
        public static T GetFieldAs<T>(this object obj, string fieldName, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic) where T : class
            => GetFieldValue(obj, fieldName, bindingAttr) as T;
        /// <summary>
        /// 反射设置字段值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fieldName">字段名称</param>
        /// <param name="bindingAttr">字段公开属性</param>
        /// <returns></returns>
        public static void SetFieldValue(this object obj, string fieldName, object value, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            => GetField(obj, fieldName, bindingAttr).SetValue(obj, value);

        /// <summary>
        /// 反射获取当前类所有字段
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="bindingAttr">取出类型</param>
        /// <returns></returns>
        public static FieldInfo[] GetFields(this object obj, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            => obj.GetType().GetFields(bindingAttr);
        #endregion


        #region 反射获取属性
        /// <summary>
        /// 反射获取属性
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="bindingAttr">公开属性类型</param>
        /// <returns></returns>
        public static PropertyInfo GetProperty(this object obj, string propertyName, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            => obj.GetType().GetProperty(propertyName, bindingAttr);
        /// <summary>
        /// 反射获取属性值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="bindingAttr">公开属性类型</param>
        /// <returns></returns>
        public static object GetPropertyValue(this object obj, string propertyName, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
           => GetProperty(obj, propertyName, bindingAttr).GetValue(obj);
        /// <summary>
        /// 反射获取属性值并转换指定类型
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="obj"></param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="bindingAttr">公开属性类型</param>
        /// <returns></returns>
        public static T GetPropertyInfoAs<T>(this object obj, string propertyName, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic) where T : class
           => GetPropertyValue(obj, propertyName, bindingAttr) as T;
        /// <summary>
        /// 反射设置属性值
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="fieldName">字段名称</param>
        /// <param name="bindingAttr">字段公开属性</param>
        /// <returns></returns>
        public static void SetPropertyValue(this object obj, string fieldName, object value, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
          => GetProperty(obj, fieldName, bindingAttr).SetValue(obj, value);

        /// <summary>
        /// 反射获取当前类所有属性
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="bindingAttr">取出类型</param>
        /// <returns></returns>
        public static PropertyInfo[] GetPropertys(this object obj, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            => obj.GetType().GetProperties(bindingAttr);
        #endregion


        #region 反射获取方法
        /// <summary>
        /// 反射获取方法
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="methodName">方法名称</param>
        /// <param name="bindingAttr">方法类型</param>
        /// <returns></returns>
        public static MethodInfo GetMethod(this object obj, string methodName, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            => obj.GetType().GetMethod(methodName, bindingAttr);
        /// <summary>
        /// 反射获取方法并传递参数执行
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="methodName">方法名称</param>
        /// <param name="parameter">方法参数</param>
        /// <param name="bindingAttr">方法类型</param>
        /// <returns></returns>
        public static object GetMethodAndExcute(this object obj, string methodName, object[] parameter, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            => GetMethod(obj, methodName, bindingAttr).Invoke(obj, parameter);

        /// <summary>
        /// 反射获取方法组
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="bindingAttr">方法类型</param>
        /// <returns></returns>
        public static MethodInfo[] GetMethods(this object obj, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
            => obj.GetType().GetMethods(bindingAttr);
        #endregion

        #region 反射获取除IgnoreAttribute外的内容
        public static T[] GetWithoutAttrOf<T>(this T[] contents, System.Type type, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic) where T : MemberInfo
        {
            List<T> propertyInfos = new List<T>();
            foreach (var item in contents)
            {
                if (item.GetCustomAttributes(type, false).Length == 0)
                {
                    propertyInfos.Add(item);
                }
            }
            return propertyInfos.ToArray();
        }
        #endregion
    }
}
