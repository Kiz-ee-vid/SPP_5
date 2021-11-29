using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    public class DependencyConfiguration
    {       
        private Dictionary<Type, List<DependencyRecord>> dependencies = new Dictionary<Type, List<DependencyRecord>>();
        public Dictionary<Type, List<DependencyRecord>> getDictionary() {
            return this.dependencies;
        }
        public void Register(Type Dep, Type Res, bool isSingleton = false, string name = null) {
            if (!dependencies.ContainsKey(Dep))
                dependencies[Dep] = new List<DependencyRecord>();
                      dependencies[Dep].Add(new DependencyRecord(Res,isSingleton,name));
        }

        public void Register<U, V>(bool isSingleton=false, string name = null) where V : U
           where U : class
        {
            Register(typeof(U), typeof(V), isSingleton, name);
        }
    }
}
