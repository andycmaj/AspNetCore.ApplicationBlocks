using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace AspNetCore.ApplicationBlocks
{
    public static class CompileLibrariesExtensions
    {
        public static IEnumerable<Assembly> GetLoadableAssemblies(
            this IEnumerable<CompilationLibrary> compilationLibraries
        )
        {
            return compilationLibraries
                .Where(lib => lib.Type == "project" || lib.Type == "package")
                .Where(lib => lib.Assemblies.Any())
                .Select(lib => lib.Assemblies.First())
                .Select(assemblyName => {
                    try {
                        var lastSlash = assemblyName.LastIndexOf("/");
                        if (lastSlash != -1)
                        {
                            assemblyName = assemblyName.Remove(0, lastSlash + 1);
                        }

                        var nameToLoad = assemblyName.Replace(".dll", string.Empty);

                        var assembly = Assembly.Load(new AssemblyName(nameToLoad));
                        assembly.GetExportedTypes();

                        return assembly;
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(assembly => assembly != null);
        }
    }
}