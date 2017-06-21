using System.Threading.Tasks;
using Grpc.Core;
using Protogame.Editor.Grpc.ExtensionHost;
using Protoinject;
using System.Linq;
using Protogame.Editor.Api.Version1.Toolbar;

namespace Protogame.Editor.ExtHost
{
    public class ToolbarEntriesImpl : ToolbarEntries.ToolbarEntriesBase
    {
        private readonly IToolbarProvider[] _toolbarProvider;

        public ToolbarEntriesImpl(IToolbarProvider[] toolbarProvider)
        {
            _toolbarProvider = toolbarProvider;
        }

        public override async Task<GetToolbarItemsResponse> GetToolbarItems(GetToolbarItemsRequest request, ServerCallContext context)
        {
            var resp = new GetToolbarItemsResponse();

            if (_toolbarProvider == null)
            {
                return resp;
            }

            foreach (var mp in _toolbarProvider)
            {
                resp.ToolbarItems.AddRange(mp.GetToolbarItems().Select(x => new Grpc.ExtensionHost.GenericToolbarItem
                {
                    Id = x.Id.GetHashCode(),
                    Icon = x.Icon,
                    Toggled = x.Toggled,
                    Enabled = x.Enabled,
                }));
            }

            return resp;
        }

        public override Task<ToolbarItemClickedResponse> ToolbarItemClicked(ToolbarItemClickedRequest request, ServerCallContext context)
        {
            if (_toolbarProvider != null)
            {
                foreach (var mp in _toolbarProvider)
                {
                    foreach (var me in mp.GetToolbarItems())
                    {
                        if (request.ToolbarId == me.Id.GetHashCode())
                        {
                            me.Handler?.Invoke(me);
                            return Task.FromResult(new ToolbarItemClickedResponse());
                        }
                    }
                }
            }

            return Task.FromResult(new ToolbarItemClickedResponse());
        }
    }
}
