using System;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

const string supported = "1.5.78.11833";
if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: ReferenceCheck <game Managed directory> <API directory>");
    return 2;
}
try
{
    bool valid = true;
    for (int index = 0; index < args.Length; index++)
    {
        using var pe = new PEReader(File.OpenRead(Path.Combine(args[index], "Assembly-CSharp.dll")));
        var reader = pe.GetMetadataReader();
        string? version = null;
        bool hasApi = false;
        foreach (var handle in reader.TypeDefinitions)
        {
            var type = reader.GetTypeDefinition(handle);
            if (reader.GetString(type.Name) == "Mod" && reader.GetString(type.Namespace) == "Modding") hasApi = true;
            if (reader.GetString(type.Name) != "Constants") continue;
            foreach (var fieldHandle in type.GetFields())
            {
                var field = reader.GetFieldDefinition(fieldHandle);
                if (reader.GetString(field.Name) != "GAME_VERSION" || field.GetDefaultValue().IsNil) continue;
                var constant = reader.GetConstant(field.GetDefaultValue());
                if (constant.TypeCode == ConstantTypeCode.String)
                    version = System.Text.Encoding.Unicode.GetString(reader.GetBlobBytes(constant.Value));
            }
        }
        Console.WriteLine($"{(index == 0 ? "Game" : "API")}: version {version ?? "unknown"}; mod loader {(hasApi ? "present" : "absent")}");
        if (version != supported || (index == 1 && !hasApi)) valid = false;
    }
    if (!valid)
    {
        Console.Error.WriteLine($"Build stopped: both references must target {supported}, and API must include the mod loader. Game files were not changed.");
        return 1;
    }
    foreach (string name in new[] { "UnityEngine.dll", "PlayMaker.dll" })
        if (!File.Exists(Path.Combine(args[0], name))) throw new FileNotFoundException("Missing game reference: " + name);
    return 0;
}
catch (Exception ex) when (ex is IOException || ex is BadImageFormatException || ex is UnauthorizedAccessException)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}
