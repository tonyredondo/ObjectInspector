using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace Wanhjor.ObjectInspector.Tests
{
    public class DuckTypeNonPublicTests
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
            ExpectedException<DuckTypePropertyOrFieldNotFound>(() => _ = duckFieldsNotFound.PrivateField);
            ExpectedException<DuckTypePropertyOrFieldNotFound>(() => _ = duckFieldsNotFound.ProtectedField);
            ExpectedException<DuckTypePropertyOrFieldNotFound>(() => _ = duckFieldsNotFound.InternalField);
            ExpectedException<DuckTypePropertyOrFieldNotFound>(() => duckFieldsNotFound.PrivateField = "_");
            ExpectedException<DuckTypePropertyOrFieldNotFound>(() => duckFieldsNotFound.ProtectedField = "_");
            ExpectedException<DuckTypePropertyOrFieldNotFound>(() => duckFieldsNotFound.InternalField = "_");

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
            ExpectedException<DuckTypeFieldIsReadonly>(() => duckReadonlyField.PublicReadonlyField = "_");
            ExpectedException<DuckTypeFieldIsReadonly>(() => duckReadonlyField.PrivateReadonlyField = "_");
            ExpectedException<DuckTypeFieldIsReadonly>(() => duckReadonlyField.ProtectedReadonlyField = "_");
            ExpectedException<DuckTypeFieldIsReadonly>(() => duckReadonlyField.InternalReadonlyField = "_");
        }

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


        [Fact]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void TestPropertyAccessors()
        {
            
        }
        
        public class ObjTestProperties
        {
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
    }
}