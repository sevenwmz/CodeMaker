using System;
using System.IO;
using System.Linq;
using System.CodeDom;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.CSharp;

namespace CodeMacker
{
    public class CodeMake
    {
        #region Field Area
        private CodeNamespace _samples;
        private CodeCompileUnit _targetUnit;
        private CodeTypeDeclaration _targetClass;
        private readonly string _outputFileName;
        private static IList<string> _assemblyUsingLocation = null;
        private event Action _assemblyLoad = null;
        /// <summary>
        /// 单例IOC容器
        /// </summary>
        private static Dictionary<string, object> _singletonContainer = null;
        private static readonly object _lock_obj = new object();

        /// <summary>
        /// 命名空间
        /// </summary>
        public string NameSpace { get; private set; }
        /// <summary>
        /// 类名称
        /// </summary>
        public string ClassName { get; private set; }
        /// <summary>
        /// 命名空间+类名称
        /// </summary>
        public string FullNameSpaceWithClass { get; private set; }

        #region CodeMaker Filter 节点
        private static bool _onecEventNotRun = true;
        /// <summary>
        /// 整个项目运行中只调用一次的事件，事件发生点在尚未构造对象之前
        /// </summary>
        public event Action DoOnceWorkBeforeConstructor = null;

        /// <summary>
        /// 开始构造函数之前
        /// </summary>
        public event Action BeforeConstructor = null;
        /// <summary>
        /// 结束构造函数时
        /// </summary>
        public event Action AfterConstructor = null;

        /// <summary>
        /// 添加命名空间之前（生成代码 AddNamespace）
        /// </summary>
        public event Action BeforeAddNamespace = null;
        /// <summary>
        /// 添加命名空间之后（生成代码 AddNamespace）
        /// </summary>
        public event Action AfterAddNamespace = null;

        /// <summary>
        /// 添加构造函数之前（生成代码 AddConstructor）
        /// </summary>
        public event Action BeforeAddConstructor = null;
        /// <summary>
        /// 添加构造函数之后（生成代码 AddConstructor）
        /// </summary>
        public event Action AfterAddConstructor = null;

        /// <summary>
        /// 添加字段之前（生成代码 AddField）
        /// </summary>
        public event Action BeforeAddField = null;
        /// <summary>
        /// 添加字段之后（生成代码 AddField）
        /// </summary>
        public event Action AfterAddField = null;

        /// <summary>
        /// 添加属性之前（生成代码 AddPropertie）
        /// </summary>
        public event Action BeforeAddPropertie = null;
        /// <summary>
        /// 添加属性之后（生成代码 AddPropertie）
        /// </summary>
        public event Action AfterAddPropertie = null;

        /// <summary>
        /// 添加方法之前（生成代码 AddMethod）
        /// </summary>
        public event Action BeforeAddMethod = null;
        /// <summary>
        /// 添加方法之后（生成代码 AddMethod）
        /// </summary>
        public event Action AfterAddMethod = null;

        /// <summary>
        /// 创建对象之前（生成实例 CreateInstance）
        /// </summary>
        public event Action BeforeCreateInstance = null;
        /// <summary>
        /// 创建对象之后（生成实例 CreateInstance）
        /// </summary>
        public event Action AfterCreateInstance = null;

        #endregion

        #endregion

        #region Ctor
        static CodeMake()
        {
            if (_singletonContainer is null)
            {
                lock (_lock_obj)
                {
                    if (_singletonContainer is null)
                    {
                        _singletonContainer = new Dictionary<string, object>();
                    }
                }
            }
            if (_assemblyUsingLocation is null)
            {
                lock (_lock_obj)
                {
                    if (_assemblyUsingLocation is null)
                    {
                        _assemblyUsingLocation = new List<string>();
                    }
                }
            }
        }

        public CodeMake(bool reLoadAssembly = false, Action<CodeMake> eventCallBack = null)
            : this("CodeDOM", reLoadAssembly, eventCallBack)
        {
        }
        public CodeMake(string nameSpace, bool reLoadAssembly = false, Action<CodeMake> eventCallBack = null)
            : this(nameSpace, "System", reLoadAssembly, eventCallBack)
        {
        }
        public CodeMake(string nameSpace, string usingNameSpace, bool reLoadAssembly = false, Action<CodeMake> eventCallBack = null)
            : this(nameSpace, usingNameSpace, "CreatedClass", reLoadAssembly, eventCallBack)
        {

        }

        public CodeMake(string nameSpace, string usingNameSpace, string className, bool reLoadAssembly = false, Action<CodeMake> eventCallBack = null)
            : this(nameSpace, usingNameSpace, className, TypeAttributes.Public, reLoadAssembly, eventCallBack)
        {

        }
        public CodeMake(string nameSpace, string usingNameSpace, string className, TypeAttributes visitAttr, bool reLoadAssembly = false, Action<CodeMake> eventCallBack = null)
            : this(nameSpace, usingNameSpace, className, visitAttr, "C:\\", reLoadAssembly, eventCallBack)
        {

        }
        public CodeMake(string nameSpace, string usingNameSpace, string className, TypeAttributes visitAttr, string fileFullPath, bool reLoadAssembly = false, Action<CodeMake> eventCallBack = null)
        {
            #region Verify Area
            if (string.IsNullOrEmpty(nameSpace))
            {
                throw new ArgumentException("命名空间不能为空");
            }
            if (string.IsNullOrEmpty(className))
            {
                throw new ArgumentException("类名不能为空");
            }
            #endregion

            if (eventCallBack != null)
            {
                eventCallBack(this);
            }
            if (_onecEventNotRun)
            {
                if (DoOnceWorkBeforeConstructor != null)
                {
                    DoOnceWorkBeforeConstructor();
                    _onecEventNotRun = false;
                }
            }

            if (BeforeConstructor != null)
            {
                BeforeConstructor();
            }

            #region Main
            if (_assemblyUsingLocation.Count <= 0)
            {
                _assemblyLoad += () => LoadBasicAssembly();
            }
            if (reLoadAssembly)
            {
                _assemblyLoad += () => LoadBasicAssembly();
            }

            _targetUnit = new CodeCompileUnit();
            _samples = new CodeNamespace(nameSpace);
            _samples.Imports.Add(new CodeNamespaceImport(usingNameSpace));
            _targetClass = new CodeTypeDeclaration(className);
            _targetClass.IsClass = true;
            _targetClass.TypeAttributes = visitAttr;
            _samples.Types.Add(_targetClass);
            _targetUnit.Namespaces.Add(_samples);

            NameSpace = nameSpace;
            ClassName = className;
            FullNameSpaceWithClass = NameSpace + "." + ClassName;

            _outputFileName = fileFullPath;
            #endregion

            if (AfterConstructor != null)
            {
                AfterConstructor();
            }
        }

        #endregion

        #region AssemblyLoadLocation Function Area
        /// <summary>
        /// 基础程序集加载
        /// </summary>
        private void LoadBasicAssembly()
        {
            if (_assemblyUsingLocation.Count > 0)
            {
                _assemblyUsingLocation.Clear();
            }
            DirectoryInfo root1 = new DirectoryInfo(AppContext.BaseDirectory);
            foreach (FileInfo f in root1.GetFiles())
            {
                if (f.Name.Contains(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    AddedAssemblyBy(f.FullName);
                }
            }
            AddedAssemblyBy(typeof(System.Object).GetTypeInfo().Assembly.Location);
            AddedAssemblyBy(typeof(Console).GetTypeInfo().Assembly.Location);
            AddedAssemblyBy(Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll"));
        }

        /// <summary>
        /// 加载程序集
        /// </summary>
        /// <param name="location"></param>
        private void AddedAssemblyBy(string location)
        {
            if (!_assemblyUsingLocation.Any(s => s.Contains(location)))
            {
                _assemblyUsingLocation.Add(location);
            }
        }

        /// <summary>
        /// 自定义一个生成对象引用的程序集扩展
        /// </summary>
        /// <param name="assemblyLocations"></param>
        /// <returns></returns>
        public CodeMake AddAssembly(string assemblyLocation)
            => AddAssemblys(new List<string> { assemblyLocation });

        /// <summary>
        /// 自定义一组生成对象引用的程序集扩展
        /// </summary>
        /// <param name="assemblyLocations"></param>
        /// <returns></returns>
        public CodeMake AddAssemblys(List<string> assemblyLocations)
        {
            foreach (var location in assemblyLocations)
            {
                _assemblyLoad += () => AddedAssemblyBy(location);
            }
            return this;
        }
        #endregion

        #region NameSpaceAdded Function Area

        /// <summary>
        /// 新增命名空间
        /// </summary>
        /// <param name="codeNamespaceImport">命名空间文本，不需要添加using</param>
        public CodeMake AddNamespace(string codeNamespaceImport)
            => AddNamespaces(() => new List<CodeNamespaceImport> { new CodeNamespaceImport(codeNamespaceImport) });

        /// <summary>
        /// 新增多个命名空间
        /// </summary>
        /// <param name="codeNamespaceImport">命名空间文本，不需要添加using</param>
        public CodeMake AddNamespaces(List<string> codeNamespaceImport)
        {
            List<CodeNamespaceImport> codeNamespace = new List<CodeNamespaceImport>();
            codeNamespaceImport.ForEach(c => codeNamespace.Add(new CodeNamespaceImport(c)));
            return AddNamespaces(() => codeNamespace);
        }

        /// <summary>
        /// 新增命名空间 自定义
        /// 
        ///     Demo
        ///     var codeNamespace = new CodeNamespaceImport>("namespace");
        /// </summary>
        /// <param name="codeNamespaceImport"></param>
        public CodeMake AddNamespace(Func<CodeNamespaceImport> codeNamespaceImport)
            => AddNamespaces(() => new List<CodeNamespaceImport> { codeNamespaceImport() });

        /// <summary>
        /// 新增命名空间 自定义
        /// 
        ///     Demo
        ///     var codeNamespace = new List<CodeNamespaceImport>()
        ///     {
        ///         new CodeNamespaceImport("namespace")
        ///     };
        /// </summary>
        /// <param name="codeNamespaceImport"></param>
        public CodeMake AddNamespaces(Func<List<CodeNamespaceImport>> codeNamespaceImport)
        {
            if (BeforeAddNamespace != null)
            {
                BeforeAddNamespace();
            }
            codeNamespaceImport().ForEach(c => _samples.Imports.Add(c));
            if (AfterAddNamespace != null)
            {
                AfterAddNamespace();
            }
            return this;
        }
        #endregion

        #region Inherit Function Area

        /// <summary>
        /// 继承接口名称
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CodeMake AddInherit(string name)
        {
            _targetClass.BaseTypes.Add(name);
            return this;
        }

        /// <summary>
        /// 继承接口类型
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CodeMake AddInherit(Type name)
        {
            _targetClass.BaseTypes.Add(name);
            return this;
        }
        #endregion

        #region Constructor Function Area

        /// <summary>
        /// 添加构造函数
        /// </summary>
        /// <param name="ctor">ctor</param>
        public CodeMake AddConstructor(ConstructorEntity ctor)
         => AddConstructor(() =>
            {
                if (ctor is null)
                {
                    throw new ArgumentException("构造函数基本访问类型参数不能为空");
                }
                // Declare the constructor
                CodeConstructor constructor = new CodeConstructor();
                constructor.Attributes = ctor.Attr;
                if (ctor.Params != null)
                {
                    ctor.Params.ForEach(s =>
                    {
                        // Add parameters.
                        constructor.Parameters.Add(new CodeParameterDeclarationExpression(
                            s.ParamType, s.Name));

                        if (!string.IsNullOrEmpty(s.ReferenceName))
                        {
                            // Add field initialization logic
                            CodeFieldReferenceExpression reference =
                                new CodeFieldReferenceExpression(
                                new CodeThisReferenceExpression(), s.ReferenceName);

                            constructor.Statements.Add(new CodeAssignStatement(reference,
                                new CodeArgumentReferenceExpression(s.Name)));
                        }

                    });
                }
                return constructor;
            });

        /// <summary>
        /// 添加构造函数
        /// </summary>
        /// <param name="ctor">ctor</param>
        public CodeMake AddConstructor(Func<CodeConstructor> ctor)
        {
            if (BeforeAddConstructor != null)
            {
                BeforeAddConstructor();
            }

            _targetClass.Members.Add(ctor());

            if (AfterAddConstructor != null)
            {
                AfterAddConstructor();
            }
            return this;
        }

        #endregion

        #region Field Function Area

        /// <summary>
        /// 新增字段
        /// </summary>
        /// <param name="FieldEntity">字段Model</param>
        public CodeMake AddField(FieldEntity fieldModel)
            => AddField(() =>
            {
                if (fieldModel is null)
                {
                    throw new ArgumentException("字段参数信息不能为null");
                }
                return GetFieldBy(fieldModel);
            });

        /// <summary>
        /// 新增多个字段
        /// </summary>
        /// <param name="FieldEntity">字段Model</param>
        public CodeMake AddFields(List<FieldEntity> fields)
        {
            fields.ForEach(f => AddField(f));
            return this;
        }

        /// <summary>
        /// 新增字段
        /// </summary>
        /// <param name="attr">字段标签</param>
        /// <param name="fieldName">字段名称</param>
        /// <param name="fieldType">字段类型</param>
        /// <param name="comment">字段注释</param>
        public CodeMake AddField(string fieldName, Type fieldType, MemberAttributes attr = MemberAttributes.Public , object defaultValue = default, string comment = null)
            => AddField(
                new FieldEntity(fieldName, fieldType)
                {
                    Attr = attr,
                    Comment = comment,
                    DefaultValue = defaultValue,
                });

        /// <summary>
        /// 新增字段(自定义)
        /// 
        /// 示例：
        ///         CodeMemberField field = new CodeMemberField();
        ///         field.Attributes = attr;
        ///         field.Name = fieldName;
        ///         field.Type = new CodeTypeReference(fieldType);
        ///         if (!string.IsNullOrEmpty(comment))
        ///         {
        ///             field.Comments.Add(new CodeCommentStatement(comment));
        ///         }
        ///         return field;
        /// </summary>
        /// <param name="fieldMember">字段类型</param>
        public CodeMake AddField(Func<CodeMemberField> fieldMember)
            => AddFields(() => new List<CodeMemberField> { fieldMember() });

        /// <summary>
        /// 新增多个字段(自定义)
        /// 
        /// 
        ///         Demo：
        ///         List<CodeMemberField> fields = new List<CodeMemberField>();
        ///         CodeMemberField field = new CodeMemberField();
        ///         field.Attributes = attr;
        ///         field.Name = fieldName;
        ///         field.Type = new CodeTypeReference(fieldType);
        ///         if (!string.IsNullOrEmpty(comment))
        ///         {
        ///             field.Comments.Add(new CodeCommentStatement(comment));
        ///         }
        ///         fields.Add(field);
        ///         return fields;
        ///         
        /// </summary>
        /// <param name="fieldMember"></param>
        public CodeMake AddFields(Func<List<CodeMemberField>> fieldMember)
        {
            if (BeforeAddField != null)
            {
                BeforeAddField();
            }
            fieldMember().ForEach(f => _targetClass.Members.Add(f));
            if (AfterAddField != null)
            {
                AfterAddField();
            }
            return this;
        }

        private CodeMemberField GetFieldBy(FieldEntity fieldModel)
        {
            // Declare the Value field.
            CodeMemberField field = new CodeMemberField(new CodeTypeReference(fieldModel.Type), fieldModel.Name);
            field.Attributes = fieldModel.Attr;
            if (fieldModel.DefaultValue != null)
            {
                field.InitExpression = new CodePrimitiveExpression(fieldModel.DefaultValue);
            }
            if (!string.IsNullOrEmpty(fieldModel.Comment))
            {
                field.Comments.Add(new CodeCommentStatement(fieldModel.Comment));
            }
            return field;
        }
        #endregion

        #region Properties Function Area

        /// <summary>
        /// 新增属性
        /// </summary>
        /// <param name="pro">属性Model</param>
        public CodeMake AddPropertie(PropertyEntity pro)
            => AddProperties(() =>
            {
                if (pro is null)
                {
                    throw new ArgumentException("属性参数信息不能为null");
                }


                // Declare the read-only Width property.
                string fieldName = string.Empty;
                if (pro.HasGet && pro.HasSet)
                {
                    fieldName = pro.Name + " { get; set; }//";
                }
                else if (pro.HasGet && !pro.HasSet)
                {
                    fieldName = pro.Name + " { get; }//";
                }
                else
                {
                    throw new ArgumentException("属性不能设置只写或当成字段来使用");
                }

                var propertity = GetFieldBy(new FieldEntity(fieldName, pro.Type)
                {
                    Attr = pro.Attr,
                    Comment = pro.Comment
                });
                return new List<CodeTypeMember> { propertity };
            });


        /// <summary>
        /// 增加属性
        /// </summary>
        /// <param name="attr">属性标签</param>
        /// <param name="propertieName">属性名称</param>
        /// <param name="propertieType">属性类型</param>
        /// <param name="comment">属性注释</param>
        public CodeMake AddPropertie(MemberAttributes attr, string propertieName, Type propertieType, string comment = null)
            => AddPropertie(new PropertyEntity(propertieName, propertieType)
            {
                HasGet = true,
                HasSet = true,
                Comment = comment
            });

        /// <summary>
        /// 添加多个属性
        /// </summary>
        /// <param name="pros"></param>
        public CodeMake AddProperties(List<PropertyEntity> pros)
        {
            pros.ForEach(s => AddPropertie(s));
            return this;
        }

        /// <summary>
        /// 新增1个属性(自定义)
        /// </summary>
        /// <param name="propertyMember">Func CodeTypeMember</param>
        public CodeMake AddPropertie(Func<CodeTypeMember> propertyMember)
            => AddProperties(() => new List<CodeTypeMember>
                {
                    propertyMember()
                }
            );

        /// <summary>
        /// 新增多个属性(自定义)
        /// </summary>
        /// <param name="propertyMember">Func list CodeTypeMember</param>
        public CodeMake AddProperties(Func<List<CodeTypeMember>> propertyMember)
        {
            if (BeforeAddPropertie != null)
            {
                BeforeAddPropertie();
            }

            propertyMember().ForEach(p => _targetClass.Members.Add(p));

            if (AfterAddPropertie != null)
            {
                AfterAddPropertie();
            }
            return this;
        }

        #endregion

        #region Method Area

        /// <summary>
        /// 添加方法
        /// </summary>
        /// <param name="methods">方法</param>
        /// <returns>this</returns>
        public CodeMake AddMethod(string method, string comment = null)
            => AddMethod(new MethodEntity { Method = method, Comment = comment });

        /// <summary>
        /// 添加单个方法
        /// </summary>
        /// <param name="methods">方法</param>
        /// <returns>this</returns>
        public CodeMake AddMethod(MethodEntity method)
            => AddMethods(new List<MethodEntity> { method });

        /// <summary>
        /// 添加多个方法
        /// </summary>
        /// <param name="methods">方法集合</param>
        /// <returns>this</returns>
        public CodeMake AddMethods(List<MethodEntity> methods)
            => AddMethods(() =>
            {
                var methodsList = new List<CodeTypeMember>();
                methods.ForEach(m =>
                {
                    CodeSnippetTypeMember snippet = new CodeSnippetTypeMember
                    {
                        Text = m.Method
                    };
                    if (!string.IsNullOrEmpty(m.Comment))
                    {
                        snippet.Comments.Add(new CodeCommentStatement(m.Comment, false));
                    }
                    methodsList.Add(snippet);
                });
                return methodsList;
            });


        /// <summary>
        /// 添加方法(自定义)
        /// </summary>
        /// <param name="method">Func<CodeTypeMember></param>
        public CodeMake AddMethod(Func<CodeTypeMember> method)
            => AddMethods(() => new List<CodeTypeMember> { method() });

        /// <summary>
        /// 添加多个方法(自定义)
        /// </summary>
        /// <param name="method">Func<List<CodeTypeMember>></param>
        public CodeMake AddMethods(Func<List<CodeTypeMember>> method)
        {
            if (BeforeAddMethod != null)
            {
                BeforeAddMethod();
            }

            method().ForEach(m => _targetClass.Members.Add(m));

            if (AfterAddMethod != null)
            {
                AfterAddMethod();
            }
            return this;
        }

        #endregion

        #region OutPut
        /// <summary>
        /// 控制台输出
        /// </summary>
        /// <returns></returns>
        public CodeMake Log()
        {
            Console.WriteLine(GenerateCSharpString());
            return this;
        }

        /// <summary>
        /// 元数据引用控制台输出
        /// </summary>
        /// <returns></returns>
        public CodeMake MetadataLog()
        {
            foreach (var item in _assemblyUsingLocation)
            {
                Console.WriteLine(item);
            }
            return this;
        }

        /// <summary>
        /// 文本输出（string）
        /// </summary>
        /// <param name="fileFullPath">文件地址</param>
        public string GenerateCSharpString()
            => CodeDomOutString(() =>
            {
                CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
                CodeGeneratorOptions options = new CodeGeneratorOptions { BracingStyle = "C" };

                using (StringWriter sourceWriter = new StringWriter())
                {
                    provider.GenerateCodeFromCompileUnit(_targetUnit, sourceWriter, options);
                    return sourceWriter.ToString();
                }
            });

        /// <summary>
        /// 自定义CodeDom输出（string）
        /// </summary>
        /// <param name="fileFullPath">文件地址</param>
        public string CodeDomOutString(Func<string> codeDomContext)
            => codeDomContext();


        /// <summary>
        /// 文件输出（.cs）
        /// </summary>
        /// <param name="fileFullPath">文件地址</param>
        public CodeMake GenerateCSharpFile(string fileFullPath)
        {
            if (string.IsNullOrEmpty(fileFullPath))
            {
                throw new ArgumentException("文件输出路径为空，请设置输出路径！");
            }
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            using (StreamWriter sourceWriter = new StreamWriter(fileFullPath))
            {
                provider.GenerateCodeFromCompileUnit(
                    _targetUnit, sourceWriter, options);
            }
            return this;
        }

        /// <summary>
        /// 文件输出（.cs）
        /// </summary>
        public CodeMake CodeDomOutFile()
            => GenerateCSharpFile(_outputFileName);
        #endregion

        #region CreateInstance Function Area

        /// <summary>
        /// 创建单例对象 默认获取方式为命名空间+类名
        /// </summary>
        /// <returns></returns>
        public object CreateInstanceOfSingleton()
            => CreateInstanceOfSingleton(this.GenerateCSharpString(), this.FullNameSpaceWithClass);

        /// <summary>
        /// 创建单例对象 存取Key自定义
        /// </summary>
        /// <param name="singletonKey"></param>
        /// <returns></returns>
        public object CreateInstanceOfSingleton(string singletonKey)
            => CreateInstanceOfSingleton(this.GenerateCSharpString(), singletonKey);

        /// <summary>
        /// 创建单例对象  按命名空间+类名区分
        /// </summary>
        /// <param name="context">创建对象文本</param>
        /// <param name="singletonKey">命名空间+类名称</param>
        /// <returns></returns>
        public object CreateInstanceOfSingleton(string context, string singletonKey)
        {
            if (HasSingletonInstance(singletonKey))
            {
                return GetSingletonInstanceBy(singletonKey);
            }
            var instance = CreateInstance(context, this.FullNameSpaceWithClass);
            _singletonContainer.Add(singletonKey, instance);
            return instance;
        }

        /// <summary>
        /// 根据本类构建对象
        /// </summary>
        /// <returns>返回Object类，根据内容反射获取信息</returns>
        public object CreateInstance()
            => CreateInstance(this.GenerateCSharpString(), this.FullNameSpaceWithClass);

        /// <summary>
        /// 根据传入内容构建对象
        /// </summary>
        /// <returns>返回Object类，根据内容反射获取信息</returns>
        public object CreateInstance(string context, string fullNamespaceClass)
            => CreateInstance(() =>
            {
                #region Verify
                if (string.IsNullOrEmpty(context))
                {
                    throw new ArgumentException("生成的代码不能为空");
                }
                if (string.IsNullOrEmpty(fullNamespaceClass))
                {
                    throw new ArgumentException("命名空间和类名称不能为空");
                }
                #endregion

                #region 加载构建
                //元数据加载
                if (_assemblyLoad != null)
                {
                    _assemblyLoad();
                }
                MetadataReference[] references = _assemblyUsingLocation.ToArray().Select(r => MetadataReference.CreateFromFile(r)).ToArray();


                CSharpCompilation compilation = CSharpCompilation.Create(
                    Path.GetRandomFileName(),
                    syntaxTrees: new[] { CSharpSyntaxTree.ParseText(context) },
                    references: references,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                #endregion

                #region 创建对象
                using (var ms = new MemoryStream())
                {
                    EmitResult result = compilation.Emit(ms);

                    if (result.Success)
                    {
                        ms.Seek(0, SeekOrigin.Begin);

                        Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                        //var type = assembly.GetType("CodeDOM.CodeDOMCreatedClass");
                        return assembly.CreateInstance(fullNamespaceClass);
                    }
                    else
                    {
                        Console.WriteLine($"================================================");
                        Console.WriteLine($"动态生成实体失败 生成对象内容为：\r\n \r\n {context}");
                        Console.WriteLine($"================================================");
                        return result.Diagnostics.Where(diagnostic =>
                                                    diagnostic.IsWarningAsError ||
                                                    diagnostic.Severity == DiagnosticSeverity.Error);
                    }
                }
                #endregion
            });

        /// <summary>
        /// 构建自定义生成方式和对象
        /// </summary>
        /// <returns>返回Object类，根据内容反射获取信息</returns>
        public object CreateInstance(Func<object> createInfo)
        {
            if (BeforeCreateInstance != null)
            {
                BeforeCreateInstance();
            }

            var resultObj = createInfo();

            if (AfterCreateInstance != null)
            {
                AfterCreateInstance();
            }

            return resultObj;
        }

        #endregion

        #region Singleton Ioc Function Area

        /// <summary>
        /// 获取单例对象
        /// </summary>
        /// <param name="key">命名空间+类名称</param>
        /// <returns></returns>
        public static object GetSingletonInstanceBy(string key)
            => HasSingletonInstance(key) ? _singletonContainer[key] : null;

        /// <summary>
        /// 是否包含单例对象
        /// </summary>
        /// <param name="key">命名空间+类名称</param>
        /// <returns></returns>
        public static bool HasSingletonInstance(string key)
            => _singletonContainer.ContainsKey(key);

        /// <summary>
        /// 更新单例对象
        /// </summary>
        /// <param name="key">命名空间+类名称</param>
        /// <returns></returns>
        public static void UpdateSingletonInstance(string key, object instance)
        {
            if (HasSingletonInstance(key))
            {
                _singletonContainer[key] = instance;
            }
            else
            {
                _singletonContainer.Add(key, instance);
            }
        }

        /// <summary>
        /// 删除单例对象
        /// </summary>
        /// <param name="key">命名空间+类名称</param>
        /// <returns></returns>
        public static void DelSingletonInstance(string key)
            => _singletonContainer.Remove(key);
        #endregion

    }

}
