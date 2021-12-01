using ConsoleApp1;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static ConsoleApp1.DependencyRecord;

namespace Test
{
    public class Tests
    {

        public interface ImyInterface{ }
        public interface i2{}
        public interface i3{}
        public interface i4{public i3 getI3(); }
        public interface i5{public IEnumerable<i3> getI3s();}

        class i2Impl : i2
        { private ImyInterface inter;
            public i2Impl(ImyInterface inter)
            {
                this.inter = inter;
            }
        }
        class i3Impl : i3
        {
            private IEnumerable enumerable;

            public i3Impl(IEnumerable en)
            {
                this.enumerable = en;
            }

        }
        class i3Impl2 : i3
        {

            public i3Impl2()
            {
                Console.WriteLine("i3Impl2 constructor called");
                // this.impl = im;
            }
        }

        class i3Impl3 : i3
        {
            // private i2 impl;

            public i3Impl3()
            {
                // this.impl = im;
            }
        }

        class i4Impl : i4
        {
            public i3 impl;

            public i4Impl([SpecialDep("name")]i3 im)
            {

                this.impl = im;
            }

            public i3 getI3()
            {
                return impl;
            }
        }

        class i5Impl : i5
        {
            public IEnumerable<i3> i3s;

            public i5Impl(IEnumerable<i3> i3s)
            {
                this.i3s = i3s;
            }

            public IEnumerable<i3> getI3s()
            {
                return i3s;
            }
        }

        class IMyInterfaceImpl : ImyInterface
        {

        }


        interface IService<TRepository> where TRepository : i2
        {
            i2 getI2();
        }
        class ServiceImpl<TRepository> : IService<TRepository>
        where TRepository : i2
        {
            private i2 aiTwo;
            public ServiceImpl(TRepository repository)
            {
                this.aiTwo = repository;                
            }

            public i2 getI2()
            {
                return this.aiTwo;
            }
        }


        DependencyContainer depCont;
        DependencyConfiguration depConfig;
        DependencyRecord depRecord;

        [SetUp]
        public void Setup()
        {
            depConfig = new DependencyConfiguration();
            depCont = new DependencyContainer(depConfig);
            depRecord=new DependencyRecord(typeof(i2Impl),false,null);
        }

        [Test]
        public void DependencyContainerCheckAvailabilityTest()
        {
            bool test=depCont.checkAvailability(typeof(i2));
            Assert.AreEqual(false,test);
            depConfig.Register<i2,i2Impl>();
            test = depCont.checkAvailability(typeof(i2));
            Assert.AreEqual(true,test);
        }

        [Test]
        public void DependencyContainerGetConstructorTest()
        {
            ConstructorInfo test = depCont.getConstructor(typeof(i2Impl));
            Assert.Null(test);       
            depConfig.Register<i2, i2Impl>();
            test = depCont.getConstructor(typeof(i2Impl));
            Assert.Null(test);
            depConfig.Register<ImyInterface, IMyInterfaceImpl>();
            test = depCont.getConstructor(typeof(i2Impl));
            Assert.NotNull(test);
        }

        [Test]
        public void DependencyContainerResolveTestGeneric()
        {
            ImyInterface test = null;
            try
            {
                 test = depCont.Resolve<ImyInterface>();
            }
            catch (KeyNotFoundException e) {
                Assert.IsTrue(e.Message.Contains("failed to find type:ImyInterface in current configuration"));
            }          
            Assert.Null(test);
            depConfig.Register<ImyInterface, IMyInterfaceImpl>();
            test = depCont.Resolve<ImyInterface>();
            Assert.NotNull(test);
        }

        [Test]
        public void DependencyContainerResolveTestNormal()
        {
            ImyInterface test = null;
            try
            {
                test =(ImyInterface) depCont.Resolve(typeof(ImyInterface));
            }
            catch (KeyNotFoundException e)
            {
                Assert.IsTrue(e.Message.Contains("failed to find type:ImyInterface in current configuration"));
            }
            Assert.Null(test);
            depConfig.Register<ImyInterface, IMyInterfaceImpl>();
            test = (ImyInterface)depCont.Resolve(typeof(ImyInterface));
            Assert.NotNull(test);
        }

        [Test]
        public void DependencyContainerDependencyRecordToObjectTest()
        {    
            object test = null;
            try
            {
                test= depCont.deprToObj(depRecord);
            }
            catch (KeyNotFoundException e)
            {
                Assert.IsTrue(e.Message.Contains("failed to find optimal constructor for generating:"));
            }
            Assert.Null(test);
            depConfig.Register<ImyInterface, IMyInterfaceImpl>();
            test = depCont.deprToObj(depRecord);
            Assert.NotNull(test);
        }

        [Test]//also test for open generics
        public void DependencyConfigurationGetDictionarytest()
        {
            Dictionary<Type, List<DependencyRecord>> test=null;          
            test = depConfig.getDictionary();
            Assert.AreEqual(0, test.Count);
            depConfig.Register<ImyInterface, IMyInterfaceImpl>();
            depConfig.Register<i2, i2Impl>();
            depConfig.Register(typeof(IService<>), typeof(ServiceImpl<>), true);
            test = depConfig.getDictionary();
            Assert.AreEqual(3, test.Count);
        }

        [Test]
        public void NamedDependenciesTest()
        {
            depConfig.Register<ImyInterface, IMyInterfaceImpl>();
            depConfig.Register<i2, i2Impl>();
            depConfig.Register<i3, i3Impl>();
            depConfig.Register<i3, i3Impl2>();
            depConfig.Register<i3, i3Impl3>(true, "name");
            depConfig.Register<i4, i4Impl>(true, "name");
            Assert.AreNotEqual(depCont.Resolve<i3>(), depCont.Resolve<i3>("name"));
            Assert.AreEqual(typeof(i3Impl3), depCont.Resolve<i3>("name").GetType());
            Assert.AreEqual(typeof(i3Impl3), depCont.Resolve<i4>().getI3().GetType());
        }

        [Test]
        public void listDependenciesTest()
        {
            depConfig.Register<ImyInterface, IMyInterfaceImpl>();
            depConfig.Register<i2, i2Impl>();
            depConfig.Register<i3, i3Impl>();
            depConfig.Register<i3, i3Impl2>();
            depConfig.Register<i3, i3Impl3>(true, "name");
            depConfig.Register<i4, i4Impl>(true, "name");
            depConfig.Register<i5, i5Impl>();

            var test = depCont.Resolve<IEnumerable<i3>>();
            Assert.AreEqual(3, depCont.Resolve<IEnumerable<i3>>().Count());
        }


    }
}