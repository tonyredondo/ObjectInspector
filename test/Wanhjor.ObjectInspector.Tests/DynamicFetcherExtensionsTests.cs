using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;
// ReSharper disable InconsistentNaming
#pragma warning disable 414

namespace Wanhjor.ObjectInspector.Tests
{
    public class DynamicFetcherExtensionsTests
    {
        private static void ExpectedException<TException>(Action action)
        {
            try
            {
                action();
            }
            catch(Exception ex)
            {
                if (ex is TException)
                    return;
                throw;
            }
        } 
        
        
        
        [Fact]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void TestFieldAccessors()
        {
            var field = new ObjTestFields();

            GetterTest("PublicField", "Public Field");
            GetterTest("PrivateField", "Private Field");
            GetterTest("ProtectedField", "Protected Field");
            GetterTest("InternalField", "Internal Field");
            
            GetterTest("PublicReadonlyField", "Public Readonly Field");
            GetterTest("PrivateReadonlyField", "Private Readonly Field");
            GetterTest("ProtectedReadonlyField", "Protected Readonly Field");
            GetterTest("InternalReadonlyField", "Internal Readonly Field");
            
            SetterTest("PublicField", "Public Field 2");
            SetterTest("PrivateField", "Private Field 2");
            SetterTest("ProtectedField", "Protected Field 2");
            SetterTest("InternalField", "Internal Field 2");
            
            
            void GetterTest(string name, string fieldValue)
            {
                if (field.TryGetMemberValue(name, out string value))
                    Assert.Equal(fieldValue, value);
                else
                    throw new KeyNotFoundException();
            }

            void SetterTest(string name, string fieldValue)
            {
                if (field.TrySetMemberValue(name, fieldValue))
                    GetterTest(name, fieldValue);
                else
                    throw new KeyNotFoundException();
            }
        }

        #region Inner Types for TestFieldAccessors
        
        public class ObjTestFields
        {
            public string PublicField = "Public Field";
            private string PrivateField = "Private Field";
            protected string ProtectedField = "Protected Field";
            internal string InternalField = "Internal Field";
            
            public readonly string PublicReadonlyField = "Public Readonly Field";
            private readonly string PrivateReadonlyField = "Private Readonly Field";
            protected readonly string ProtectedReadonlyField = "Protected Readonly Field";
            internal readonly string InternalReadonlyField = "Internal Readonly Field";
        }

        #endregion

        
        
        [Fact]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void TestPropertyAccessors()
        {
            var objProperties = new ObjTestProperties();

            GetterTest("PublicGetterPublicSetterProp", "Public getter / Public setter property");
            GetterTest("PrivateGetterPrivateSetterProp", "Private getter / Private setter property");
            GetterTest("ProtectedGetterProtectedSetterProp", "Protected getter / Protected setter property");
            GetterTest("InternalGetterInternalSetterProp", "Internal getter / Internal setter property");

            SetterTest("PublicGetterPublicSetterProp", "Public getter / Public setter property 2");
            SetterTest("PrivateGetterPrivateSetterProp", "Private getter / Private setter property 2");
            SetterTest("ProtectedGetterProtectedSetterProp", "Protected getter / Protected setter property 2");
            SetterTest("InternalGetterInternalSetterProp", "Internal getter / Internal setter property 2");

            CallerTest("Sum", new object[] {2, 2}, 4);
            CallerTest("Mult", new object[] {3, 3}, 9);
            
            void GetterTest(string name, string pValue)
            {
                if (objProperties.TryGetMemberValue(name, out string value))
                    Assert.Equal(pValue, value);
                else
                    throw new KeyNotFoundException();
            }

            void SetterTest(string name, string pValue)
            {
                if (objProperties.TrySetMemberValue(name, pValue))
                    GetterTest(name, pValue);
                else
                    throw new KeyNotFoundException();
            }
            
            void CallerTest(string name, object[] arguments, object expectedValue)
            {
                if (objProperties.TryInvokeMethod(name, arguments, out object value))
                    Assert.Equal(expectedValue, value);
                else
                    throw new KeyNotFoundException();
            }
        }

        #region Inner Types for TestPropertyAccessors
        
        public class ObjTestProperties
        {
            public string PublicGetterPublicSetterProp { get; set; } = "Public getter / Public setter property";
            private string PrivateGetterPrivateSetterProp { get; set; } = "Private getter / Private setter property";
            protected string ProtectedGetterProtectedSetterProp { get; set; } = "Protected getter / Protected setter property";
            internal string InternalGetterInternalSetterProp { get; set; } = "Internal getter / Internal setter property";

            public int Sum(int a, int b) => a + b;
            private int Mult(int a, int b) => a * b;
        }
        
        #endregion

    }
}