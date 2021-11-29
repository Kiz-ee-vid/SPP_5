using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    public class DependencyRecord
    {
        public class SpecialDep : Attribute
        {
            public string did { get; }
            public SpecialDep(string did)
            {
                this.did = did;
            }
        }
        public Type Dependency { get; }
        public bool isSingleton { get; }
        public string name { get; }
        public DependencyRecord(Type ImplementationClass, bool isSeingleton, string nameOfDepend)
        {
            this.Dependency = ImplementationClass;
            this.name= nameOfDepend;
            this.isSingleton= isSeingleton;
        }
    }
}
