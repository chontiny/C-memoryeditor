namespace Squalr.Engine.Memory
{
    using Squalr.Engine.Memory.Clr;
    using Squalr.Engine.TaskScheduler;
    using Squalr.Engine.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Singleton class to resolve the address of managed objects in an external process.
    /// </summary>
    public class AddressResolver : ScheduledTask
    {
        /// <summary>
        /// Time in ms of how often to poll and resolve addresses initially.
        /// </summary>
        private const Int32 ResolveIntervalInitial = 200;

        /// <summary>
        /// Time in ms of how often to poll and re-resolve addresses.
        /// </summary>
        private const Int32 ResolveInterval = 5000;

        /// <summary>
        /// Singleton instance of the <see cref="AddressResolver" /> class.
        /// </summary>
        private static Lazy<AddressResolver> addressResolverInstance = new Lazy<AddressResolver>(
            () => { return new AddressResolver(); },
            LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Prevents a default instance of the <see cref="AddressResolver" /> class from being created.
        /// </summary>
        private AddressResolver() : base("Address Resolver", isRepeated: true, trackProgress: false)
        {
            this.DotNetNameMap = new Dictionary<String, DotNetObject>();
        }

        /// <summary>
        /// Gets or sets the mapping of object identifiers to their object.
        /// </summary>
        private Dictionary<String, DotNetObject> DotNetNameMap { get; set; }

        /// <summary>
        /// Gets a singleton instance of the <see cref="AddressResolver"/> class.
        /// </summary>
        /// <returns>A singleton instance of the class.</returns>
        public static AddressResolver GetInstance()
        {
            return AddressResolver.addressResolverInstance.Value;
        }

        /// <summary>
        /// Converts an address to a module and an address offset.
        /// </summary>
        /// <param name="address">The original address.</param>
        /// <param name="moduleName">The module name containing this address, if there is one. Otherwise, empty string.</param>
        /// <returns>The module name and address offset. If not contained by a module, the original address is returned.</returns>
        public UInt64 AddressToModule(UInt64 address, out String moduleName)
        {
            NormalizedModule containingModule = VirtualMemoryAdapterFactory.GetVirtualMemoryAdapter().GetModules()
                .Select(module => module)
                .Where(module => module.ContainsAddress(address))
                .FirstOrDefault();

            moduleName = containingModule?.Name ?? String.Empty;

            return containingModule == null ? address : address - containingModule.BaseAddress.ToUInt64();
        }

        /// <summary>
        /// Determines the base address of a module given a module name.
        /// </summary>
        /// <param name="identifier">The module identifier, or name.</param>
        /// <returns>The base address of the module.</returns>
        public IntPtr ResolveModule(String identifier)
        {
            IntPtr result = IntPtr.Zero;

            identifier = identifier?.RemoveSuffixes(true, ".exe", ".dll");
            IEnumerable<NormalizedModule> modules = VirtualMemoryAdapterFactory.GetVirtualMemoryAdapter().GetModules()
                ?.ToList()
                ?.Select(module => module)
                ?.Where(module => module.Name.RemoveSuffixes(true, ".exe", ".dll").Equals(identifier, StringComparison.OrdinalIgnoreCase));

            if (modules.Count() > 0)
            {
                result = modules.First().BaseAddress;
            }

            return result;
        }

        /// <summary>
        /// Determines the base address of a module given a global keyword.
        /// </summary>
        /// <param name="identifier">The global keyword identifier defined by a script.</param>
        /// <returns>The base address as defined by the keyword.</returns>
        public IntPtr ResolveGlobalKeyword(String identifier)
        {
            // Scripting.ScriptEngine scriptEngine = new Scripting.ScriptEngine();
            // return ((dynamic)scriptEngine.MemoryCore.GetGlobalKeyword(identifier))?.ToIntPtr() ?? IntPtr.Zero;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines the base address of a .Net object given an object identifier.
        /// </summary>
        /// <param name="identifier">The .Net object identifier, which is the full namespace path to the object.</param>
        /// <returns>The base address of the .Net object.</returns>
        public IntPtr ResolveDotNetObject(String identifier)
        {
            IntPtr result = IntPtr.Zero;
            DotNetObject dotNetObject;

            if (identifier == null)
            {
                return result;
            }

            if (this.DotNetNameMap.TryGetValue(identifier, out dotNetObject))
            {
                result = dotNetObject.ObjectReference.ToIntPtr();
            }

            return result;
        }

        /// <summary>
        /// Begins polling the external process for information needed to resolve addresses.
        /// </summary>
        protected override void OnBegin()
        {
            this.UpdateInterval = AddressResolver.ResolveIntervalInitial;
        }

        /// <summary>
        /// Polls the external process, gathering object information from the managed heap.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for handling canceled tasks.</param>
        protected override void OnUpdate(CancellationToken cancellationToken)
        {
            Dictionary<String, DotNetObject> nameMap = new Dictionary<String, DotNetObject>();
            List<DotNetObject> objectTrees = DotNetObjectCollector.GetInstance().ObjectTrees;

            // Build .NET object list
            objectTrees?.ForEach(x => this.BuildNameMap(nameMap, x));
            this.DotNetNameMap = nameMap;

            // After we have successfully grabbed information from the process, slow the update interval
            if (objectTrees != null)
            {
                this.UpdateInterval = AddressResolver.ResolveInterval;
            }
        }

        /// <summary>
        /// Called when the repeated task completes.
        /// </summary>
        protected override void OnEnd()
        {

        }

        /// <summary>
        /// Recursively updates the name map for a given object, mapping an identifier to a .Net object.
        /// </summary>
        /// <param name="nameMap">The name map being constructed</param>
        /// <param name="currentObject">The object to add</param>
        private void BuildNameMap(Dictionary<String, DotNetObject> nameMap, DotNetObject currentObject)
        {
            if (currentObject == null || currentObject.GetFullName() == null)
            {
                return;
            }

            nameMap[currentObject.GetFullName()] = currentObject;
            currentObject?.Children?.ForEach(x => this.BuildNameMap(nameMap, x));
        }
    }
    //// End class
}
//// End namespace