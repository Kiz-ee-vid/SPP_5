using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using static ConsoleApp1.DependencyRecord;

namespace ConsoleApp1
{
    public class DependencyContainer
    {
        private DependencyConfiguration dc;

        private Dictionary<Type, object> singletonObjects = new Dictionary<Type, object>();
        public DependencyContainer(DependencyConfiguration dc)
        {
            this.dc = dc;
        }

        public bool checkAvailability(Type t, string name = null) {
            if ((typeof(IEnumerable).IsAssignableFrom(t))&&(t.GetGenericArguments().Length==1))
            {
                Type genType = t.GetGenericArguments()[0];
                if (dc.getDictionary().ContainsKey(genType)) { return true; }
            }

            bool ret = dc.getDictionary().ContainsKey(t);

            if ((ret) && (name != null)) {
                foreach (DependencyRecord dr in dc.getDictionary()[t]) {
                    if (dr.name == name) return true;
                }
            }
            return ret;
        }

        public ConstructorInfo getConstructor(Type t) {
            ParameterInfo[] paramz;
            foreach (ConstructorInfo ci in t.GetConstructors()) {
                bool found= true;
                paramz = ci.GetParameters();
                foreach (ParameterInfo pi in paramz) {
                    if (!checkAvailability(pi.ParameterType)) 
                    { found = false; }
                }
                if (found) return (ci);
            }

            return null;
        }
        public T Resolve<T>(string name = null) {
            return (T)Resolve(typeof(T),name);
        }

        public object deprToObj(DependencyRecord dr) {
            object ret = null;
            if ((dr.isSingleton) && (singletonObjects.ContainsKey(dr.Dependency)))
            {
                return singletonObjects[dr.Dependency];
            }

            ConstructorInfo constr = getConstructor(dr.Dependency);
            if (constr != null)
            {
                ParameterInfo[] argTypes = constr.GetParameters();
                object[] argz = new object[argTypes.Length];
                for (int i = 0; i < argz.Length; i++)
                {
                    var nameAttr = argTypes[i].GetCustomAttribute<SpecialDep>();
                    string depName = ((nameAttr == null) ? (dr.name) : (nameAttr.did));
                    argz.SetValue(Resolve(argTypes[i].ParameterType, depName), i);
                }
                //todo try catch
                ret = Activator.CreateInstance(dr.Dependency, argz);
                if (dr.isSingleton)
                {
                    singletonObjects[dr.Dependency] = ret;
                }
            }
            else
            {
                throw new KeyNotFoundException("failed to find optimal constructor for generating:"
                    + dr.Dependency + ((dr.name != null) ? (" with name \"" + dr.name + '\"') : ("")) + " in current configuration");
            }

            return ret;
        }

        public object Resolve(Type t,string name=null){
            object ret=null;
            if (typeof(IEnumerable).IsAssignableFrom(t))
            {
                Type genType = t.GetGenericArguments()[0];
                var implementations = Array.CreateInstance(genType, dc.getDictionary()[genType].Count);
                for (int i = 0; i < implementations.Length; i++)
                {
                    object obj = null;
                    try { obj = deprToObj(dc.getDictionary()[genType][i]); } catch(KeyNotFoundException e) {

                    }
                    finally
                    {
                        implementations.SetValue(obj, i);
                    }
   
                }
                return implementations;
            }
            DependencyRecord dr = null;

            if ((this.dc.getDictionary().ContainsKey(t))
                ||
                ((t.GenericTypeArguments.Length != 0) &&(!this.dc.getDictionary().ContainsKey(t))&&(this.dc.getDictionary().ContainsKey(t.GetGenericTypeDefinition())))
                ) {
                IEnumerable<DependencyRecord> list2;

                if (this.dc.getDictionary().ContainsKey(t))
                {
                    var list = this.dc.getDictionary()[t];
                    list2 = list;
                }
                else {

                    Type clearGen = t.GetGenericTypeDefinition();
                    var clearDependencies = this.dc.getDictionary()[clearGen];
                    List<DependencyRecord> newDependencies= new List<DependencyRecord>();
                    foreach (DependencyRecord cleardependency in clearDependencies) {
                        Type newType = cleardependency.Dependency.MakeGenericType(t.GetGenericArguments()[0]);
                        newDependencies.Add(new DependencyRecord(newType, cleardependency.isSingleton, cleardependency.name));
                        this.dc.Register(t,newType, cleardependency.isSingleton, cleardependency.name);
                    }
                    list2 = newDependencies;

                }
                


                if (name != null) {
                    list2 = list2.Where((a)=>(a.name==name));
                }
           
               
                foreach (DependencyRecord depRec in list2) {

                    if ((depRec.isSingleton) && (singletonObjects.ContainsKey(depRec.Dependency)))
                    {
                        return singletonObjects[depRec.Dependency];
                    }

                    ////depRec.Dependency.GetConstructors
                    if (null!= getConstructor(depRec.Dependency)) {
                        dr = depRec; break;
                    }
                }
                if (dr == null) {
                    throw new KeyNotFoundException("failed to find type:"+ t.Name +((name!=null)?(" with name \""+name+'\"'):(""))+" in current configuration");
                }             
                ret = deprToObj(dr);
            }
            else
            {
                throw new KeyNotFoundException("failed to find type:" + t.Name + " in current configuration");

            }
            return ret;
        }
    }
}
