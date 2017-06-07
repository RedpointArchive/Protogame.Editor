using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Protoinject;

namespace Protogame.Editor.Api.Version1
{
    public class MarshalledKernel : MarshalByRefObject, IKernel
    {
        private readonly IKernel _realKernel;

        public MarshalledKernel(IKernel realKernel)
        {
            _realKernel = realKernel;
        }

        public IHierarchy Hierarchy
        {
            get
            {
                return _realKernel.Hierarchy;
            }
        }

        public IBindToInScopeWithDescendantFilterOrUniqueOrNamed<TInterface> Bind<TInterface>()
        {
            return _realKernel.Bind<TInterface>();
        }

        public IBindToInScopeWithDescendantFilterOrUniqueOrNamed Bind(Type @interface)
        {
            return _realKernel.Bind(@interface);
        }

        public INode CreateEmptyNode(string name, INode parent = null)
        {
            return _realKernel.CreateEmptyNode(name, parent);
        }

        public IScope CreateScopeFromNode(INode node)
        {
            return _realKernel.CreateScopeFromNode(node);
        }

        public void Discard<T>(IPlan<T> plan)
        {
            _realKernel.Discard(plan);
        }

        public void Discard(IPlan plan)
        {
            _realKernel.Discard(plan);
        }

        public void DiscardAll<T>(IPlan<T>[] plans)
        {
            _realKernel.DiscardAll(plans);
        }

        public void DiscardAll(IPlan[] plans)
        {
            _realKernel.DiscardAll(plans);
        }

        public Task DiscardAllAsync<T>(IPlan<T>[] plans)
        {
            return _realKernel.DiscardAllAsync(plans);
        }

        public Task DiscardAllAsync(IPlan[] plans)
        {
            return _realKernel.DiscardAllAsync(plans);
        }

        public Task DiscardAsync<T>(IPlan<T> plan)
        {
            return _realKernel.DiscardAsync(plan);
        }

        public Task DiscardAsync(IPlan plan)
        {
            return _realKernel.DiscardAsync(plan);
        }

        public T Get<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.Get<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public object Get(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.Get(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public T[] GetAll<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.GetAll<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public object[] GetAll(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.GetAll(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public Task<T[]> GetAllAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.GetAllAsync<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public Task<object[]> GetAllAsync(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.GetAllAsync(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public Task<T> GetAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.GetAsync<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public Task<object> GetAsync(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.GetAsync(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public void Load<T>() where T : IProtoinjectModule
        {
            _realKernel.Load<T>();
        }

        public void Load(IProtoinjectModule module)
        {
            _realKernel.Load(module);
        }

        public IPlan<T> Plan<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.Plan<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public IPlan Plan(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.Plan(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public IPlan<T> Plan<T>(INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.Plan<T>(current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
        }

        public IPlan Plan(Type type, INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.Plan(type, current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
        }

        public IPlan<T>[] PlanAll<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.PlanAll<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public IPlan[] PlanAll(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.PlanAll(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public IPlan<T>[] PlanAll<T>(INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.PlanAll<T>(current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
        }

        public IPlan[] PlanAll(Type type, INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.PlanAll(type, current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
        }

        public Task<IPlan<T>[]> PlanAllAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.PlanAllAsync<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public Task<IPlan[]> PlanAllAsync(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.PlanAllAsync(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public Task<IPlan<T>[]> PlanAllAsync<T>(INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.PlanAllAsync<T>(current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
        }

        public Task<IPlan[]> PlanAllAsync(Type type, INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.PlanAllAsync(type, current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
        }

        public Task<IPlan<T>> PlanAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.PlanAsync<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public Task<IPlan> PlanAsync(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.PlanAsync(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public Task<IPlan<T>> PlanAsync<T>(INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.PlanAsync<T>(current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
        }

        public Task<IPlan> PlanAsync(Type type, INode current, string bindingName, string planName, INode planRoot, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.PlanAsync(type, current, bindingName, planName, planRoot, injectionAttributes, arguments, transientBindings);
        }

        public T Resolve<T>(IPlan<T> plan)
        {
            return _realKernel.Resolve(plan);
        }

        public object Resolve(IPlan plan)
        {
            return _realKernel.Resolve(plan);
        }

        public T[] ResolveAll<T>(IPlan<T>[] plans)
        {
            return _realKernel.ResolveAll(plans);
        }

        public object[] ResolveAll(IPlan[] plans)
        {
            return _realKernel.ResolveAll(plans);
        }

        public Task<T[]> ResolveAllAsync<T>(IPlan<T>[] plans)
        {
            return _realKernel.ResolveAllAsync(plans);
        }

        public Task<object[]> ResolveAllAsync(IPlan[] plans)
        {
            return _realKernel.ResolveAllAsync(plans);
        }

        public INode<T>[] ResolveAllToNode<T>(IPlan<T>[] plans)
        {
            return _realKernel.ResolveAllToNode(plans);
        }

        public INode[] ResolveAllToNode(IPlan[] plans)
        {
            return _realKernel.ResolveAllToNode(plans);
        }

        public Task<INode<T>[]> ResolveAllToNodeAsync<T>(IPlan<T>[] plans)
        {
            return _realKernel.ResolveAllToNodeAsync(plans);
        }

        public Task<INode[]> ResolveAllToNodeAsync(IPlan[] plans)
        {
            return _realKernel.ResolveAllToNodeAsync(plans);
        }

        public Task<T> ResolveAsync<T>(IPlan<T> plan)
        {
            return _realKernel.ResolveAsync(plan);
        }

        public Task<object> ResolveAsync(IPlan plan)
        {
            return _realKernel.ResolveAsync(plan);
        }

        public INode<T> ResolveToNode<T>(IPlan<T> plan)
        {
            return _realKernel.ResolveToNode(plan);
        }

        public INode ResolveToNode(IPlan plan)
        {
            return _realKernel.ResolveToNode(plan);
        }

        public Task<INode<T>> ResolveToNodeAsync<T>(IPlan<T> plan)
        {
            return _realKernel.ResolveToNodeAsync(plan);
        }

        public Task<INode> ResolveToNodeAsync(IPlan plan)
        {
            return _realKernel.ResolveToNodeAsync(plan);
        }

        public T TryGet<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.TryGet<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public object TryGet(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.TryGet(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public Task<T> TryGetAsync<T>(INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.TryGetAsync<T>(current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public Task<object> TryGetAsync(Type type, INode current, string bindingName, string planName, IInjectionAttribute[] injectionAttributes, IConstructorArgument[] arguments, Dictionary<Type, List<IMapping>> transientBindings)
        {
            return _realKernel.TryGetAsync(type, current, bindingName, planName, injectionAttributes, arguments, transientBindings);
        }

        public void Unbind<T>()
        {
            _realKernel.Unbind<T>();
        }

        public void Unbind(Type @interface)
        {
            _realKernel.Unbind(@interface);
        }

        public void Validate<T>(IPlan<T> plan)
        {
            _realKernel.Validate(plan);
        }

        public void Validate(IPlan plan)
        {
            _realKernel.Validate(plan);
        }

        public void ValidateAll<T>(IPlan<T>[] plans)
        {
            _realKernel.ValidateAll(plans);
        }

        public void ValidateAll(IPlan[] plans)
        {
            _realKernel.ValidateAll(plans);
        }

        public Task ValidateAllAsync<T>(IPlan<T>[] plans)
        {
            return _realKernel.ValidateAllAsync(plans);
        }

        public Task ValidateAllAsync(IPlan[] plans)
        {
            return _realKernel.ValidateAllAsync(plans);
        }

        public Task ValidateAsync<T>(IPlan<T> plan)
        {
            return _realKernel.ValidateAsync(plan);
        }

        public Task ValidateAsync(IPlan plan)
        {
            return _realKernel.ValidateAsync(plan);
        }
    }
}
