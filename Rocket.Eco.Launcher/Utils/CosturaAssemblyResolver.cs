using Mono.Cecil;

namespace Rocket.Eco.Launcher.Utils
{
    public sealed class CosturaAssemblyResolver : BaseAssemblyResolver
    {
        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            string actName = name.Name;
            
            if (!CosturaHelper.Assemblies.TryGetValue(actName, out CosturaHelper.AssemblyData result))
                return null;

            if (result.Assembly != null)
                return result.Assembly;

            result.Stream.Position = 0;

            try
            {
                AssemblyDefinition defn = AssemblyDefinition.ReadAssembly(result.Stream, new ReaderParameters()
                {
                    AssemblyResolver = this
                });

                result.Assembly = defn;

                return defn;
            }
            catch (ResolutionException e)
            {
                return null;
            }
        }
    }
}
