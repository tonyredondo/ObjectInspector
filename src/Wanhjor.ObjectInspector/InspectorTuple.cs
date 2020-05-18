using System.Runtime.CompilerServices;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Inspector base class for an object inspector
    /// </summary>
    public class InspectorBase
    {
        protected readonly ObjectInspector Inspector;
        protected ObjectInspector.ObjectData InstanceData;

        /// <summary>
        /// Creates a new inspector tuple using an object inspector
        /// </summary>
        /// <param name="inspector">Object inspector</param>
        protected InspectorBase(ObjectInspector inspector)
        {
            Inspector = inspector;
        }

        /// <summary>
        /// Sets an object instance to inspect
        /// </summary>
        /// <param name="instance">Object instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetInstance(object instance)
        {
            InstanceData = Inspector.With(instance);
        }
    }

    /// <summary>
    /// Tuple to inspect an object instance
    /// </summary>
    public class InspectorTuple<T1> : InspectorBase
    {
        private readonly string _name1;

        /// <summary>
        /// Item Value for Name1
        /// </summary>
        public T1 Item1
        {
            get => (T1)InstanceData[_name1]!;
            set => InstanceData[_name1] = value;
        }

        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        public InspectorTuple(string name1)
            : this(name1, new ObjectInspector(name1)) { }

        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        public InspectorTuple(InspectName name1)
            : this(name1.Name, new ObjectInspector(name1)) { }

        /// <summary>
        /// Creates a new inspector tuple for an object inspector
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="inspector">Object inspector instance</param>
        public InspectorTuple(string name1, ObjectInspector inspector)
            : base(inspector)
        {
            _name1 = name1;
        }
    }

    /// <summary>
    /// Tuple to inspect an object instance
    /// </summary>
    public class InspectorTuple<T1, T2> : InspectorTuple<T1>
    {
        private readonly string _name2;

        /// <summary>
        /// Item Value for Name2
        /// </summary>
        public T2 Item2
        {
            get => (T2)InstanceData[_name2]!;
            set => InstanceData[_name2] = value;
        }

        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        public InspectorTuple(string name1, string name2)
            : this(name1, name2, new ObjectInspector(name1, name2)) { }


        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        public InspectorTuple(InspectName name1, InspectName name2)
            : this(name1.Name, name2.Name, new ObjectInspector(name1, name2)) { }

        /// <summary>
        /// Creates a new inspector tuple for an object inspector
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="inspector">Object inspector instance</param>
        public InspectorTuple(string name1, string name2, ObjectInspector inspector)
            : base(name1, inspector)
        {
            _name2 = name2;
        }
    }

    /// <summary>
    /// Tuple to inspect an object instance
    /// </summary>
    public class InspectorTuple<T1, T2, T3> : InspectorTuple<T1, T2>
    {
        private readonly string _name3;

        /// <summary>
        /// Item Value for Name3
        /// </summary>
        public T3 Item3
        {
            get => (T3)InstanceData[_name3]!;
            set => InstanceData[_name3] = value;
        }

        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        public InspectorTuple(string name1, string name2, string name3)
            : this(name1, name2, name3, new ObjectInspector(name1, name2, name3)) { }

        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        public InspectorTuple(InspectName name1, InspectName name2, InspectName name3)
            : this(name1.Name, name2.Name, name3.Name, new ObjectInspector(name1, name2, name3)) { }

        /// <summary>
        /// Creates a new inspector tuple for an object inspector
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        /// <param name="inspector">Object inspector instance</param>
        public InspectorTuple(string name1, string name2, string name3, ObjectInspector inspector)
            : base(name1, name2, inspector)
        {
            _name3 = name3;
        }
    }

    /// <summary>
    /// Tuple to inspect an object instance
    /// </summary>
    public class InspectorTuple<T1, T2, T3, T4> : InspectorTuple<T1, T2, T3>
    {
        private readonly string _name4;

        /// <summary>
        /// Item Value for Name4
        /// </summary>
        public T4 Item4
        {
            get => (T4)InstanceData[_name4]!;
            set => InstanceData[_name4] = value;
        }

        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        /// <param name="name4">Property or field name 4</param>
        public InspectorTuple(string name1, string name2, string name3, string name4)
            : this(name1, name2, name3, name4, new ObjectInspector(name1, name2, name3, name4)) { }

        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        /// <param name="name4">Property or field name 4</param>
        public InspectorTuple(InspectName name1, InspectName name2, InspectName name3, InspectName name4)
            : this(name1.Name, name2.Name, name3.Name, name4.Name, new ObjectInspector(name1, name2, name3, name4)) { }

        /// <summary>
        /// Creates a new inspector tuple for an object inspector
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        /// <param name="name4">Property or field name 4</param>
        /// <param name="inspector">Object inspector instance</param>
        public InspectorTuple(string name1, string name2, string name3, string name4, ObjectInspector inspector)
            : base(name1, name2, name3, inspector)
        {
            _name4 = name4;
        }
    }

    /// <summary>
    /// Tuple to inspect an object instance
    /// </summary>
    public class InspectorTuple<T1, T2, T3, T4, T5> : InspectorTuple<T1, T2, T3, T4>
    {
        private readonly string _name5;

        /// <summary>
        /// Item Value for Name5
        /// </summary>
        public T5 Item5
        {
            get => (T5)InstanceData[_name5]!;
            set => InstanceData[_name5] = value;
        }

        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        /// <param name="name4">Property or field name 4</param>
        /// <param name="name5">Property or field name 5</param>
        public InspectorTuple(string name1, string name2, string name3, string name4, string name5)
            : this(name1, name2, name3, name4, name5, new ObjectInspector(name1, name2, name3, name4, name5)) { }

        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        /// <param name="name4">Property or field name 4</param>
        /// <param name="name5">Property or field name 5</param>
        public InspectorTuple(InspectName name1, InspectName name2, InspectName name3, InspectName name4, InspectName name5)
            : this(name1.Name, name2.Name, name3.Name, name4.Name, name5.Name, new ObjectInspector(name1, name2, name3, name4, name5)) { }

        /// <summary>
        /// Creates a new inspector tuple for an object inspector
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        /// <param name="name4">Property or field name 4</param>
        /// <param name="name5">Property or field name 5</param>
        /// <param name="inspector">Object inspector instance</param>
        public InspectorTuple(string name1, string name2, string name3, string name4, string name5, ObjectInspector inspector)
            : base(name1, name2, name3, name4, inspector)
        {
            _name5 = name5;
        }
    }

    /// <summary>
    /// Tuple to inspect an object instance
    /// </summary>
    public class InspectorTuple<T1, T2, T3, T4, T5, T6> : InspectorTuple<T1, T2, T3, T4, T5>
    {
        private readonly string _name6;

        /// <summary>
        /// Item Value for Name6
        /// </summary>
        public T6 Item6
        {
            get => (T6)InstanceData[_name6]!;
            set => InstanceData[_name6] = value;
        }

        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        /// <param name="name4">Property or field name 4</param>
        /// <param name="name5">Property or field name 5</param>
        /// <param name="name6">Property or field name 6</param>
        public InspectorTuple(string name1, string name2, string name3, string name4, string name5, string name6)
            : this(name1, name2, name3, name4, name5, name6, new ObjectInspector(name1, name2, name3, name4, name5, name6)) { }

        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        /// <param name="name4">Property or field name 4</param>
        /// <param name="name5">Property or field name 5</param>
        /// <param name="name6">Property or field name 6</param>
        public InspectorTuple(InspectName name1, InspectName name2, InspectName name3, InspectName name4, InspectName name5, InspectName name6)
            : this(name1.Name, name2.Name, name3.Name, name4.Name, name5.Name, name6.Name, new ObjectInspector(name1, name2, name3, name4, name5, name6)) { }

        /// <summary>
        /// Creates a new inspector tuple for an object inspector
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        /// <param name="name4">Property or field name 4</param>
        /// <param name="name5">Property or field name 5</param>
        /// <param name="name6">Property or field name 6</param>
        /// <param name="inspector">Object inspector instance</param>
        public InspectorTuple(string name1, string name2, string name3, string name4, string name5, string name6, ObjectInspector inspector)
            : base(name1, name2, name3, name4, name5, inspector)
        {
            _name6 = name6;
        }
    }
}