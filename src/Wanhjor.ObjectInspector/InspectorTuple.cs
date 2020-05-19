using System.Runtime.CompilerServices;

namespace Wanhjor.ObjectInspector
{
    /// <summary>
    /// Inspector base class for an object inspector
    /// </summary>
    public class InspectorBase
    {
        protected object? Instance;

        /// <summary>
        /// Sets an object instance to inspect
        /// </summary>
        /// <param name="instance">Object instance</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetInstance(object instance)
        {
            Instance = instance;
        }
    }

    /// <summary>
    /// Tuple to inspect an object instance
    /// </summary>
    public class InspectorTuple<T1> : InspectorBase
    {
        private readonly DynamicFetcher _fetcher1;

        /// <summary>
        /// Item Value for Name1
        /// </summary>
        public T1 Item1
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (T1)_fetcher1!.Fetch(Instance)!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _fetcher1.Shove(Instance, value);
        }

        /// <summary>
        /// Invokes item1 as a method
        /// </summary>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Method return value</returns>
        public T1 InvokeItem1(params object[] parameters)
            => (T1)_fetcher1.Invoke(Instance, parameters)!;

        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        public InspectorTuple(string name1)
        {
            _fetcher1 = new DynamicFetcher(name1);
        }
        
        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        public InspectorTuple(InspectName name1)
        {
            _fetcher1 = new DynamicFetcher(name1);
        }
    }

    /// <summary>
    /// Tuple to inspect an object instance
    /// </summary>
    public class InspectorTuple<T1, T2> : InspectorTuple<T1>
    {
        private readonly DynamicFetcher _fetcher2;

        /// <summary>
        /// Item Value for Name2
        /// </summary>
        public T2 Item2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (T2)_fetcher2!.Fetch(Instance)!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _fetcher2.Shove(Instance, value);
        }

        /// <summary>
        /// Invokes item2 as a method
        /// </summary>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Method return value</returns>
        public T2 InvokeItem2(params object[] parameters)
            => (T2)_fetcher2.Invoke(Instance, parameters)!;
        
        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        public InspectorTuple(string name1, string name2) : base(name1)
        {
            _fetcher2 = new DynamicFetcher(name2);
        }
        
        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        public InspectorTuple(InspectName name1, InspectName name2) : base(name1)
        {
            _fetcher2 = new DynamicFetcher(name2);
        }
    }

    /// <summary>
    /// Tuple to inspect an object instance
    /// </summary>
    public class InspectorTuple<T1, T2, T3> : InspectorTuple<T1, T2>
    {
        private readonly DynamicFetcher _fetcher3;

        /// <summary>
        /// Item Value for Name2
        /// </summary>
        public T3 Item3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (T3)_fetcher3!.Fetch(Instance)!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _fetcher3.Shove(Instance, value);
        }

        /// <summary>
        /// Invokes item3 as a method
        /// </summary>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Method return value</returns>
        public T3 InvokeItem3(params object[] parameters)
            => (T3)_fetcher3.Invoke(Instance, parameters)!;

        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        public InspectorTuple(string name1, string name2, string name3) : base(name1, name2)
        {
            _fetcher3 = new DynamicFetcher(name3);
        }
        
        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        public InspectorTuple(InspectName name1, InspectName name2, InspectName name3) : base(name1, name2)
        {
            _fetcher3 = new DynamicFetcher(name3);
        }
    }

    /// <summary>
    /// Tuple to inspect an object instance
    /// </summary>
    public class InspectorTuple<T1, T2, T3, T4> : InspectorTuple<T1, T2, T3>
    {
        private readonly DynamicFetcher _fetcher4;

        /// <summary>
        /// Item Value for Name2
        /// </summary>
        public T4 Item4
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (T4)_fetcher4!.Fetch(Instance)!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _fetcher4.Shove(Instance, value);
        }

        /// <summary>
        /// Invokes item4 as a method
        /// </summary>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Method return value</returns>
        public T4 InvokeItem4(params object[] parameters)
            => (T4)_fetcher4.Invoke(Instance, parameters)!;

        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        /// <param name="name4">Property or field name 4</param>
        public InspectorTuple(string name1, string name2, string name3, string name4) : base(name1, name2, name3)
        {
            _fetcher4 = new DynamicFetcher(name4);
        }
        
        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        /// <param name="name4">Property or field name 4</param>
        public InspectorTuple(InspectName name1, InspectName name2, InspectName name3, InspectName name4) : base(name1, name2, name3)
        {
            _fetcher4 = new DynamicFetcher(name4);
        }
    }

    /// <summary>
    /// Tuple to inspect an object instance
    /// </summary>
    public class InspectorTuple<T1, T2, T3, T4, T5> : InspectorTuple<T1, T2, T3, T4>
    {
        private readonly DynamicFetcher _fetcher5;

        /// <summary>
        /// Item Value for Name2
        /// </summary>
        public T5 Item5
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (T5)_fetcher5!.Fetch(Instance)!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _fetcher5.Shove(Instance, value);
        }

        /// <summary>
        /// Invokes item5 as a method
        /// </summary>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Method return value</returns>
        public T5 InvokeItem5(params object[] parameters)
            => (T5)_fetcher5.Invoke(Instance, parameters)!;

        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        /// <param name="name4">Property or field name 4</param>
        /// <param name="name5">Property or field name 5</param>
        public InspectorTuple(string name1, string name2, string name3, string name4, string name5) : base(name1, name2, name3, name4)
        {
            _fetcher5 = new DynamicFetcher(name5);
        }
        
        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        /// <param name="name4">Property or field name 4</param>
        /// <param name="name5">Property or field name 5</param>
        public InspectorTuple(InspectName name1, InspectName name2, InspectName name3, InspectName name4, InspectName name5) : base(name1, name2, name3, name4)
        {
            _fetcher5 = new DynamicFetcher(name5);
        }
    }

    /// <summary>
    /// Tuple to inspect an object instance
    /// </summary>
    public class InspectorTuple<T1, T2, T3, T4, T5, T6> : InspectorTuple<T1, T2, T3, T4, T5>
    {
        private readonly DynamicFetcher _fetcher6;

        /// <summary>
        /// Item Value for Name2
        /// </summary>
        public T6 Item6
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (T6)_fetcher6!.Fetch(Instance)!;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _fetcher6.Shove(Instance, value);
        }

        /// <summary>
        /// Invokes item6 as a method
        /// </summary>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Method return value</returns>
        public T6 InvokeItem6(params object[] parameters)
            => (T6)_fetcher6.Invoke(Instance, parameters)!;

        /// <summary>
        /// Creates a new inspector tuple for an object
        /// </summary>
        /// <param name="name1">Property or field name 1</param>
        /// <param name="name2">Property or field name 2</param>
        /// <param name="name3">Property or field name 3</param>
        /// <param name="name4">Property or field name 4</param>
        /// <param name="name5">Property or field name 5</param>
        /// <param name="name6">Property or field name 6</param>
        public InspectorTuple(string name1, string name2, string name3, string name4, string name5, string name6) : base(name1, name2, name3, name4, name5)
        {
            _fetcher6 = new DynamicFetcher(name6);
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
        public InspectorTuple(InspectName name1, InspectName name2, InspectName name3, InspectName name4, InspectName name5, InspectName name6) : base(name1, name2, name3, name4, name5)
        {
            _fetcher6 = new DynamicFetcher(name6);
        }
    }
}