using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;
// ReSharper disable InconsistentNaming
#pragma warning disable 414

namespace Wanhjor.ObjectInspector.Tests
{
    public class DuckTypeNonPublicTestsPrivateClass
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

            var duckFieldsNotFound = field.DuckAs<IObjTestFieldsNotFound>();
            Assert.Same(duckFieldsNotFound.PublicField, "Public Field");
            duckFieldsNotFound.PublicField = "Public Field";
            ExpectedException<DuckTypePropertyOrFieldNotFoundException>(() => _ = duckFieldsNotFound.PrivateField);
            ExpectedException<DuckTypePropertyOrFieldNotFoundException>(() => _ = duckFieldsNotFound.ProtectedField);
            ExpectedException<DuckTypePropertyOrFieldNotFoundException>(() => _ = duckFieldsNotFound.InternalField);
            ExpectedException<DuckTypePropertyOrFieldNotFoundException>(() => duckFieldsNotFound.PrivateField = "_");
            ExpectedException<DuckTypePropertyOrFieldNotFoundException>(() => duckFieldsNotFound.ProtectedField = "_");
            ExpectedException<DuckTypePropertyOrFieldNotFoundException>(() => duckFieldsNotFound.InternalField = "_");

            var duckField = field.DuckAs<IObjTestFields>();
            Assert.Same(duckField.PublicField, "Public Field");
            Assert.Same(duckField.PrivateField, "Private Field");
            Assert.Same(duckField.ProtectedField, "Protected Field");
            Assert.Same(duckField.InternalField, "Internal Field");
            duckField.PublicField = "Public Field";
            duckField.PrivateField = "Private Field";
            duckField.ProtectedField = "Protected Field";
            duckField.InternalField = "Internal Field";

            var duckReadonlyField = field.DuckAs<IObjTestReadOnlyFields>();
            Assert.Same(duckReadonlyField.PublicReadonlyField, "Public Readonly Field");
            Assert.Same(duckReadonlyField.PrivateReadonlyField, "Private Readonly Field");
            Assert.Same(duckReadonlyField.ProtectedReadonlyField, "Protected Readonly Field");
            Assert.Same(duckReadonlyField.InternalReadonlyField, "Internal Readonly Field");
            ExpectedException<DuckTypeFieldIsReadonlyException>(() => duckReadonlyField.PublicReadonlyField = "_");
            ExpectedException<DuckTypeFieldIsReadonlyException>(() => duckReadonlyField.PrivateReadonlyField = "_");
            ExpectedException<DuckTypeFieldIsReadonlyException>(() => duckReadonlyField.ProtectedReadonlyField = "_");
            ExpectedException<DuckTypeFieldIsReadonlyException>(() => duckReadonlyField.InternalReadonlyField = "_");
        }

        #region Inner Types for TestFieldAccessors
        
        public interface IObjTestFieldsNotFound
        {
            [Duck(Kind = DuckKind.Field)]
            string PublicField { get; set; }
            [Duck(Kind = DuckKind.Field)]
            string PrivateField { get; set; }
            [Duck(Kind = DuckKind.Field)]
            string ProtectedField { get; set; }
            [Duck(Kind = DuckKind.Field)]
            string InternalField { get; set; }
        }
        
        public interface IObjTestFields
        {
            [Duck(Kind = DuckKind.Field)]
            string PublicField { get; set; }
            [Duck(Kind = DuckKind.Field, Flags = BindingFlags.Instance | BindingFlags.NonPublic)]
            string PrivateField { get; set; }
            [Duck(Kind = DuckKind.Field, Flags = BindingFlags.Instance | BindingFlags.NonPublic)]
            string ProtectedField { get; set; }
            [Duck(Kind = DuckKind.Field, Flags = BindingFlags.Instance | BindingFlags.NonPublic)]
            string InternalField { get; set; }
        }
        
        public interface IObjTestReadOnlyFields
        {
            [Duck(Kind = DuckKind.Field)]
            string PublicReadonlyField { get; set; }
            [Duck(Kind = DuckKind.Field, Flags = BindingFlags.Instance | BindingFlags.NonPublic)]
            string PrivateReadonlyField { get; set; }
            [Duck(Kind = DuckKind.Field, Flags = BindingFlags.Instance | BindingFlags.NonPublic)]
            string ProtectedReadonlyField { get; set; }
            [Duck(Kind = DuckKind.Field, Flags = BindingFlags.Instance | BindingFlags.NonPublic)]
            string InternalReadonlyField { get; set; }
        }
        
        private class ObjTestFields
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

            var duckProperties = objProperties.DuckAs<IObjTestProperties>();
            
            // Getter
            Assert.Same(duckProperties.PublicGetterPublicSetterProp, "Public getter / Public setter property");
            Assert.Same(duckProperties.PrivateGetterPrivateSetterProp, "Private getter / Private setter property");
            Assert.Same(duckProperties.ProtectedGetterProtectedSetterProp, "Protected getter / Protected setter property");
            Assert.Same(duckProperties.InternalGetterInternalSetterProp, "Internal getter / Internal setter property");
            
            Assert.Same(duckProperties.PublicGetterPrivateSetterProp, "Public getter / Private setter property");
            Assert.Same(duckProperties.PublicGetterProtectedSetterProp, "Public getter / Protected setter property");
            Assert.Same(duckProperties.PublicGetterInternalSetterProp, "Public getter / Internal setter property");
            Assert.Same(duckProperties.PublicGetterNoSetterProp, "Public getter / no setter property");
            
            Assert.Same(duckProperties.PrivateGetterPublicSetterProp, "Private getter / Public setter property");
            Assert.Same(duckProperties.ProtectedGetterPublicSetterProp, "Protected getter / Public setter property");
            Assert.Same(duckProperties.InternalGetterPublicSetterProp, "Internal getter / Public setter property");
            ExpectedException<DuckTypePropertyCantBeReadException>(() => _ = duckProperties.NoGetterPublicSetterProp);

            // Setter
            duckProperties.PublicGetterPublicSetterProp = "Public getter / Public setter property";
            duckProperties.PrivateGetterPrivateSetterProp = "Private getter / Private setter property";
            duckProperties.ProtectedGetterProtectedSetterProp = "Protected getter / Protected setter property";
            duckProperties.InternalGetterInternalSetterProp = "Internal getter / Internal setter property";
            
            duckProperties.PublicGetterPrivateSetterProp = "Public getter / Private setter property";
            duckProperties.PublicGetterProtectedSetterProp = "Public getter / Protected setter property";
            duckProperties.PublicGetterInternalSetterProp = "Public getter / Internal setter property";
            ExpectedException<DuckTypePropertyCantBeWrittenException>(() => duckProperties.PublicGetterNoSetterProp = "Public getter / no setter property");

            duckProperties.PrivateGetterPublicSetterProp = "Private getter / Public setter property";
            duckProperties.ProtectedGetterPublicSetterProp = "Protected getter / Public setter property";
            duckProperties.InternalGetterPublicSetterProp = "Internal getter / Public setter property";
            duckProperties.NoGetterPublicSetterProp = "No getter / Public setter property";
        }

        #region Inner Types for TestPropertyAccessors
        
        public interface IObjTestProperties
        {
            [Duck(Flags = DuckAttribute.AllFlags)]
            int PrivateValueProp { get; set; }
            
            [Duck(Flags = DuckAttribute.AllFlags)]
            string PublicGetterPublicSetterProp { get; set; }
            [Duck(Flags = DuckAttribute.AllFlags)]
            string PrivateGetterPrivateSetterProp { get; set; }
            [Duck(Flags = DuckAttribute.AllFlags)]
            string ProtectedGetterProtectedSetterProp { get; set; }
            [Duck(Flags = DuckAttribute.AllFlags)]
            string InternalGetterInternalSetterProp { get; set; }
            
            [Duck(Flags = DuckAttribute.AllFlags)]
            string PublicGetterPrivateSetterProp { get; set; }
            [Duck(Flags = DuckAttribute.AllFlags)]
            string PublicGetterProtectedSetterProp { get; set; }
            [Duck(Flags = DuckAttribute.AllFlags)]
            string PublicGetterInternalSetterProp { get; set; }
            [Duck(Flags = DuckAttribute.AllFlags)]
            string PublicGetterNoSetterProp { get; set; }
            
            [Duck(Flags = DuckAttribute.AllFlags)]
            string PrivateGetterPublicSetterProp { get; set; }
            [Duck(Flags = DuckAttribute.AllFlags)]
            string ProtectedGetterPublicSetterProp { get; set; }
            [Duck(Flags = DuckAttribute.AllFlags)]
            string InternalGetterPublicSetterProp { get; set; }
            [Duck(Flags = DuckAttribute.AllFlags)]
            string NoGetterPublicSetterProp { get; set; }
        }
        
        private class ObjTestProperties
        {
            private int PrivateValueProp { get; set; }

            private string _privateGetterPublicSetterProp = "Private getter / Public setter property";
            private string _protectedGetterPublicSetterProp = "Protected getter / Public setter property";
            private string _internalGetterPublicSetterProp = "Internal getter / Public setter property";
            private string _noGetterPublicSetterProp = "No getter / Public setter property";
            
            public string PublicGetterPublicSetterProp { get; set; } = "Public getter / Public setter property";
            private string PrivateGetterPrivateSetterProp { get; set; } = "Private getter / Private setter property";
            protected string ProtectedGetterProtectedSetterProp { get; set; } = "Protected getter / Protected setter property";
            internal string InternalGetterInternalSetterProp { get; set; } = "Internal getter / Internal setter property";

            public string PublicGetterPrivateSetterProp { get; private set; } = "Public getter / Private setter property";
            public string PublicGetterProtectedSetterProp { get; protected set; } = "Public getter / Protected setter property";
            public string PublicGetterInternalSetterProp { get; internal set; } = "Public getter / Internal setter property";
            public string PublicGetterNoSetterProp { get; } = "Public getter / no setter property";
            
            public string PrivateGetterPublicSetterProp
            {
                private get => _privateGetterPublicSetterProp;
                set => _privateGetterPublicSetterProp = value;
            }
            public string ProtectedGetterPublicSetterProp
            {
                protected get => _protectedGetterPublicSetterProp;
                set => _protectedGetterPublicSetterProp = value;
            }
            public string InternalGetterPublicSetterProp
            {
                internal get => _internalGetterPublicSetterProp;
                set => _internalGetterPublicSetterProp = value;
            }
            public string NoGetterPublicSetterProp {  set => _noGetterPublicSetterProp = value; }
        }
        
        #endregion

    }
}