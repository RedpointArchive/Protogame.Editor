using Google.Protobuf.Collections;
using Grpc.Core;
using Protogame.Editor.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Protogame.Editor.Menu
{
    public class ExtensionBasedMenuProvider : IMenuProvider
    {
        private readonly IExtensionManager _extensionManager;
        private Task _updateMenuItems;
        private MenuEntry[] _menuItems;

        public ExtensionBasedMenuProvider(IExtensionManager extensionManager)
        {
            _extensionManager = extensionManager;
            _menuItems = new MenuEntry[0];
        }

        public MenuEntry[] GetMenuItems()
        {
            if (_updateMenuItems == null || _updateMenuItems.IsCompleted)
            {
                _updateMenuItems = Task.Run(async () =>
                {
                    var items = new List<MenuEntry>();
                    foreach (var ext in _extensionManager.Extensions)
                    {
                        var client = ext.GetClient<Grpc.ExtensionHost.MenuEntries.MenuEntriesClient>();
                        RepeatedField<Grpc.ExtensionHost.MenuItem> rawItems;
                        try
                        {
                            rawItems = (await client.GetMenuItemsAsync(new Grpc.ExtensionHost.GetMenuItemsRequest(), new CallOptions(deadline: DateTime.UtcNow.AddMilliseconds(250))).ResponseAsync.ConfigureAwait(false)).MenuItems;
                        }
                        catch (Exception ex)
                        {
                            continue;
                        }
                        items.AddRange(rawItems.Select(y =>
                        {
                            MenuClickHandler clickCallback = e =>
                            {
                                client.MenuItemClicked(new Grpc.ExtensionHost.MenuItemClickedRequest { MenuItemId = y.Id });
                            };
                            return new MenuEntry(y.Path, y.Enabled, (int)y.Order, clickCallback, null);
                        }));
                    }
                    _menuItems = items.ToArray();
                });
            }

            return _menuItems;
        }
    }
}

