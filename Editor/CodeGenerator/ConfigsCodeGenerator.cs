namespace EM.Configs.Editor
{

using System.Reflection;
using Assistant.Editor;

public sealed class ConfigsCodeGenerator
{
	private readonly TypeInfo _typeInfo;

	private readonly string _path;

	#region ConfigsCodeGenerator

	public ConfigsCodeGenerator(TypeInfo typeInfo,
		string path)
	{
		_typeInfo = typeInfo;
		_path = path;
	}

	public void Execute()
	{
		new CodeGeneratorSimple(nameof(ConfigLink) + "ExtensionGenerated.cs", _path,
				new CodeGeneratorSimpleComment("This code is generated automatically, do not change it!",
					new CodeGeneratorSimpleComment("ReSharper disable All",
						new CodeGeneratorSimpleNamespace(_typeInfo.Namespace,
							new CodeGeneratorSimpleUsing(new[] {"System.Collections.Generic", "System.Linq", "EM.Configs"},
								new CodeGeneratorSimpleClass(nameof(ConfigLink) + "Extension", "static",
									new CodeGeneratorConfigLinkExtension(_typeInfo)))))))
			.Create();
	}

	#endregion
}

}