#if PLATFORM_WINDOWS

using Protogame;
using System.Linq;
using System;
using Form = System.Windows.Forms.Form;
using MainMenu = System.Windows.Forms.MainMenu;
using MenuItem = System.Windows.Forms.MenuItem;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ProtogameUIStylingTest
{
    public class WindowsMainMenuController : IMainMenuController
    {
        private readonly IMenuProvider[] _menuProviders;
        private IGameContext _gameContext;

        public WindowsMainMenuController(IMenuProvider[] menuProviders)
        {
            _menuProviders = menuProviders;
        }

        private class MenuItemTag
        {
            public string Text { get; set; }

            public int? Order { get; set; }

            public bool RegisteredClick { get; set; }
        }

        public void Update(IGameContext gameContext, IUpdateContext updateContext)
        {
            _gameContext = gameContext;

            var menuEntries = _menuProviders.SelectMany(x => x.GetMenuItems());

            var menuStrip = CreateMainMenuControlIfNecessary(gameContext);

            var existingMenuItems = new Dictionary<string, MenuItem>();

            // Add menu items.
            foreach (var menuItem in menuStrip.MenuItems.OfType<MenuItem>())
            {
                AddMenuItems(existingMenuItems, string.Empty, menuItem);
            }

            // Configure menu items.
            foreach (var menuEntry in menuEntries)
            {
                MenuItem menuItem;
                if (existingMenuItems.ContainsKey(menuEntry.Path))
                {
                    menuItem = existingMenuItems[menuEntry.Path];
                }
                else
                {
                    var components = menuEntry.Path.Split('/');
                    menuItem = BuildMenuItemPath(menuStrip, components, menuEntry.Order);
                }

                if (menuItem.Tag == null)
                {
                    menuItem.Tag = new MenuItemTag();
                }
                var menuItemTag = (MenuItemTag)menuItem.Tag;
                menuItemTag.Text = menuEntry.Path.Split('/').Last();
                menuItemTag.Order = menuEntry.Order;
                menuItem.Text = menuEntry.DynamicTextHandler != null ? menuEntry.DynamicTextHandler(menuEntry) : menuEntry.Path.Split('/').Last();
                menuItem.Enabled = menuEntry.DynamicEnabledHandler != null ? menuEntry.DynamicEnabledHandler(menuEntry) : menuEntry.Enabled;
                if (!menuItemTag.RegisteredClick)
                {
                    menuItem.Click += (sender, e) =>
                    {
                        menuEntry.Handler(_gameContext, menuEntry);
                    };
                    menuItemTag.RegisteredClick = true;
                }
            }

            // Add menu seperators.
            foreach (var menuItem in menuStrip.MenuItems.OfType<MenuItem>())
            {
                UpdateMenuSeperators(menuItem);
            }
        }

        private void UpdateMenuSeperators(MenuItem menuItem)
        {
            var lastOrder = 0;
            var index = 0;
            var didHaveSeperator = false;
            foreach (var child in menuItem.MenuItems.OfType<MenuItem>())
            {
                var tag = child.Tag as MenuItemTag;
                if (tag == null && child.Text == "-")
                {
                    // Seperator already exists here.
                    index++;
                    didHaveSeperator = true;
                    continue;
                }

                if (tag.Order == null)
                {
                    continue;
                }

                if (tag.Order.Value / 100 != lastOrder / 100)
                {
                    // Insert menu seperator.
                    if (!didHaveSeperator)
                    {
                        menuItem.MenuItems.Add(index, new MenuItem("-"));
                    }
                    didHaveSeperator = true;
                }
                else
                {
                    didHaveSeperator = false;
                }

                lastOrder = tag.Order.Value;

                index++;
            }
        }

        private MenuItem BuildMenuItemPath(MainMenu menuStrip, string[] components, int? lastOrder)
        {
            if (components.Length == 0)
            {
                return menuStrip.MenuItems[0];
            }

            foreach (var mi in menuStrip.MenuItems.OfType<MenuItem>())
            {
                var tagName = (MenuItemTag)mi.Tag;
                if (tagName.Text == components[0])
                {
                    return BuildMenuItemPath(mi, components.Skip(1).ToArray(), lastOrder);
                }
            }

            var mii = new MenuItem();
            mii.Tag = new MenuItemTag { Text = components[0], Order = null };
            mii.Text = components[0];

            if (components.Length == 1 && lastOrder.HasValue)
            {
                var targetIndex = -1;
                var orderedMenuItems = menuStrip.MenuItems.OfType<MenuItem>().OrderBy(x => ((MenuItemTag)x.Tag).Order ?? 100000).ToArray();
                for (var i = 0; i < orderedMenuItems.Length; i++)
                {
                    var tag = (MenuItemTag)orderedMenuItems[i].Tag;

                    if (tag.Order != null && tag.Order.Value < lastOrder.Value)
                    {
                        targetIndex = i;
                    }
                }
                
                menuStrip.MenuItems.Add(targetIndex + 1, mii);
            }
            else
            {
                menuStrip.MenuItems.Add(mii);
            }

            return BuildMenuItemPath(mii, components.Skip(1).ToArray(), lastOrder);
        }

        private MenuItem BuildMenuItemPath(MenuItem menuItemParent, string[] components, int? lastOrder)
        {
            if (components.Length == 0)
            {
                return menuItemParent;
            }

            foreach (var mi in menuItemParent.MenuItems.OfType<MenuItem>())
            {
                var tagName = (MenuItemTag)mi.Tag;
                if (tagName.Text == components[0])
                {
                    return BuildMenuItemPath(mi, components.Skip(1).ToArray(), lastOrder);
                }
            }

            var mii = new MenuItem();
            mii.Tag = new MenuItemTag { Text = components[0], Order = null };
            mii.Text = components[0];

            if (components.Length == 1 && lastOrder.HasValue)
            {
                var targetIndex = -1;
                var orderedMenuItems = menuItemParent.MenuItems.OfType<MenuItem>().OrderBy(x => ((MenuItemTag)x.Tag).Order ?? 100000).ToArray();
                for (var i = 0; i < orderedMenuItems.Length; i++)
                {
                    var tag = (MenuItemTag)orderedMenuItems[i].Tag;

                    if (tag.Order != null && tag.Order.Value < lastOrder.Value)
                    {
                        targetIndex = i;
                    }
                }
                
                menuItemParent.MenuItems.Add(targetIndex + 1, mii);
            }
            else
            {
                menuItemParent.MenuItems.Add(mii);
            }

            return BuildMenuItemPath(mii, components.Skip(1).ToArray(), lastOrder);
        }

        private void AddMenuItems(Dictionary<string, MenuItem> menuItems, string parentPath, MenuItem node)
        {
            var tag = node.Tag as MenuItemTag;
            if (node.Tag == null)
            {
                // Must be a seperator?
                return;
            }

            menuItems.Add((parentPath + "/" + tag.Text).TrimStart('/'), node);
            foreach (var child in node.MenuItems.OfType<MenuItem>())
            {
                AddMenuItems(menuItems, (parentPath + "/" + tag.Text).TrimStart('/'), child);
            }
        }

        private MainMenu CreateMainMenuControlIfNecessary(IGameContext gameContext)
        {
            var form = (Form)Form.FromHandle(gameContext.Window.PlatformWindow.Handle);
            
            if (form.Menu != null)
            {
                return form.Menu;
            }

            var menuStrip = new MainMenu();
            form.Menu = menuStrip;
            return menuStrip;
        }
    }
}

#endif