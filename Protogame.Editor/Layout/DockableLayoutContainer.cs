using System;
using Microsoft.Xna.Framework;
using Protogame;
using System.Collections.Generic;
using System.Linq;

namespace Protogame.Editor.Layout
{
    /// <summary>
    /// A layout container where <see cref="DockableContainer"/>s can be placed.  Each dockable layout container
    /// nine regions; an outer set of regions (top, left, bottom, right) and an inner tabbed region.
    /// </summary>
    public class DockableLayoutContainer : IDockableContainer, ITabbableContainer
    {
        private readonly List<ITabbableContainer> _innerRegions;

        const int _tabHeight = 20;
        const int _regionSpacing = 2;

        public DockableLayoutContainer()
        {
            Title = string.Empty;
            LeftWidth = 200;
            RightWidth = 200;
            TopHeight = 200;
            BottomHeight = 200;
            _innerRegions = new List<ITabbableContainer>();
        }

        public string Title { get; set; }
        public IAssetReference<TextureAsset> Icon { get; set; }

        public IDockableContainer LeftRegion { get; private set; }
        public IDockableContainer RightRegion { get; private set; }
        public IDockableContainer TopRegion { get; private set; }
        public IDockableContainer BottomRegion { get; private set; }
        public ITabbableContainer[] InnerRegions => _innerRegions.ToArray();
        public int ActiveTabIndex { get; set; }

        public int LeftWidth { get; set; }
        public int RightWidth { get; set; }
        public int TopHeight { get; set; }
        public int BottomHeight { get; set; }

        public IContainer[] Children
        {
            get
            {
                var containers = new List<IContainer>();
                if (LeftRegion != null)
                {
                    containers.Add(LeftRegion);
                }
                if (RightRegion != null)
                {
                    containers.Add(RightRegion);
                }
                if (TopRegion != null)
                {
                    containers.Add(TopRegion);
                }
                if (BottomRegion != null)
                {
                    containers.Add(BottomRegion);
                }

                if (ActiveTabIndex >= 0 && ActiveTabIndex < _innerRegions.Count)
                {
                    containers.Add(_innerRegions[ActiveTabIndex]);
                }

                return containers.ToArray();
            }
        }

        public bool Focused { get; set; }
        public int Order { get; set; }
        public IContainer Parent { get; set; }
        public object Userdata { get; set; }

        public IEnumerable<KeyValuePair<IContainer, Rectangle>> ChildrenWithLayouts(Rectangle layout)
        {
            var regionSize = layout;

            if (LeftRegion != null)
            {
                var leftRegionWidth = Math.Min(regionSize.Width, LeftWidth);

                yield return new KeyValuePair<IContainer, Rectangle>(
                    LeftRegion,
                    new Rectangle(
                        regionSize.X,
                        regionSize.Y,
                        leftRegionWidth,
                        regionSize.Height));
                regionSize = new Rectangle(
                    regionSize.X + leftRegionWidth + _regionSpacing,
                    regionSize.Y,
                    regionSize.Width - leftRegionWidth - _regionSpacing,
                    regionSize.Height);
            }
            
            if (RightRegion != null)
            {
                var rightRegionWidth = Math.Min(regionSize.Width, RightWidth);

                yield return new KeyValuePair<IContainer, Rectangle>(
                    RightRegion,
                    new Rectangle(
                        regionSize.X + regionSize.Width - rightRegionWidth,
                        regionSize.Y,
                        rightRegionWidth,
                        regionSize.Height));
                regionSize = new Rectangle(
                    regionSize.X,
                    regionSize.Y,
                    regionSize.Width - rightRegionWidth - _regionSpacing,
                    regionSize.Height);
            }

            if (TopRegion != null)
            {
                var topRegionHeight = Math.Min(regionSize.Height, TopHeight);

                yield return new KeyValuePair<IContainer, Rectangle>(
                    TopRegion,
                    new Rectangle(
                        regionSize.X,
                        regionSize.Y,
                        regionSize.Width,
                        topRegionHeight));
                regionSize = new Rectangle(
                    regionSize.X,
                    regionSize.Y + topRegionHeight + _regionSpacing,
                    regionSize.Width,
                    regionSize.Height - topRegionHeight - _regionSpacing);
            }

            if (BottomRegion != null)
            {
                var bottomRegionHeight = Math.Min(regionSize.Height, BottomHeight);

                yield return new KeyValuePair<IContainer, Rectangle>(
                    BottomRegion,
                    new Rectangle(
                        regionSize.X,
                        regionSize.Y + regionSize.Height - bottomRegionHeight,
                        regionSize.Width,
                        bottomRegionHeight));
                regionSize = new Rectangle(
                    regionSize.X,
                    regionSize.Y,
                    regionSize.Width,
                    regionSize.Height - bottomRegionHeight - _regionSpacing);
            }

            if (ActiveTabIndex >= 0 && ActiveTabIndex < _innerRegions.Count)
            {
                var activeTabContainer = _innerRegions[ActiveTabIndex];
                var activeTabDockableLayoutContainer = activeTabContainer as DockableLayoutContainer;

                if (activeTabDockableLayoutContainer != null && _innerRegions.Count == 1)
                {
                    // Only one child and it is a dockable region itself, so do not render tabs.
                    yield return new KeyValuePair<IContainer, Rectangle>(
                        _innerRegions[ActiveTabIndex],
                        new Rectangle(
                            regionSize.X,
                            regionSize.Y,
                            regionSize.Width,
                            regionSize.Height));
                }
                else
                {
                    // Leave some room for rendering tabs.  Should this involve skin configuration?
                    yield return new KeyValuePair<IContainer, Rectangle>(
                        _innerRegions[ActiveTabIndex],
                        new Rectangle(
                            regionSize.X,
                            regionSize.Y + _tabHeight,
                            regionSize.Width,
                            regionSize.Height - _tabHeight));
                }
            }
        }

        public class TabForRendering
        {
            public int Index { get; set; }

            public Rectangle Layout { get; set; }

            public string Title { get; set; }

            public IAssetReference<TextureAsset> Icon { get; set; }

            public bool IsActive { get; set; }
        }

        public IEnumerable<TabForRendering> TabWithLayouts(Rectangle layout)
        {
            var regionSize = layout;

            if (LeftRegion != null)
            {
                var leftRegionWidth = Math.Min(regionSize.Width, LeftWidth);
                
                regionSize = new Rectangle(
                    regionSize.X + leftRegionWidth + _regionSpacing,
                    regionSize.Y,
                    regionSize.Width - leftRegionWidth - _regionSpacing,
                    regionSize.Height);
            }

            if (RightRegion != null)
            {
                var rightRegionWidth = Math.Min(regionSize.Width, RightWidth);
                
                regionSize = new Rectangle(
                    regionSize.X,
                    regionSize.Y,
                    regionSize.Width - rightRegionWidth - _regionSpacing,
                    regionSize.Height);
            }

            if (TopRegion != null)
            {
                var topRegionHeight = Math.Min(regionSize.Height, TopHeight);
                
                regionSize = new Rectangle(
                    regionSize.X,
                    regionSize.Y + topRegionHeight + _regionSpacing,
                    regionSize.Width,
                    regionSize.Height - topRegionHeight - _regionSpacing);
            }

            if (BottomRegion != null)
            {
                var bottomRegionHeight = Math.Min(regionSize.Height, BottomHeight);
                
                regionSize = new Rectangle(
                    regionSize.X,
                    regionSize.Y,
                    regionSize.Width,
                    regionSize.Height - bottomRegionHeight - _regionSpacing);
            }

            if (ActiveTabIndex >= 0 && ActiveTabIndex < _innerRegions.Count)
            {
                var activeTabContainer = _innerRegions[ActiveTabIndex];
                var activeTabDockableLayoutContainer = activeTabContainer as DockableLayoutContainer;

                if (activeTabDockableLayoutContainer != null && _innerRegions.Count == 1)
                {
                    // No tabs rendered for this.
                }
                else
                {
                    // Yield all of the tabs.
                    for (var i = 0; i < _innerRegions.Count; i++)
                    {
                        // TODO: Measure width of tabs.

                        yield return new TabForRendering
                        {
                            Index = i,
                            Title = _innerRegions[i].Title,
                            Icon = _innerRegions[i].Icon,
                            Layout = new Rectangle(
                                regionSize.X + (i * 100),
                                regionSize.Y + 4,
                                100,
                                _tabHeight - 3),
                            IsActive = ActiveTabIndex == i
                        };
                    }
                }
            }
        }

        public void SetLeftRegion(IDockableContainer child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            if (child.Parent != null)
            {
                throw new InvalidOperationException();
            }

            LeftRegion = child;
            LeftRegion.Parent = this;
        }

        public void SetRightRegion(IDockableContainer child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            if (child.Parent != null)
            {
                throw new InvalidOperationException();
            }

            RightRegion = child;
            RightRegion.Parent = this;
        }

        public void SetTopRegion(IDockableContainer child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            if (child.Parent != null)
            {
                throw new InvalidOperationException();
            }

            TopRegion = child;
            TopRegion.Parent = this;
        }

        public void SetBottomRegion(IDockableContainer child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            if (child.Parent != null)
            {
                throw new InvalidOperationException();
            }

            BottomRegion = child;
            BottomRegion.Parent = this;
        }

        public void AddInnerRegion(ITabbableContainer child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }

            if (child.Parent != null)
            {
                throw new InvalidOperationException();
            }

            _innerRegions.Add(child);
            child.Parent = this;
        }

        public bool HandleEvent(ISkinLayout skinLayout, Rectangle layout, IGameContext context, Event @event)
        {
            foreach (var tab in TabWithLayouts(layout))
            {
                var mousePressEvent = @event as MousePressEvent;
                if (mousePressEvent != null)
                {
                    if (tab.Layout.Contains(mousePressEvent.MouseState.Position))
                    {
                        ActiveTabIndex = tab.Index;
                    }
                }
            }

            foreach (var kv in ChildrenWithLayouts(layout))
            {
                if (kv.Key.HandleEvent(skinLayout, kv.Value, context, @event))
                {
                    return true;
                }
            }

            return false;
        }

        public void Render(IRenderContext context, ISkinLayout skinLayout, ISkinDelegator skinDelegator, Rectangle layout)
        {
            skinDelegator.Render(context, layout, this);
            foreach (var kv in this.ChildrenWithLayouts(layout).OrderByDescending(x => x.Key.Order))
            {
                kv.Key.Render(context, skinLayout, skinDelegator, kv.Value);
            }
        }

        public void Update(ISkinLayout skinLayout, Rectangle layout, GameTime gameTime, ref bool stealFocus)
        {
            foreach (var kv in ChildrenWithLayouts(layout))
            {
                kv.Key.Update(skinLayout, kv.Value, gameTime, ref stealFocus);
                if (stealFocus)
                {
                    break;
                }
            }
        }
    }
}
