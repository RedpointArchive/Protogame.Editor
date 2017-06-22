using Google.Protobuf.Collections;
using Protogame.Editor.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Protogame.Editor.Toolbar
{
    public class ExtensionBasedToolbarProvider : IToolbarProvider
    {
        private readonly IExtensionManager _extensionManager;
        private Task _updateMenuItems;
        private GenericToolbarEntry[] _menuItems;
        private List<WeakReference<Extension.Extension>> _ignoredExtensions;
        private readonly IConsoleHandle _consoleHandle;

        public ExtensionBasedToolbarProvider(
            IConsoleHandle consoleHandle,
            IExtensionManager extensionManager)
        {
            _consoleHandle = consoleHandle;
            _extensionManager = extensionManager;
            _menuItems = new GenericToolbarEntry[0];
            _ignoredExtensions = new List<WeakReference<Extension.Extension>>();
        }

        public GenericToolbarEntry[] GetToolbarItems()
        {
            if (_updateMenuItems == null || _updateMenuItems.IsCompleted)
            {
                _updateMenuItems = Task.Run(async () =>
                {
                    var items = new List<GenericToolbarEntry>();
                    foreach (var ext in _extensionManager.Extensions)
                    {
                        var toRemove = new List<WeakReference<Extension.Extension>>();
                        if (_ignoredExtensions.Any(x =>
                        {
                            Extension.Extension oext;
                            if (x.TryGetTarget(out oext))
                            {
                                return oext == ext;
                            }
                            else
                            {
                                toRemove.Add(x);
                            }
                            return false;
                        }))
                        {
                            continue;
                        }
                        _ignoredExtensions.RemoveAll(toRemove.Contains);

                        var client = ext.GetClient<Grpc.ExtensionHost.ToolbarEntries.ToolbarEntriesClient>();
                        RepeatedField<Grpc.ExtensionHost.GenericToolbarItem> rawItems;
                        try
                        {
                            var result = await client.GetToolbarItemsAsync(new Grpc.ExtensionHost.GetToolbarItemsRequest());
                            rawItems = result.ToolbarItems;
                        }
                        catch (Exception ex)
                        {
                            _consoleHandle.LogError(ex);
                            _ignoredExtensions.Add(new WeakReference<Extension.Extension>(ext));
                            continue;
                        }
                        items.AddRange(rawItems.Select(y =>
                        {
                            ToolbarClickHandler clickCallback = e =>
                            {
                                Task.Run(async () =>
                                {
                                    try
                                    {
                                        await client.ToolbarItemClickedAsync(new Grpc.ExtensionHost.ToolbarItemClickedRequest { ToolbarId = y.Id });
                                    }
                                    catch (Exception ex)
                                    {
                                        _consoleHandle.LogError(ex);
                                    }
                                });
                            };
                            return new GenericToolbarEntry(y.Id, y.Icon, y.Toggled, y.Enabled, clickCallback, null);
                        }));
                    }
                    _menuItems = items.ToArray();
                    await Task.Delay(1000);
                });
            }

            return _menuItems;
        }
    }
}
